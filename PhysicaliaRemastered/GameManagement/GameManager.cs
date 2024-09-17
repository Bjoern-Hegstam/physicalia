using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Actors;
using PhysicaliaRemastered.Actors.EnemyManagement;
using PhysicaliaRemastered.Input;
using PhysicaliaRemastered.Pickups;
using PhysicaliaRemastered.Weapons;
using XNALibrary;
using XNALibrary.Animation;
using XNALibrary.Graphics;
using XNALibrary.Interfaces;
using XNALibrary.ParticleEngine;
using XNALibrary.Sprites;
using XNALibrary.TileEngine;

namespace PhysicaliaRemastered.GameManagement;

public enum GameState
{
    Start,
    Playing,
    End,
    Paused
}

public class GameManager
{
    private const string GamedataPath = "Content/GameData/";
    private const string LibraryPath = GamedataPath + "Libraries/";
    private const string WorldPath = GamedataPath + "Worlds/";

    private readonly Game _game;
    private readonly ITextureLibrary _textureLibrary;
    private readonly TileLibrary _tileLibrary;
    private readonly SpriteLibrary _spriteLibrary;
    private readonly ParticleEngine _particleEngine;
    private readonly AnimationManager _animationManager;
    private readonly EnemyBank _enemyBank;
    private readonly WeaponBank _weaponBank;
    private readonly PickupLibrary _modifierLibrary;

    private readonly List<World> _worlds;
    private int _worldIndex;

    private readonly Player _player;

    // Used for making sure pause mode is only entered when it's available
    private int _pausePressedCount;

    public GameState State { get; set; }

    public GameState NextState { get; set; }


    public ISettings Settings { get; }

    /// <summary>
    /// Creates a new GameManager.
    /// </summary>
    /// <param name="game">The game creating the GameManager. The Manager expects
    /// the Game to have an ISettings service.</param>
    public GameManager(Game game)
    {
        _game = game;

        // Create and add needed services
        IInputHandler input = (IInputHandler)_game.Services.GetService(typeof(IInputHandler));
        Settings = new Settings(input);
        _game.Services.AddService(typeof(ISettings), Settings);

        _textureLibrary = new TextureLibrary();
        _game.Services.AddService(typeof(ITextureLibrary), _textureLibrary);

        _spriteLibrary = new SpriteLibrary(_textureLibrary);
        _game.Services.AddService(typeof(ISpriteLibrary), _spriteLibrary);

        _animationManager = new AnimationManager(_game, _textureLibrary);
        _game.Services.AddService(typeof(IAnimationManager), _animationManager);
        _game.Components.Add(_animationManager);

        _tileLibrary = new TileLibrary();
        _game.Services.AddService(typeof(ITileLibrary), _tileLibrary);

        _particleEngine = new ParticleEngine();
        _game.Services.AddService(typeof(IParticleEngine), _particleEngine);

        _enemyBank = new EnemyBank(_animationManager);
        _game.Services.AddService(typeof(IEnemyBank), _enemyBank);

        _weaponBank = new WeaponBank(_particleEngine, _spriteLibrary, _animationManager);
        _game.Services.AddService(typeof(IWeaponBank), _weaponBank);

        _modifierLibrary = new PickupLibrary();
        _game.Services.AddService(typeof(IPickupLibrary), _modifierLibrary);

        _worlds = [];
        _worldIndex = -1;

        _player = new Player(Settings);

        NextState = State = GameState.Start;
    }

    /// <summary>
    /// Changes the state of the GameManager.
    /// </summary>
    private void ChangeState()
    {
        // Cleanup things that was done when we previously entered the current state
        switch (State)
        {
            case GameState.Start:
                break;
            case GameState.Playing:
                break;
            case GameState.End:
                break;
            case GameState.Paused:
                _animationManager.Enabled = true;
                break;
        }

        // Prepare for the next state
        switch (NextState)
        {
            case GameState.Start:
                break;
            case GameState.Playing:
                break;
            case GameState.End:
                break;
            case GameState.Paused:
                _animationManager.Enabled = false;
                break;
        }

        State = NextState;
    }

    public void ResetLevel()
    {
        if (_worldIndex < _worlds.Count)
        {
            _worlds[_worldIndex].ResetLevel();
            NextState = GameState.Playing;
            _pausePressedCount = 0;

            _player.Health = Settings.PlayerStartHealth;
            _player.Flickering = false;
        }
    }

    /// <summary>
    /// Makes the game ready for a new game session.
    /// </summary>
    public void NewSession()
    {
        // Make sure the animation manager is enabled
        _animationManager.Enabled = true;

        _worldIndex = 0;
        _worlds[_worldIndex].NewSession();

        _player.Health = Settings.PlayerStartHealth;
        // Give the player his default weapon
        _player.ClearWeapons();
        _player.AddWeapon(_weaponBank.GetWeapon(-1).Copy());

        State = NextState = GameState.Start;
    }

    /// <summary>
    /// Sets the state of the GameManager as specified by the session object.
    /// </summary>
    /// <param name="session">GameSession containing representing the
    /// wanted state of the GameManager.</param>
    public void LoadSession(GameSession session)
    {
        _worldIndex = session.WorldIndex;
        _player.Health = Settings.PlayerStartHealth;

        _player.LoadSession(session, _weaponBank);

        _worlds[_worldIndex].LoadSession(session);

        // Pause the game and ensure
        NextState = GameState.Paused;
        _pausePressedCount = 1;
    }

    /// <summary>
    /// Saves the current state of the GameManager to a GameSession object.
    /// </summary>
    /// <returns>A GameSession object representing the current state of
    /// the GameManager.</returns>
    public GameSession SaveSession()
    {
        NextState = GameState.Paused;

        GameSession session = new GameSession
        {
            WorldIndex = _worldIndex
        };

        _player.SaveSession(session);
        _worlds[_worldIndex].SaveSession(session);

        return session;
    }

    public void LoadContent(ContentManager contentManager)
    {
        Settings.LoadContent(contentManager);
    }

    /// <summary>
    /// Loads in a game as specified in the file indentified by the path.
    /// </summary>
    /// <param name="path">Path to the file containing the xml data to load.</param>
    public void LoadXml(string path)
    {
        XmlReaderSettings readerSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true
        };

        using XmlReader reader = XmlReader.Create(path, readerSettings);
        LoadXml(reader);
    }

    /// <summary>
    /// Loads in a game as specified by the xml data loaded by the reader.
    /// </summary>
    /// <param name="reader">XmlReader to be used for reading the xml data.</param>
    public void LoadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Settings")
            {
                Settings.LoadXml(GamedataPath + reader.ReadString(), _spriteLibrary);
            }

            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "TextureLibrary")
            {
                _textureLibrary.LoadXml(LibraryPath + reader.ReadString(), _game.GraphicsDevice);
            }

            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "SpriteLibrary")
            {
                _spriteLibrary.LoadXml(LibraryPath + reader.ReadString());
            }

            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "AnimationBank")
            {
                _animationManager.LoadXml(LibraryPath + reader.ReadString());
            }

            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "TileLibrary")
            {
                _tileLibrary.LoadXml(LibraryPath + reader.ReadString(), _spriteLibrary, _animationManager);
            }

            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "ParticleDefinitions")
            {
                _particleEngine.LoadXml(LibraryPath + reader.ReadString(), _spriteLibrary, _animationManager);

                // Prepare the particle engine to avoid slow downs (JIT)
                _particleEngine.Prepare();
            }

            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "WeaponBank")
            {
                _weaponBank.LoadXml(LibraryPath + reader.ReadString());
            }

            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "EnemyBank")
            {
                _enemyBank.LoadXml(LibraryPath + reader.ReadString());
            }

            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "PickupLibrary")
            {
                _modifierLibrary.LoadXml(LibraryPath + reader.ReadString(), _spriteLibrary);
            }

            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Player")
            {
                SetupPlayer(reader);
            }

            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Worlds")
            {
                LoadWorlds(reader);
            }
        }

        // Set a valid index if one or more worlds has been added
        if (_worlds.Count > 0)
        {
            _worldIndex = 0;
        }
    }

    private void LoadWorlds(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "World")
            {
                World world = new World(_game, _player);
                _worlds.Add(world);
                world.WorldIndex = _worlds.Count;
                world.LoadXml(WorldPath + reader.ReadString(), _tileLibrary, _spriteLibrary);
            }

            if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "Worlds")
            {
                return;
            }
        }
    }

    private void SetupPlayer(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "CollisionBox")
            {
                _player.CollisionBox = ReadRectangle(reader);
            }

            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Animation")
            {
                int animKey = int.Parse(reader.GetAttribute("key") ?? throw new ResourceLoadException());
                int action = int.Parse(reader.GetAttribute("action") ?? throw new ResourceLoadException());
                Animation anim = _animationManager.AddPlaybackAnimation(animKey);
                _player.AddAnimation(action, anim);
                _player.CurrentAnimationType = action;
            }

            if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "Player")
            {
                return;
            }
        }
    }

    private Rectangle ReadRectangle(XmlReader reader)
    {
        int x = int.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
        int y = int.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());
        int width = int.Parse(reader.GetAttribute("width") ?? throw new ResourceLoadException());
        int height = int.Parse(reader.GetAttribute("height") ?? throw new ResourceLoadException());

        return new Rectangle(x, y, width, height);
    }

    public void Update(GameTime gameTime)
    {
        switch (State)
        {
            case GameState.Start:
                if (Settings.InputMap.IsPressed(InputAction.MenuStart))
                {
                    NextState = GameState.Playing;
                }

                break;
            case GameState.Playing:
                // Has the Player finished the game?
                if (_worlds[_worldIndex].State == WorldState.Finished)
                {
                    // Go to next world when the player wants that
                    if (Settings.InputMap.IsPressed(InputAction.MenuStart))
                    {
                        _worldIndex++;
                    }

                    if (_worldIndex >= _worlds.Count)
                    {
                        // TODO: Congratulate player for winning the game
                        // Perhaps add some kind of special finish (Bonus level, etc.)
                        NextState = GameState.End;
                    }
                }
                else
                {
                    if (_worlds[_worldIndex].LevelState == LevelState.Start ||
                        _worlds[_worldIndex].LevelState == LevelState.Dead)
                    {
                        _pausePressedCount = 0;
                    }
                    else
                    {
                        _pausePressedCount = 1;
                    }

                    // Pause if start is pressed
                    // pausePressedCount is used as a workaround the fact that
                    // MenuStart will be pressed when a Level is start. The Level's
                    // state will then be set to LevelState.Playing which causes the
                    // Level to start paused.
                    if (_worlds[_worldIndex].LevelState == LevelState.Playing &&
                        Settings.InputMap.IsPressed(InputAction.MenuStart) && _pausePressedCount == 1)
                    {
                        NextState = GameState.Paused;
                    }

                    _worlds[_worldIndex].Update(gameTime);
                }

                break;
            case GameState.Paused:
                // Continue playing if the player wants to go back
                if (Settings.InputMap.IsPressed(InputAction.MenuBack))
                {
                    NextState = GameState.Playing;
                }

                break;
            case GameState.End:
                break;
        }

        if (NextState != State)
        {
            ChangeState();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        switch (State)
        {
            case GameState.Start:
                spriteBatch.DrawString(Settings.WorldQuoteFont, "And so it begins", new Vector2(130, 200), Color.White);
                break;
            case GameState.Playing:
                _worlds[_worldIndex].Draw(spriteBatch);
                break;
            case GameState.Paused:
                _worlds[_worldIndex].Draw(spriteBatch);
                break;
            case GameState.End:
                spriteBatch.DrawString(Settings.PlayerDeadFont, "Great Success!", new Vector2(150, 250), Color.White);
                break;
        }
    }
}