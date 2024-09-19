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
    private readonly Game _game;

    private readonly List<World> _worlds;
    private int _worldIndex;

    private readonly Player _player;

    private int _pausePressedCount;

    public GameState State { get; set; }
    public GameState NextState { get; set; }

    private Settings Settings => _game.Services.GetService<Settings>();
    private SpriteLibrary SpriteLibrary => _game.Services.GetService<SpriteLibrary>();
    private TileLibrary TileLibrary => _game.Services.GetService<TileLibrary>();
    private AnimationManager AnimationManager => _game.Services.GetService<AnimationManager>();
    private WeaponBank WeaponBank => _game.Services.GetService<WeaponBank>();
    private ParticleEngine ParticleEngine => _game.Services.GetService<ParticleEngine>();
    private EnemyBank EnemyBank => _game.Services.GetService<EnemyBank>();
    private PickupTemplateLibrary PickupTemplateLibrary => _game.Services.GetService<PickupTemplateLibrary>();

    public GameManager(Game game)
    {
        _game = game;

        _worlds = [];
        _worldIndex = -1;

        var settings = game.Services.GetService<Settings>();
        _player = new Player(settings);

        NextState = State = GameState.Start;
    }

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
                AnimationManager.Enabled = true;
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
                AnimationManager.Enabled = false;
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
        AnimationManager.Enabled = true;

        _worldIndex = 0;
        _worlds[_worldIndex].NewSession();

        _player.Health = Settings.PlayerStartHealth;
        // Give the player his default weapon
        _player.ClearWeapons();
        _player.AddWeapon(WeaponBank.GetWeapon(-1).Copy());

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

        _player.LoadSession(session, WeaponBank);

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

        var session = new GameSession
        {
            WorldIndex = _worldIndex
        };

        _player.SaveSession(session);
        _worlds[_worldIndex].SaveSession(session);

        return session;
    }
    
    public void LoadXml(string path, ContentManager contentManager)
    {
        var readerSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true
        };

        using var reader = XmlReader.Create(path, readerSettings);
        LoadXml(reader, contentManager);
    }

    public void LoadXml(XmlReader reader, ContentManager contentManager)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Settings" })
            {
                Settings.LoadXml(Environment.GameDataPath + reader.ReadString(), SpriteLibrary);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "SpriteLibrary" })
            {
                SpriteLibrary.LoadXml(Environment.LibraryPath + reader.ReadString(), contentManager);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "AnimationBank" })
            {
                AnimationManager.LoadXml(Environment.LibraryPath + reader.ReadString(), contentManager);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "TileLibrary" })
            {
                TileLibrary.LoadXml(Environment.LibraryPath + reader.ReadString(), SpriteLibrary, AnimationManager);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "ParticleDefinitions" })
            {
                ParticleEngine.LoadXml(Environment.LibraryPath + reader.ReadString(), SpriteLibrary, AnimationManager);

                // Prepare the particle engine to avoid slow downs (JIT)
                ParticleEngine.Prepare();
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "WeaponBank" })
            {
                WeaponBank.LoadXml(Environment.LibraryPath + reader.ReadString());
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "EnemyBank" })
            {
                EnemyBank.LoadXml(Environment.LibraryPath + reader.ReadString());
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "PickupLibrary" })
            {
                PickupTemplateLibrary.LoadXml(Environment.LibraryPath + reader.ReadString(), SpriteLibrary);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Player" })
            {
                SetupPlayer(reader);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Worlds" })
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
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "World" })
            {
                var world = new World(_game, _player);
                _worlds.Add(world);
                world.WorldIndex = _worlds.Count;
                world.LoadXml(Environment.WorldPath + reader.ReadString(), TileLibrary, SpriteLibrary);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Worlds" })
            {
                return;
            }
        }
    }

    private void SetupPlayer(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "CollisionBox" })
            {
                _player.CollisionBox = ReadRectangle(reader);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Animation" })
            {
                int animKey = int.Parse(reader.GetAttribute("key") ?? throw new ResourceLoadException());
                int action = int.Parse(reader.GetAttribute("action") ?? throw new ResourceLoadException());
                Animation anim = AnimationManager.AddPlaybackAnimation(animKey);
                _player.AddAnimation(action, anim);
                _player.CurrentAnimationType = action;
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Player" })
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