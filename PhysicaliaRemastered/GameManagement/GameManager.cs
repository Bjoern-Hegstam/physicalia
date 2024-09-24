using System;
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

public class GameManager(Game game)
{
    private Player? _player;
    
    private readonly List<World> _worlds = [];
    private int _worldIndex = -1;

    private int _pausePressedCount;

    public GameState State { get; set; } = GameState.Start;
    public GameState NextState { get; set; } = GameState.Start;

    private Fonts Fonts => game.Services.GetService<Fonts>();
    private Settings Settings => game.Services.GetService<Settings>();
    private SpriteLibrary SpriteLibrary => game.Services.GetService<SpriteLibrary>();
    private TileLibrary TileLibrary => game.Services.GetService<TileLibrary>();
    private AnimationLibrary AnimationLibrary => game.Services.GetService<AnimationLibrary>();
    private AnimationRunner AnimationRunner => game.Services.GetService<AnimationRunner>();
    private WeaponLibrary WeaponLibrary => game.Services.GetService<WeaponLibrary>();
    private ParticleEngine ParticleEngine => game.Services.GetService<ParticleEngine>();
    private EnemyLibrary EnemyLibrary => game.Services.GetService<EnemyLibrary>();
    private PickupTemplateLibrary PickupTemplateLibrary => game.Services.GetService<PickupTemplateLibrary>();

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
                AnimationRunner.Enabled = true;
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
                AnimationRunner.Enabled = false;
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

            _player!.Health = Settings.PlayerStartHealth;
            _player.Flickering = false;
        }
    }

    public void NewGame()
    {
        // Make sure the animation manager is enabled
        AnimationRunner.Enabled = true;

        _worldIndex = 0;
        _worlds[_worldIndex].NewGame();

        // Give the player his default weapon
        _player!.ClearWeapons();
        _player.AddWeapon(WeaponLibrary.GetWeapon(-1).Copy());

        State = NextState = GameState.Start;
    }

    public void LoadGame(SaveGame saveGame)
    {
        _worldIndex = saveGame.WorldIndex;
        _player!.Health = Settings.PlayerStartHealth;

        _player.LoadGame(saveGame, WeaponLibrary);

        _worlds[_worldIndex].LoadGame(saveGame);

        // Pause the game and ensure
        NextState = GameState.Paused;
        _pausePressedCount = 1;
    }

    public SaveGame SaveGame()
    {
        NextState = GameState.Paused;

        var saveGame = new SaveGame
        {
            WorldIndex = _worldIndex
        };

        _player!.SaveGame(saveGame);
        _worlds[_worldIndex].SaveGame(saveGame);

        return saveGame;
    }
    
    public void LoadContent(string path, ContentManager contentManager)
    {
        var readerSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true
        };

        using var reader = XmlReader.Create(path, readerSettings);
        LoadContent(reader, contentManager);
    }

    public void LoadContent(XmlReader reader, ContentManager contentManager)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Settings" })
            {
                Settings settings = SettingsLoader.Load(Environment.GameDataPath + reader.ReadString(), game);
                game.Services.AddService(settings);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "SpriteLibrary" })
            {
                SpriteLibrary spriteLibrary = SpriteLibraryLoader.Load(Environment.LibraryPath + reader.ReadString(), contentManager);
                game.Services.AddService(spriteLibrary);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "AnimationLibrary" })
            {
                AnimationLibrary animationLibrary = AnimationLibraryLoader.Load(Environment.LibraryPath + reader.ReadString(), contentManager);
                game.Services.AddService(animationLibrary);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "TileLibrary" })
            {
                TileLibrary tileLibrary = TileLibraryLoader.Load(Environment.LibraryPath + reader.ReadString(), SpriteLibrary, AnimationRunner);
                game.Services.AddService(tileLibrary);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "ParticleDefinitions" })
            {
                ParticleEngine.LoadXml(Environment.LibraryPath + reader.ReadString(), SpriteLibrary, AnimationRunner);

                // Prepare the particle engine to avoid slow downs (JIT)
                ParticleEngine.Prepare();
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "WeaponLibrary" })
            {
                WeaponLibrary.LoadXml(Environment.LibraryPath + reader.ReadString());
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "EnemyLibrary" })
            {
                EnemyLibrary.LoadXml(Environment.LibraryPath + reader.ReadString());
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "PickupLibrary" })
            {
                PickupTemplateLibrary.LoadXml(Environment.LibraryPath + reader.ReadString(), SpriteLibrary);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Player" })
            {
                LoadPlayer(reader);
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
                var world = new World(game, _player);
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

    private void LoadPlayer(XmlReader reader)
    {
        _player = new Player(game.Services.GetService<Settings>());
            
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "CollisionBox" })
            {
                _player.CollisionBox = ReadRectangle(reader);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Animation" })
            {
                var actorState = Enum.Parse<ActorState>(reader.GetAttribute("actorState") ?? throw new ResourceLoadException());
                var animationDefinitionId = new AnimationDefinitionId(reader.GetAttribute("id") ?? throw new ResourceLoadException());

                Animation anim = new Animation(AnimationLibrary[animationDefinitionId]);
                _player.AddAnimation(actorState, anim);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Player" })
            {
                return;
            }
        }
    }

    private static Rectangle ReadRectangle(XmlReader reader)
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
                if (Settings.InputMap.IsPressed(InputAction.MenuStart))
                {
                    game.Exit();
                }
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
                spriteBatch.DrawString(Fonts.WorldQuote, "And so it begins", new Vector2(130, 200), Color.White);
                break;
            case GameState.Playing:
                _worlds[_worldIndex].Draw(spriteBatch);
                break;
            case GameState.Paused:
                _worlds[_worldIndex].Draw(spriteBatch);
                break;
            case GameState.End:
                spriteBatch.DrawString(Fonts.PlayerDead, "Great Success!", new Vector2(150, 250), Color.White);
                break;
        }
    }
}