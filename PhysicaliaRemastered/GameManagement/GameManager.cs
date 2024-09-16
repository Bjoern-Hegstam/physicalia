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
using XNALibrary.Graphics;
using XNALibrary.Graphics.Animation;
using XNALibrary.Graphics.ParticleEngine;
using XNALibrary.Graphics.Sprites;
using XNALibrary.Graphics.TileEngine;
using XNALibrary.Interfaces;

namespace PhysicaliaRemastered.GameManagement;

public enum GameState
{
    Start,
    Playing,
    End,
    Paused
}

/// <summary>
/// Manages over the game components of the Physicalia game.
/// </summary>
public class GameManager
{
    private const string GAMEDATA_PATH = @"Content\GameData\";
    private const string LIBRARY_PATH = GAMEDATA_PATH + "Libraries\\";
    private const string WORLD_PATH = GAMEDATA_PATH + "Worlds\\";

    private Game game;
    private ISettings settings;
    private ITextureLibrary textureLibrary;
    private TileLibrary tileLibrary;
    private SpriteLibrary spriteLibrary;
    private ParticleEngine particleEngine;
    private AnimationManager animationManager;
    private EnemyBank enemyBank;
    private WeaponBank weaponBank;
    private PickupLibrary modifierLibrary;

    private List<World> worlds;
    private int worldIndex;

    private Player player;

    private GameState currentState;
    private GameState nextState;

    // Used for making sure pause mode is only entered when it's available
    private int pausePressedCount = 0;

    public GameState State
    {
        get { return currentState; }
        set { currentState = value; }
    }

    public GameState NextState
    {
        get { return nextState; }
        set { nextState = value; }
    }


    public ISettings Settings
    {
        get { return settings; }
    }

    /// <summary>
    /// Creates a new GameManager.
    /// </summary>
    /// <param name="game">The game creating the GameManager. The Manager expects
    /// the Game to have an ISettings service.</param>
    public GameManager(Game game)
    {
        this.game = game;

        // Create and add needed services
        IInputHandler input = (IInputHandler)this.game.Services.GetService(typeof(IInputHandler));
        settings = new Settings(input);
        this.game.Services.AddService(typeof(ISettings), settings);

        textureLibrary = new TextureLibrary();
        this.game.Services.AddService(typeof(ITextureLibrary), textureLibrary);

        spriteLibrary = new SpriteLibrary(textureLibrary);
        this.game.Services.AddService(typeof(ISpriteLibrary), spriteLibrary);

        animationManager = new AnimationManager(this.game, textureLibrary);
        this.game.Services.AddService(typeof(IAnimationManager), animationManager);
        this.game.Components.Add(animationManager);

        tileLibrary = new TileLibrary();
        this.game.Services.AddService(typeof(ITileLibrary), tileLibrary);

        particleEngine = new ParticleEngine();
        this.game.Services.AddService(typeof(IParticleEngine), particleEngine);

        enemyBank = new EnemyBank(animationManager);
        this.game.Services.AddService(typeof(IEnemyBank), enemyBank);

        weaponBank = new WeaponBank(particleEngine, spriteLibrary, animationManager);
        this.game.Services.AddService(typeof(IWeaponBank), weaponBank);

        modifierLibrary = new PickupLibrary();
        this.game.Services.AddService(typeof(IPickupLibrary), modifierLibrary);

        worlds = new List<World>();
        worldIndex = -1;

        player = new Player(settings);

        nextState = currentState = GameState.Start;
    }

    /// <summary>
    /// Changes the state of the GameManager.
    /// </summary>
    private void ChangeState()
    {
        // Cleanup things that was done when we previously entered the current state
        switch (currentState)
        {
            case GameState.Start:
                break;
            case GameState.Playing:
                break;
            case GameState.End:
                break;
            case GameState.Paused:
                animationManager.Enabled = true;
                break;
        }

        // Prepare for the next state
        switch (nextState)
        {
            case GameState.Start:
                break;
            case GameState.Playing:
                break;
            case GameState.End:
                break;
            case GameState.Paused:
                animationManager.Enabled = false;
                break;
        }

        currentState = nextState;
    }

    public void ResetLevel()
    {
        if (worldIndex < worlds.Count)
        {
            worlds[worldIndex].ResetLevel();
            nextState = GameState.Playing;
            pausePressedCount = 0;

            player.Health = settings.PlayerStartHealth;
            player.Flickering = false;
        }
    }

    /// <summary>
    /// Makes the game ready for a new game session.
    /// </summary>
    public void NewSession()
    {
        // Make sure the animation manager is enabled
        animationManager.Enabled = true;

        worldIndex = 0;
        worlds[worldIndex].NewSession();

        player.Health = settings.PlayerStartHealth;
        // Give the player his default weapon
        player.ClearWeapons();
        player.AddWeapon(weaponBank.GetWeapon(-1).Copy());

        State = nextState = GameState.Start;
    }

    /// <summary>
    /// Sets the state of the GameManager as specified by the session object.
    /// </summary>
    /// <param name="session">GameSession containing representing the
    /// wanted state of the GameManager.</param>
    public void LoadSession(GameSession session)
    {
        worldIndex = session.WorldIndex;
        player.Health = settings.PlayerStartHealth;

        player.LoadSession(session, weaponBank);

        worlds[worldIndex].LoadSession(session);

        // Pause the game and ensure
        nextState = GameState.Paused;
        pausePressedCount = 1;
    }

    /// <summary>
    /// Saves the current state of the GameManager to a GameSession object.
    /// </summary>
    /// <returns>A GameSession object representing the current state of
    /// the GameManager.</returns>
    public GameSession SaveSession()
    {
        nextState = GameState.Paused;

        GameSession session = new GameSession();

        session.WorldIndex = worldIndex;
        player.SaveSession(session);
        worlds[worldIndex].SaveSession(session);

        return session;
    }

    public void LoadContent(ContentManager contentManager)
    {
        settings.LoadContent(contentManager);
    }

    /// <summary>
    /// Loads in a game as specified in the file indentified by the path.
    /// </summary>
    /// <param name="path">Path to the file containing the xml data to load.</param>
    public void LoadXml(string path)
    {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.IgnoreComments = true;
        readerSettings.IgnoreWhitespace = true;
        readerSettings.IgnoreProcessingInstructions = true;

        using (XmlReader reader = XmlReader.Create(path, readerSettings))
        {
            LoadXml(reader);
        }
    }

    /// <summary>
    /// Loads in a game as specified by the xml data loaded by the reader.
    /// </summary>
    /// <param name="reader">XmlReader to be used for reading the xml data.</param>
    public void LoadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Settings")
            {
                settings.LoadXml(GAMEDATA_PATH + reader.ReadString(), spriteLibrary);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "TextureLibrary")
            {
                textureLibrary.LoadXml(LIBRARY_PATH + reader.ReadString(), game.GraphicsDevice);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "SpriteLibrary")
            {
                spriteLibrary.LoadXml(LIBRARY_PATH + reader.ReadString());
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "AnimationBank")
            {
                animationManager.LoadXml(LIBRARY_PATH + reader.ReadString());
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "TileLibrary")
            {
                tileLibrary.LoadXml(LIBRARY_PATH + reader.ReadString(), spriteLibrary, animationManager);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "ParticleDefinitions")
            {
                particleEngine.LoadXml(LIBRARY_PATH + reader.ReadString(), spriteLibrary, animationManager);

                // Prepare the particle engine to avoid slow downs (JIT)
                particleEngine.Prepare();
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "WeaponBank")
            {
                weaponBank.LoadXml(LIBRARY_PATH + reader.ReadString());
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "EnemyBank")
            {
                enemyBank.LoadXml(LIBRARY_PATH + reader.ReadString());
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "PickupLibrary")
            {
                modifierLibrary.LoadXml(LIBRARY_PATH + reader.ReadString(), spriteLibrary);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Player")
            {
                SetupPlayer(reader);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Worlds")
            {
                LoadWorlds(reader);
            }
        }

        // Set a valid index if one or more worlds has been added
        if (worlds.Count > 0)
            worldIndex = 0;
    }

    /// <summary>
    /// Loads in the World specified in the xml file read by the passed in
    /// XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    private void LoadWorlds(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "World")
            {
                World world = new World(game, player);
                worlds.Add(world);
                world.WorldIndex = worlds.Count;
                world.LoadXml(WORLD_PATH + reader.ReadString(), tileLibrary, spriteLibrary);
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "Worlds")
            {
                return;
            }
        }
    }

    /// <summary>
    /// Sets up the player according to the xml data.
    /// </summary>
    /// <param name="reader"></param>
    private void SetupPlayer(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "CollisionBox")
            {
                player.CollisionBox = ReadRectangle(reader);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Animation")
            {
                int animKey = int.Parse(reader.GetAttribute("key"));
                int action = int.Parse(reader.GetAttribute("action"));
                Animation anim = animationManager.AddPlaybackAnimation(animKey);
                player.AddAnimation(action, anim);
                player.CurrentAnimationType = action;
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "Player")
                return;
        }
    }

    private Rectangle ReadRectangle(XmlReader reader)
    {
        int x = int.Parse(reader.GetAttribute("x"));
        int y = int.Parse(reader.GetAttribute("y"));
        int width = int.Parse(reader.GetAttribute("width"));
        int height = int.Parse(reader.GetAttribute("height"));

        return new Rectangle(x, y, width, height);
    }

    public void Update(GameTime gameTime)
    {
        switch (currentState)
        {
            case GameState.Start:
                if (settings.InputMap.IsPressed(InputAction.MenuStart))
                    nextState = GameState.Playing;
                break;
            case GameState.Playing:
                // Has the Player finished the game?
                if (worlds[worldIndex].State == WorldState.Finished)
                {
                    // Go to next world when the player wants that
                    if (settings.InputMap.IsPressed(InputAction.MenuStart))
                    {
                        worldIndex++;
                    }

                    if (worldIndex >= worlds.Count)
                    {
                        // TODO: Congratulate player for winning the game
                        // Perhaps add some kind of special finish (Bonus level, etc.)
                        nextState = GameState.End;
                    }
                }
                else
                {
                    if (worlds[worldIndex].LevelState == LevelState.Start ||
                        worlds[worldIndex].LevelState == LevelState.Dead)
                        pausePressedCount = 0;
                    else
                        pausePressedCount = 1;

                    // Pause if start is pressed
                    // pausePressedCount is used as a workaround the fact that
                    // MenuStart will be pressed when a Level is start. The Level's
                    // state will then be set to LevelState.Playing which causes the
                    // Level to start paused.
                    if (worlds[worldIndex].LevelState == LevelState.Playing &&
                        settings.InputMap.IsPressed(InputAction.MenuStart) &&
                        pausePressedCount == 1)
                        nextState = GameState.Paused;

                    worlds[worldIndex].Update(gameTime);
                }
                break;
            case GameState.Paused:
                // Continue playing if the player wants to go back
                if (settings.InputMap.IsPressed(InputAction.MenuBack))
                    nextState = GameState.Playing;
                break;
            case GameState.End:
                break;
        }

        while (nextState != currentState)
            ChangeState();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        switch (currentState)
        {
            case GameState.Start:
                spriteBatch.DrawString(settings.WorldQuoteFont, "And so it begins", new Vector2(130, 200), Color.White);
                break;
            case GameState.Playing:
                worlds[worldIndex].Draw(spriteBatch);
                break;
            case GameState.Paused:
                worlds[worldIndex].Draw(spriteBatch);
                break;
            case GameState.End:
                spriteBatch.DrawString(settings.PlayerDeadFont, "Great Success!", new Vector2(150, 250), Color.White);
                break;
        }
    }
}