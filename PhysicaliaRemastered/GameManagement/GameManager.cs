using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using XNALibrary.Graphics;
using XNALibrary.Graphics.Particles;
using XNALibrary.Services;
using Physicalia.Input;
using Physicalia.Weapons;
using Physicalia.Pickups;

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

    #region Fields

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

    #endregion

    #region Properties

    public GameState State
    {
        get { return this.currentState; }
        set { this.currentState = value; }
    }

    public GameState NextState
    {
        get { return this.nextState; }
        set { this.nextState = value; }
    }


    public ISettings Settings
    {
        get { return this.settings; }
    }

    #endregion

    #region Constructors

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
        this.settings = new Settings(input);
        this.game.Services.AddService(typeof(ISettings), this.settings);

        this.textureLibrary = new TextureLibrary();
        this.game.Services.AddService(typeof(ITextureLibrary), this.textureLibrary);

        this.spriteLibrary = new SpriteLibrary(this.textureLibrary);
        this.game.Services.AddService(typeof(ISpriteLibrary), this.spriteLibrary);

        this.animationManager = new AnimationManager(this.game, this.textureLibrary);
        this.game.Services.AddService(typeof(IAnimationManager), this.animationManager);
        this.game.Components.Add(this.animationManager);

        this.tileLibrary = new TileLibrary();
        this.game.Services.AddService(typeof(ITileLibrary), this.tileLibrary);

        this.particleEngine = new ParticleEngine();
        this.game.Services.AddService(typeof(IParticleEngine), this.particleEngine);

        this.enemyBank = new EnemyBank(this.animationManager);
        this.game.Services.AddService(typeof(IEnemyBank), this.enemyBank);

        this.weaponBank = new WeaponBank(this.particleEngine, this.spriteLibrary, this.animationManager);
        this.game.Services.AddService(typeof(IWeaponBank), this.weaponBank);

        this.modifierLibrary = new PickupLibrary();
        this.game.Services.AddService(typeof(IPickupLibrary), this.modifierLibrary);

        this.worlds = new List<World>();
        this.worldIndex = -1;

        this.player = new Player(this.settings);

        this.nextState = this.currentState = GameState.Start;
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Changes the state of the GameManager.
    /// </summary>
    private void ChangeState()
    {
        // Cleanup things that was done when we previously entered the current state
        switch (this.currentState)
        {
            case GameState.Start:
                break;
            case GameState.Playing:
                break;
            case GameState.End:
                break;
            case GameState.Paused:
                this.animationManager.Enabled = true;
                break;
            default:
                break;
        }

        // Prepare for the next state
        switch (this.nextState)
        {
            case GameState.Start:
                break;
            case GameState.Playing:
                break;
            case GameState.End:
                break;
            case GameState.Paused:
                this.animationManager.Enabled = false;
                break;
            default:
                break;
        }

        this.currentState = this.nextState;
    }

    #endregion

    public void ResetLevel()
    {
        if (this.worldIndex < this.worlds.Count)
        {
            this.worlds[this.worldIndex].ResetLevel();
            this.nextState = GameState.Playing;
            this.pausePressedCount = 0;

            this.player.Health = this.settings.PlayerStartHealth;
            this.player.Flickering = false;
        }
    }

    #region Session management

    /// <summary>
    /// Makes the game ready for a new game session.
    /// </summary>
    public void NewSession()
    {
        // Make sure the animation manager is enabled
        this.animationManager.Enabled = true;

        this.worldIndex = 0;
        this.worlds[this.worldIndex].NewSession();

        this.player.Health = this.settings.PlayerStartHealth;
        // Give the player his default weapon
        this.player.ClearWeapons();
        this.player.AddWeapon(this.weaponBank.GetWeapon(-1).Copy());

        this.State = this.nextState = GameState.Start;
    }

    /// <summary>
    /// Sets the state of the GameManager as specified by the session object.
    /// </summary>
    /// <param name="session">GameSession containing representing the
    /// wanted state of the GameManager.</param>
    public void LoadSession(GameSession session)
    {
        this.worldIndex = session.WorldIndex;
        this.player.Health = this.settings.PlayerStartHealth;

        this.player.LoadSession(session, this.weaponBank);

        this.worlds[this.worldIndex].LoadSession(session);

        // Pause the game and ensure
        this.nextState = GameState.Paused;
        this.pausePressedCount = 1;
    }

    /// <summary>
    /// Saves the current state of the GameManager to a GameSession object.
    /// </summary>
    /// <returns>A GameSession object representing the current state of
    /// the GameManager.</returns>
    public GameSession SaveSession()
    {
        this.nextState = GameState.Paused;

        GameSession session = new GameSession();

        session.WorldIndex = this.worldIndex;
        this.player.SaveSession(session);
        this.worlds[this.worldIndex].SaveSession(session);

        return session;
    }

    #endregion

    #region Load Data

    public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
    {
        this.settings.LoadContent(contentManager);
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
            this.LoadXml(reader);
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
                this.settings.LoadXml(GAMEDATA_PATH + reader.ReadString(), this.spriteLibrary);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "TextureLibrary")
            {
                this.textureLibrary.LoadXml(LIBRARY_PATH + reader.ReadString(), this.game.GraphicsDevice);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "SpriteLibrary")
            {
                this.spriteLibrary.LoadXml(LIBRARY_PATH + reader.ReadString());
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "AnimationBank")
            {
                this.animationManager.LoadXml(LIBRARY_PATH + reader.ReadString());
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "TileLibrary")
            {
                this.tileLibrary.LoadXml(LIBRARY_PATH + reader.ReadString(), this.spriteLibrary, this.animationManager);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "ParticleDefinitions")
            {
                this.particleEngine.LoadXml(LIBRARY_PATH + reader.ReadString(), this.spriteLibrary, this.animationManager);

                // Prepare the particle engine to avoid slow downs (JIT)
                this.particleEngine.Prepare();
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "WeaponBank")
            {
                this.weaponBank.LoadXml(LIBRARY_PATH + reader.ReadString());
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "EnemyBank")
            {
                this.enemyBank.LoadXml(LIBRARY_PATH + reader.ReadString());
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "PickupLibrary")
            {
                this.modifierLibrary.LoadXml(LIBRARY_PATH + reader.ReadString(), this.spriteLibrary);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Player")
            {
                this.SetupPlayer(reader);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Worlds")
            {
                this.LoadWorlds(reader);
            }
        }

        // Set a valid index if one or more worlds has been added
        if (this.worlds.Count > 0)
            this.worldIndex = 0;
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
                World world = new World(this.game, this.player);
                this.worlds.Add(world);
                world.WorldIndex = this.worlds.Count;
                world.LoadXml(WORLD_PATH + reader.ReadString(), this.tileLibrary, this.spriteLibrary);
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
                this.player.CollisionBox = this.ReadRectangle(reader);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Animation")
            {
                int animKey = int.Parse(reader.GetAttribute("key"));
                int action = int.Parse(reader.GetAttribute("action"));
                Animation anim = this.animationManager.AddPlaybackAnimation(animKey);
                this.player.AddAnimation(action, anim);
                this.player.CurrentAnimationType = action;
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

    #endregion

    #region Update & Draw

    public void Update(GameTime gameTime)
    {
        switch (this.currentState)
        {
            case GameState.Start:
                if (this.settings.InputMap.IsPressed(InputAction.MenuStart))
                    this.nextState = GameState.Playing;
                break;
            case GameState.Playing:
                // Has the Player finished the game?
                if (this.worlds[this.worldIndex].State == WorldState.Finished)
                {
                    // Go to next world when the player wants that
                    if (this.settings.InputMap.IsPressed(InputAction.MenuStart))
                    {
                        this.worldIndex++;
                    }

                    if (this.worldIndex >= this.worlds.Count)
                    {
                        // TODO: Congratulate player for winning the game
                        // Perhaps add some kind of special finish (Bonus level, etc.)
                        this.nextState = GameState.End;
                    }
                }
                else
                {
                    if (this.worlds[this.worldIndex].LevelState == LevelState.Start ||
                        this.worlds[this.worldIndex].LevelState == LevelState.Dead)
                        this.pausePressedCount = 0;
                    else
                        this.pausePressedCount = 1;

                    // Pause if start is pressed
                    // pausePressedCount is used as a workaround the fact that
                    // MenuStart will be pressed when a Level is start. The Level's
                    // state will then be set to LevelState.Playing which causes the
                    // Level to start paused.
                    if (this.worlds[this.worldIndex].LevelState == LevelState.Playing &&
                        this.settings.InputMap.IsPressed(InputAction.MenuStart) &&
                        this.pausePressedCount == 1)
                        this.nextState = GameState.Paused;

                    this.worlds[this.worldIndex].Update(gameTime);
                }
                break;
            case GameState.Paused:
                // Continue playing if the player wants to go back
                if (this.settings.InputMap.IsPressed(InputAction.MenuBack))
                    this.nextState = GameState.Playing;
                break;
            case GameState.End:
                break;
        }

        while (this.nextState != this.currentState)
            this.ChangeState();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        switch (this.currentState)
        {
            case GameState.Start:
                spriteBatch.DrawString(this.settings.WorldQuoteFont, "And so it begins", new Vector2(130, 200), Color.White);
                break;
            case GameState.Playing:
                this.worlds[this.worldIndex].Draw(spriteBatch);
                break;
            case GameState.Paused:
                this.worlds[this.worldIndex].Draw(spriteBatch);
                break;
            case GameState.End:
                spriteBatch.DrawString(this.settings.PlayerDeadFont, "Great Success!", new Vector2(150, 250), Color.White);
                break;
        }
    }

    #endregion
}