using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary;
using XNALibrary.Graphics;
using XNALibrary.Services;
using Physicalia.Pickups;
using Physicalia.Input;
using System.Xml;
using Physicalia.Weapons;
using Physicalia.Pickups.Modifiers;

namespace PhysicaliaRemastered.GameManagement;

/// <summary>
/// Represents the possible states a Level can be in.
/// </summary>
public enum LevelState
{
    Start,
    Playing,
    Dead,
    Finished
}

/// <summary>
/// Class representing a Level in the game. A Level consists of a layered
/// background, a tiled foreground, enemies, different kinds of active
/// objects, particles and a player.
/// </summary>
public class Level
{
    #region Constants

    // The start position of the UI in the y-axis
    private const float UI_INDEX_POS_Y = 10F;
    private const float UI_MODIFIER_SPACING = 5F;

    private const int SCREEN_ACTIVATION_DISTANCE = 20;

    private const float PLAYER_FINISH_SLOWDOWN = 0.95F;

    #endregion

    #region Fields

    private int levelIndex;
    private int worldIndex;
    private ISettings settings;
    private Game game;

    // States
    private LevelState state;
    private LevelState nextState;

    public LevelState State
    {
        get { return this.state; }
    }

    public LevelState NextState
    {
        get { return this.nextState; }
        set { this.nextState = value; }
    }

    // View
    private ScreenSampler screenSampler;
    private IAnimationManager animationManager;
    private ISpriteLibrary spriteLibrary;
    private List<BackgroundLayer> backgrounds;

    // Gameplay
    private List<ModifierPickup> modifiers;
    private List<TileEngine> tileEngines;
    private IParticleEngine particleEngine;
    private Player player;
    private ActorStartValues playerStartValues;
    private EnemyManager enemyManager;
    private IWeaponBank weaponBank;
    private IPickupLibrary modifierLibrary;

    // ActiveObjects

    // Lists of ActiveObjects
    private List<ActiveObject> activeObjects;
    private List<ActiveObject> inactiveObjects;

    #endregion

    #region Properties

    public int WorldIndex
    {
        get { return this.worldIndex; }
        set { this.worldIndex = value; }
    }

    public int LevelIndex
    {
        get { return this.levelIndex; }
        set { this.levelIndex = value; }
    }

    public Player Player
    {
        get { return this.player; }
        set { this.player = value; }
    }

    public IAnimationManager AnimationManager
    {
        get { return this.animationManager; }
        set { this.animationManager = value; }
    }

    public ISpriteLibrary SpriteLibrary
    {
        get { return this.spriteLibrary; }
        set { this.spriteLibrary = value; }
    }

    public IParticleEngine ParticleEngine
    {
        get { return this.particleEngine; }
        set { this.particleEngine = value; }
    }

    public ISettings Settings
    {
        get { return this.settings; }
    }

    public EnemyManager EnemyManager
    {
        get { return this.enemyManager; }
        set { this.enemyManager = value; }
    }

    public ScreenSampler ScreenSampler
    {
        get { return this.screenSampler; }
    }

    #endregion

    #region Constructors

    public Level(Game game, Player player)
    {
        this.game = game;

        this.backgrounds = new List<BackgroundLayer>();

        // Get needed services
        this.settings = (ISettings)game.Services.GetService(typeof(ISettings));
        this.animationManager = (IAnimationManager)game.Services.GetService(typeof(IAnimationManager));
        this.spriteLibrary = (ISpriteLibrary)game.Services.GetService(typeof(ISpriteLibrary));
        this.enemyManager = new EnemyManager((IEnemyBank)this.game.Services.GetService(typeof(IEnemyBank)));
        this.weaponBank = (IWeaponBank)this.game.Services.GetService(typeof(IWeaponBank));
        this.modifierLibrary = (IPickupLibrary)this.game.Services.GetService(typeof(IPickupLibrary));

        this.player = player;
        this.playerStartValues = new ActorStartValues();
        this.nextState = this.state = LevelState.Start;
        this.particleEngine = (IParticleEngine)game.Services.GetService(typeof(IParticleEngine));
        this.screenSampler = new ScreenSampler(game, 0, 0, this.game.GraphicsDevice.Viewport.Width, this.game.GraphicsDevice.Viewport.Height);
        this.tileEngines = new List<TileEngine>();
        this.modifiers = new List<ModifierPickup>();

        this.activeObjects = new List<ActiveObject>();
        this.inactiveObjects = new List<ActiveObject>();
    }

    #endregion

    #region Public methods

    public void Update(GameTime gameTime)
    {
        // Switch based on current state
        switch (this.state)
        {
            case LevelState.Start:
                if (this.settings.InputMap.IsPressed(InputAction.MenuStart))
                    this.nextState = LevelState.Playing;
                break;
            case LevelState.Playing:
                this.UpdateLevel(gameTime);
                this.CheckCollisions();
                this.player.HandleInput();
                this.UpdateAnimations();

                // See if the player has fallen out of the level
                if (this.PlayerOutsideLevel())
                {
                    this.player.Kill();

                    // Do a little jump into screen
                    this.player.Velocity = new Vector2(0, this.player.JumpMagnitude * Math.Sign(this.player.Acceleration.Y));
                    this.nextState = LevelState.Dead;
                }

                // Check if the player's died
                if (this.player.Health <= 0)
                {
                    this.player.CanCollide = false;
                    this.player.Velocity = new Vector2(0, this.player.JumpMagnitude * Math.Sign(this.player.Acceleration.Y));

                    this.nextState = LevelState.Dead;
                }
                break;
            case LevelState.Dead:
                // Continue updating the Level after death, just don't check input
                this.UpdateLevel(gameTime);
                this.CheckCollisions();
                this.UpdateAnimations();

                // Don't let the player continue falling if it's outside of the level (i.e. falling)
                if (player.Velocity != Vector2.Zero &&
                    this.PlayerOffScreen() &&
                    this.player.Velocity.Y / this.player.Acceleration.Y > 0)
                {
                    player.Velocity = Vector2.Zero;
                    player.Acceleration = Vector2.Zero;
                }
                break;
            case LevelState.Finished:
                // Don't let the player continue falling if it's outside of the level (i.e. falling)
                if (this.player.Velocity != Vector2.Zero &&
                    this.PlayerOffScreen() &&
                    this.player.Velocity.Y / this.player.Acceleration.Y > 0)
                {
                    this.player.Velocity = Vector2.Zero;
                    this.player.Acceleration = Vector2.Zero;
                }

                // Slow down the player's movement in X if needed
                if (this.player.Velocity.X != 0)
                {
                    this.player.Velocity *= new Vector2(PLAYER_FINISH_SLOWDOWN, 1F);

                    // Set velocity to zero if it's very close
                    if (this.player.Velocity.X > -1 && this.player.Velocity.X < 1)
                        this.player.Velocity *= Vector2.UnitY;
                }

                this.UpdateLevel(gameTime);
                this.CheckCollisions();
                this.UpdateAnimations();
                break;
            default:
                break;
        }

        // Should the state be changed?
        if (this.nextState != this.state)
            this.ChangeState();
    }

    #region Xml loading

    public void LoadXml(string path, ITileLibrary tileLibrary)
    {
        using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(path))
        {
            this.LoadXml(reader, tileLibrary);
        }
    }

    public void LoadXml(XmlReader reader, ITileLibrary tileLibrary)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Level")
            {
                int width = int.Parse(reader.GetAttribute("width"));
                int height = int.Parse(reader.GetAttribute("height"));

                this.screenSampler.MaxWidth = width;
                this.screenSampler.MaxHeight = height;
            }

            if (reader.NodeType == System.Xml.XmlNodeType.Element &&
                reader.LocalName == "PlayerStart")
                this.playerStartValues = ActorStartValues.FromXml(reader, "PlayerStart");

            if (reader.NodeType == System.Xml.XmlNodeType.Element &&
                reader.LocalName == "Enemies")
                this.LoadEnemies(reader);

            if (reader.NodeType == System.Xml.XmlNodeType.Element &&
                reader.LocalName == "ActiveObjects")
                this.LoadActiveObjects(reader);

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Background")
            {
                int spriteKey = int.Parse(reader.GetAttribute("spriteKey"));
                float depth = float.Parse(reader.GetAttribute("depth"));
                string loopString = reader.GetAttribute("loop");
                bool loopX = loopString.Contains("x"); bool loopY = loopString.Contains("y");

                Sprite sprite = this.spriteLibrary.GetSprite(spriteKey);
                BackgroundLayer background = new BackgroundLayer(sprite, depth);
                background.LoopX = loopX; background.LoopY = loopY;

                if (!reader.IsEmptyElement)
                {
                    reader.ReadToFollowing("Position");
                    float x = float.Parse(reader.GetAttribute("x"));
                    float y = float.Parse(reader.GetAttribute("y"));
                    background.StartPosition = new Vector2(x, y);
                }

                this.backgrounds.Add(background);
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "Backgrounds")
            {
                this.backgrounds.Sort(BackgroundLayer.Compare);
            }

            // TileEngines?
            if (reader.NodeType == System.Xml.XmlNodeType.Element &&
                reader.LocalName == "TileEngines")
            {
                int engineCount = int.Parse(reader.GetAttribute("count"));

                for (int i = 0; i < engineCount; i++)
                {
                    // Add a new TileEngine
                    this.tileEngines.Add(new TileEngine(tileLibrary, 32, 32));

                    TileEngine tileEngine = this.tileEngines[i];

                    // Setup the new TileEngine
                    tileEngine.LoadXml(reader);
                }
            }
        }
    }

    /// <summary>
    /// Loads in enemies as specfied by the xml read by the XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    private void LoadEnemies(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Enemy")
            {
                int type = int.Parse(reader.GetAttribute("type"));
                ActorStartValues startValues = ActorStartValues.FromXml(reader, "StartValues");

                reader.ReadToFollowing("PatrolArea");
                int x = int.Parse(reader.GetAttribute("x"));
                int y = int.Parse(reader.GetAttribute("y"));
                int width = int.Parse(reader.GetAttribute("width"));
                int height = int.Parse(reader.GetAttribute("height"));
                Rectangle patrolArea = new Rectangle(x, y, width, height);

                this.enemyManager.EnqueueEnemy(type, startValues, patrolArea);
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "Enemies")
                return;
        }
    }

    /// <summary>
    /// Loads in ActiveObjects as specfied by the xml read by the XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    private void LoadActiveObjects(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Weapons")
            {
                if (reader.IsEmptyElement)
                    continue;

                this.LoadWeapons(reader);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Pickups")
            {
                if (reader.IsEmptyElement)
                    continue;

                this.LoadPickups(reader);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "LevelFinishTrigger")
            {
                int x = int.Parse(reader.GetAttribute("x"));
                int y = int.Parse(reader.GetAttribute("y"));

                Sprite triggerSprite = this.spriteLibrary.GetSprite(-1);
                EndLevelTrigger trigger = new EndLevelTrigger(this, triggerSprite);

                PickupContainer cont = new PickupContainer(trigger);
                cont.Position = new Vector2(x, y);
                cont.CollisionBox = new Rectangle(0, 0, triggerSprite.SourceRectangle.Width, triggerSprite.SourceRectangle.Height);
                cont.CanCollide = true;
                cont.IsActive = false;

                this.EnqueueActiveObject(cont);
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "ActiveObjects")
                return;
        }
    }

    private void LoadWeapons(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Weapon")
            {
                int key = int.Parse(reader.GetAttribute("key"));
                Weapon weapon = this.weaponBank.GetWeapon(key);

                WeaponPickup weaponPickup = new WeaponPickup(this, weapon);
                PickupContainer pickupCont = new PickupContainer(weaponPickup);

                reader.ReadToFollowing("Position");
                float x = float.Parse(reader.GetAttribute("x"));
                float y = float.Parse(reader.GetAttribute("y"));
                pickupCont.Position = new Vector2(x, y);

                reader.ReadToFollowing("CollisionBox");
                int xBox = int.Parse(reader.GetAttribute("x"));
                int yBox = int.Parse(reader.GetAttribute("y"));
                int width = int.Parse(reader.GetAttribute("width"));
                int height = int.Parse(reader.GetAttribute("height"));
                pickupCont.CollisionBox = new Rectangle(xBox, yBox, width, height);

                this.EnqueueActiveObject(pickupCont);
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "Weapons")
                return;
        }
    }

    private void LoadPickups(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Pickup")
            {
                int key = int.Parse(reader.GetAttribute("key"));
                int x = int.Parse(reader.GetAttribute("x"));
                int y = int.Parse(reader.GetAttribute("y"));

                Pickup pickup = this.modifierLibrary.GetPickup(key);
                pickup.Level = this;

                PickupContainer cont = new PickupContainer(pickup);
                cont.Position = new Vector2(x, y);
                cont.CollisionBox = new Rectangle(0, 0, pickup.Sprite.SourceRectangle.Width, pickup.Sprite.SourceRectangle.Height);
                this.EnqueueActiveObject(cont);
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "Pickups")
                return;
        }
    }

    #endregion

    /// <summary>
    /// Resets the Level to its initial state.
    /// </summary>
    public void Reset()
    {
        // Reset background layers
        foreach (BackgroundLayer background in this.backgrounds)
            background.Position = background.StartPosition;

        // Reset enemies
        this.enemyManager.Reset();

        // Set and apply the start values of the player
        this.player.StartValues = this.playerStartValues;
        this.player.ApplyStartValues();
        this.player.CanCollide = true;
        this.player.CanTakeDamage = true;

        this.player.Health = this.settings.PlayerStartHealth;

        // Retrive the ammo count the player's weapons had at the start of the level
        this.player.ApplyStoredWeaponAmmoCount();

        // Set the start position of the screen sampler
        this.screenSampler.Position = Vector2.Zero;

        while (this.activeObjects.Count > 0)
        {
            this.activeObjects[0].Reset();
            this.EnqueueActiveObject(this.activeObjects[0]);
            this.activeObjects.RemoveAt(0);
        }

        // Clear out any modifiers
        this.modifiers.Clear();

        // Set the state of the level
        this.nextState = this.state = LevelState.Start;

        // Run a check so that any objects that's on screen is activated
        this.ActivateObjects();
    }

    /// <summary>
    /// Makes a soft reset of the level. Soft meaning that the player's weapons
    /// wont be taken from him and his position wont be touched. The level's
    /// state will also not be taken care of.
    /// </summary>
    public void SoftReset()
    {
        // Reset background layers
        foreach (BackgroundLayer background in this.backgrounds)
            background.Position = background.StartPosition;

        // Reset enemies
        this.enemyManager.Reset();

        this.player.CanCollide = true;
        this.player.CanTakeDamage = true;

        // Set the start position of the screen sampler
        this.screenSampler.Position = Vector2.Zero;

        while (this.activeObjects.Count > 0)
        {
            this.activeObjects[0].Reset();
            this.EnqueueActiveObject(this.activeObjects[0]);
            this.activeObjects.RemoveAt(0);
        }

        // Clear out any modifiers
        this.modifiers.Clear();

        // Run a check so that any objects that's on screen is activated
        this.ActivateObjects();
        this.enemyManager.ActivateVisible(this.screenSampler.ScreenRectangle);
    }

    /// <summary>
    /// Adds a modifer to the Level's collection of modifiers.
    /// </summary>
    /// <param name="modifier">The modifier to add.</param>
    public void AddModifier(ModifierPickup modifier)
    {
        this.modifiers.Add(modifier);
    }

    /// <summary>
    /// Adds an ActiveObject to the Level's list of ActiveObjects.
    /// </summary>
    /// <param name="obj">ActiveObject to add.</param>
    public void AddActiveObject(ActiveObject obj)
    {
        this.activeObjects.Add(obj);
    }

    /// <summary>
    /// Enqueues an ActiveObject to the Level's Queue of ActiveObjects.
    /// </summary>
    /// <param name="obj">ActiveObject to enqueue.</param>
    public void EnqueueActiveObject(ActiveObject obj)
    {
        this.inactiveObjects.Add(obj);
    }

    /// <summary>
    /// Draws the level.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to use for drawing.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        switch (this.state)
        {
            case LevelState.Start:
                string indexString = this.WorldIndex + " - " + this.levelIndex;
                Vector2 indexStringSize = this.settings.LevelIndexFont.MeasureString(indexString);
                Vector2 indexPosition = new Vector2();
                indexPosition.X = (this.screenSampler.Width - indexStringSize.X) / 2;
                indexPosition.Y = (this.screenSampler.Height - indexStringSize.Y) / 2;

                spriteBatch.DrawString(this.settings.LevelIndexFont, indexString, indexPosition, Color.White);
                break;
            case LevelState.Playing:
                this.DrawLevel(spriteBatch);
                break;
            case LevelState.Dead:
                this.DrawLevel(spriteBatch);

                String deadString = "You Have Died!";
                Vector2 deadStringSize = this.settings.PlayerDeadFont.MeasureString(deadString);
                Vector2 deadPos;
                deadPos.X = (this.screenSampler.Width - deadStringSize.X) / 2;
                deadPos.Y = (this.screenSampler.Width - deadStringSize.X) / 2;

                spriteBatch.DrawString(this.settings.PlayerDeadFont, deadString, deadPos, Color.White);
                break;
            case LevelState.Finished:
                this.DrawLevel(spriteBatch);

                String finishString = "Level Finished!";
                Vector2 finishStringSize = this.settings.PlayerDeadFont.MeasureString(finishString);
                Vector2 finishPos;
                finishPos.X = (this.screenSampler.Width - finishStringSize.X) / 2;
                finishPos.Y = (this.screenSampler.Width - finishStringSize.X) / 2;

                spriteBatch.DrawString(this.settings.PlayerDeadFont, finishString, finishPos, Color.White);
                break;
            default:
                break;
        }
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Changes the current state of the level to the next selected one. Code
    /// related to the change between different states should be placed here.
    /// </summary>
    private void ChangeState()
    {
        switch (this.state)
        {
            case LevelState.Start:
                break;
            case LevelState.Playing:
                if (this.player.CurrentWeapon != null &&
                    this.player.CurrentWeapon.IsFiring)
                    this.player.CurrentWeapon.Stop();
                break;
            case LevelState.Dead:
                break;
            case LevelState.Finished:
                break;
            default:
                break;
        }

        this.state = this.nextState;

        switch (this.nextState)
        {
            case LevelState.Start:
                break;
            case LevelState.Playing:
                // Store the current ammo count of the player's retrieved weapons
                this.player.StoreWeaponAmmoCount();
                break;
            case LevelState.Dead:
                break;
            case LevelState.Finished:
                // Store the current ammo count of the player's retrieved weapons
                this.player.StoreWeaponAmmoCount();
                this.player.CurrentAnimationType = (int)ActorAnimation.Win;

                this.player.CanTakeDamage = false;
                break;
            default:
                break;
        }
    }

    private bool PlayerOffScreen()
    {
        Rectangle playerRect = this.player.CurrentAnimation.SourceRectangle;
        playerRect.X = (int)(this.player.Position.X - this.player.Origin.X);
        playerRect.Y = (int)(this.player.Position.Y - this.player.Origin.Y);

        return !this.screenSampler.ScreenRectangle.Intersects(playerRect);
    }

    /// <summary>
    /// Checks whether the player is outside of the Level area.
    /// </summary>
    /// <returns>True if the player is outside of the Level; false otherwise.</returns>
    private bool PlayerOutsideLevel()
    {
        bool result = false;

        Rectangle levelRect = new Rectangle();
        levelRect.Width = this.screenSampler.MaxWidth;
        levelRect.Height = this.screenSampler.MaxHeight;

        Rectangle playerRect = this.player.CurrentAnimation.SourceRectangle;
        playerRect.X = (int)(this.player.Position.X - this.player.Origin.X);
        playerRect.Y = (int)(this.player.Position.Y - this.player.Origin.Y);

        if (!levelRect.Intersects(playerRect))
        {
            // The player can only fall outside the level in Y
            if (playerRect.Bottom <= levelRect.Top &&
                player.Acceleration.Y > 0)
                result = false;
            else if (playerRect.Top >= levelRect.Bottom &&
                     player.Acceleration.Y < 0)
                result = false;
            else
                result = true;
        }


        return result;
    }

    /// <summary>
    /// Updates the objects in the level.
    /// </summary>
    private void UpdateLevel(GameTime gameTime)
    {
        // TODO: Add variable for modifying the gameTime, so that it can be controlled by a modifier.
        // Create a new GameTime variable with modified time

        // Player
        this.player.Update(gameTime);

        // Make sure the player's right and left edges are within the screen

        // Right edge
        if (this.player.Position.X < this.player.Origin.X)
        {
            this.player.Position *= Vector2.UnitY;
            this.player.Position += new Vector2(this.player.Origin.X, 0);
        }

        // Left edge
        if (this.player.Position.X - this.player.Origin.X + this.player.Width > this.screenSampler.MaxWidth)
        {
            this.player.Position *= Vector2.UnitY;
            this.player.Position += new Vector2(this.screenSampler.MaxWidth - this.player.Width + this.player.Origin.X, 0);
        }

        // EnemyManager
        this.enemyManager.Update(gameTime, this.player, this.screenSampler.ScreenRectangle);

        // ParticleEngine
        this.particleEngine.Update(gameTime);

        // Modifiers
        for (int i = this.modifiers.Count - 1; i >= 0; i--)
        {
            // Update modifier
            this.modifiers[i].Update(gameTime);

            // Remove modifier if it's gone inactive
            if (!this.modifiers[i].IsActive)
                this.modifiers.RemoveAt(i);
        }

        // Active ActiveObjects that are close to the screen.
        this.ActivateObjects();

        // Update all active ActiveObjects
        for (int i = 0; i < this.activeObjects.Count; i++)
        {
            this.activeObjects[i].Update(gameTime);
        }

        this.UpdateScreenSampler();
    }

    private void UpdateScreenSampler()
    {
        Vector2 positionDelta = this.screenSampler.Position;

        // Only update the screen position if the player is still alive
        if (this.player.Health > 0)
        {
            // Update the position of the screen sampler
            this.screenSampler.Position = this.player.Position - new Vector2(this.screenSampler.Width / 2, this.screenSampler.Height / 2);

            positionDelta -= this.screenSampler.Position;

            // Background
            foreach (BackgroundLayer background in this.backgrounds)
                background.Update(positionDelta);
        }
    }

    /// <summary>
    /// Activates the ActiveObjects that are within a certain distance from the
    /// screen area. The distance is specified by SCREEN_ACTIVATION_DISTANCE.
    /// </summary>
    private void ActivateObjects()
    {
        Rectangle screenRect = new Rectangle((int)this.screenSampler.Position.X,
            (int)this.screenSampler.Position.Y,
            this.screenSampler.Width,
            this.screenSampler.Height);

        screenRect.Inflate(SCREEN_ACTIVATION_DISTANCE, SCREEN_ACTIVATION_DISTANCE);

        for (int i = this.inactiveObjects.Count - 1; i >= 0; i--)
        {
            ActiveObject obj = this.inactiveObjects[i];

            // Get the collision box of the object
            Rectangle collBox = obj.CollisionBox;

            // Set the position to be in level coordinates
            collBox.X += (int)obj.Position.X;
            collBox.Y += (int)obj.Position.Y;

            // Is the ActiveObject within the required distance from the screen
            if (collBox.Intersects(screenRect))
            {
                // Active the object
                obj.IsActive = true;
                // Add it to the list of activated objects
                this.activeObjects.Add(obj);
                this.inactiveObjects.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Checks for collisions between the currently active objects
    /// in the level.
    /// </summary>
    private void CheckCollisions()
    {
        // Player -> EnemyManager
        this.enemyManager.CheckCollisions(this.player);

        // Actors -> ParticleEngine
        this.particleEngine.CheckCollisions(this.player);
        this.particleEngine.CheckCollisions(this.enemyManager.ActivatedEnemies);

        // Actors -> ActiveObjects
        // Particles -> ActiveObjects
        for (int i = 0; i < this.activeObjects.Count; i++)
        {
            this.activeObjects[i].CheckCollision(this.player);
            this.activeObjects[i].CheckCollisions(this.enemyManager.ActivatedEnemies);
            this.activeObjects[i].CheckCollisions(this.particleEngine.Particles);
        }

        // ActiveObjects -> Particles
        this.particleEngine.CheckCollisions(this.activeObjects.ToArray());

        // ICollisionObjects  -> TileEngine
        for (int i = 0; i < this.tileEngines.Count; i++)
        {
            this.tileEngines[i].CheckCollision(this.player);
            this.tileEngines[i].CheckCollisions(this.particleEngine.Particles);
            this.tileEngines[i].CheckCollisions(this.enemyManager.ActivatedEnemies);
        }
    }

    /// <summary>
    /// Updates the animations of all actors. Doing this only once per update
    /// reduces the risk of flickering that can occur if an actor often switches
    /// between animations.
    /// </summary>
    private void UpdateAnimations()
    {
        // Only update the player's animation if the game is being played
        if (this.state == LevelState.Playing)
            this.player.UpdateAnimation();

        this.enemyManager.UpdateAnimations();
    }

    private void DrawLevel(SpriteBatch spriteBatch)
    {
        // Background
        int backgroundIndex;
        for (backgroundIndex = 0; backgroundIndex < this.backgrounds.Count && this.backgrounds[backgroundIndex].Depth <= 1; backgroundIndex++)
            this.backgrounds[backgroundIndex].Draw(spriteBatch, this.screenSampler);

        // TileEngine
        for (int i = this.tileEngines.Count - 1; i >= 0; i--)
            this.tileEngines[i].Draw(spriteBatch, this.screenSampler.Position);

        // ActiveObjects
        for (int i = 0; i < this.activeObjects.Count; i++)
            this.activeObjects[i].Draw(spriteBatch, this.screenSampler.Position);

        // Enemies
        this.enemyManager.Draw(spriteBatch, this.screenSampler.Position);

        // Player
        this.player.Draw(spriteBatch, this.screenSampler.Position);

        // ParticleEngine
        this.particleEngine.Draw(spriteBatch, this.screenSampler.Position);

        // Foreground
        for (; backgroundIndex < this.backgrounds.Count; backgroundIndex++)
            this.backgrounds[backgroundIndex].Draw(spriteBatch, this.screenSampler);

        // UI
        this.DrawUI(spriteBatch);
    }

    private void DrawUI(SpriteBatch spriteBatch)
    {
        // HEALTH BAR

        float playerHealthPercentage = this.player.Health / this.settings.PlayerStartHealth;
        Rectangle fullHealthSource = this.settings.FullHealthUI.SourceRectangle;
        // 48 is the start of the health indicator in x
        // 115 is the width of the health indicator
        fullHealthSource.Width = 48 + (int)(115 * playerHealthPercentage);

        // Draw empty health bar first
        spriteBatch.Draw(this.settings.EmptyHealthUI.Texture,
            Vector2.Zero,
            this.settings.EmptyHealthUI.SourceRectangle,
            Color.White);

        // Draw the visible part of the full health bar
        spriteBatch.Draw(this.settings.FullHealthUI.Texture,
            Vector2.Zero,
            fullHealthSource,
            Color.White);


        // WORLD AND LEVEL NUMBERS
        string indexString = this.worldIndex + " - " + this.levelIndex;
        Vector2 indexSize = this.settings.LevelIndexFont.MeasureString(indexString);
        Vector2 indexPos = new Vector2();
        indexPos.X = (this.screenSampler.Width - indexSize.X) / 2;
        indexPos.Y = UI_INDEX_POS_Y;

        spriteBatch.DrawString(this.settings.LevelIndexFont, indexString, indexPos, Color.White);

        // CURRENT WEAPON AND AMMONUTION COUNT
        Weapon playerWeapon = this.player.CurrentWeapon;

        if (playerWeapon != null)
        {
            // Ammo is drawn below the weapon sprite
            Vector2 ammoPos = new Vector2(this.screenSampler.Width - 20, 0);

            string ammoString;
            if (playerWeapon.InfiniteAmmo)
                ammoString = "Inf";
            else
                ammoString = playerWeapon.AmmoCount + " / " + playerWeapon.MaxAmmo;

            Vector2 ammoStringSize = this.settings.LevelIndexFont.MeasureString(ammoString);
            ammoPos.X -= ammoStringSize.X;

            // Draw ammo
            spriteBatch.DrawString(this.settings.LevelIndexFont, ammoString, ammoPos, Color.White);

            Vector2 weaponPos = new Vector2(470, 5);
            Sprite weaponSprite = playerWeapon.WeaponSprite;
            // Draw weapon sprite
            spriteBatch.Draw(weaponSprite.Texture,
                weaponPos,
                weaponSprite.SourceRectangle,
                Color.White);
        }

        // MODIFIERS
        // A little extra spacing is added (5 px)
        Vector2 modifierPos = new Vector2(5, this.settings.FullHealthUI.SourceRectangle.Height + 5);
        foreach (ModifierPickup modifier in this.modifiers)
        {
            modifier.DrawTimer(spriteBatch, modifierPos, this.settings.LevelIndexFont);
            modifierPos.Y += modifier.Icon.SourceRectangle.Height + UI_MODIFIER_SPACING;
        }
    }

    #endregion

    #region Session management

    public void SaveSession(GameSession session)
    {
        foreach (ModifierPickup modifier in this.modifiers)
            session.LevelModifiers.Add(new ModifierSave(modifier.ID, modifier.TimeRemaining));

        foreach (ActiveObject activeObject in this.activeObjects)
            session.ActivatedObjects.Add(activeObject.UniqueID, new ActiveObjectSave(activeObject.Position, activeObject.IsActive));

        this.enemyManager.SaveSession(session);
    }

    public void LoadSession(GameSession session)
    {
        // Make a soft reset to prepare for loading the new session
        this.SoftReset();

        // Get all active modifiers
        foreach (ModifierSave modifier in session.LevelModifiers)
        {
            ModifierPickup levelMod = this.modifierLibrary.GetPickup(modifier.ID) as ModifierPickup;
            levelMod.Level = this;
            levelMod.TimeRemaining = modifier.TimeLeft;
            levelMod.IsActive = true;
            this.modifiers.Add(levelMod);
        }

        // Inactivate all active objects
        for (int i = this.activeObjects.Count - 1; i >= 0; i--)
        {
            this.activeObjects[i].Reset();
            this.inactiveObjects.Add(this.activeObjects[i]);
            this.activeObjects.RemoveAt(i);
        }

        // Activate ActiveObjects
        for (int i = this.inactiveObjects.Count - 1; i >= 0; i--)
        {
            if (session.ActivatedObjects.ContainsKey(this.inactiveObjects[i].UniqueID))
            {
                // Move the object to the list of actived objects
                ActiveObject activeObj = this.inactiveObjects[i];
                this.inactiveObjects.RemoveAt(i);
                this.activeObjects.Add(activeObj);

                // Setup object
                ActiveObjectSave save = session.ActivatedObjects[activeObj.UniqueID];
                activeObj.Position = save.Position;
                activeObj.IsActive = save.IsActive;
            }
            else
                // Since the load of a new session is only prepared for
                // with a soft reset, there could be active objects that where
                // activated that should inactive. Therefore all ActiveObjects
                // not affected by the new session are reset
                this.inactiveObjects[i].Reset();
        }

        this.enemyManager.LoadSession(session);

        // Have the screensampler move to the players position
        this.UpdateScreenSampler();

        // Update animations so that they reflect the state of the player and enemies
        this.UpdateAnimations();

        this.state = this.nextState = LevelState.Playing;
    }

    #endregion
}