using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.ActiveObjects;
using PhysicaliaRemastered.Actors;
using PhysicaliaRemastered.Actors.EnemyManagement;
using PhysicaliaRemastered.Graphics;
using PhysicaliaRemastered.Input;
using PhysicaliaRemastered.Pickups;
using PhysicaliaRemastered.Weapons;
using XNALibrary.Graphics;
using XNALibrary.Graphics.Sprites;
using XNALibrary.Graphics.TileEngine;
using XNALibrary.Interfaces;

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
    // The start position of the UI in the y-axis
    private const float UI_INDEX_POS_Y = 10F;
    private const float UI_MODIFIER_SPACING = 5F;

    private const int SCREEN_ACTIVATION_DISTANCE = 20;

    private const float PLAYER_FINISH_SLOWDOWN = 0.95F;

    private int levelIndex;
    private int worldIndex;
    private ISettings settings;
    private Game game;

    // States
    private LevelState state;
    private LevelState nextState;

    public LevelState State
    {
        get { return state; }
    }

    public LevelState NextState
    {
        get { return nextState; }
        set { nextState = value; }
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

    public int WorldIndex
    {
        get { return worldIndex; }
        set { worldIndex = value; }
    }

    public int LevelIndex
    {
        get { return levelIndex; }
        set { levelIndex = value; }
    }

    public Player Player
    {
        get { return player; }
        set { player = value; }
    }

    public IAnimationManager AnimationManager
    {
        get { return animationManager; }
        set { animationManager = value; }
    }

    public ISpriteLibrary SpriteLibrary
    {
        get { return spriteLibrary; }
        set { spriteLibrary = value; }
    }

    public IParticleEngine ParticleEngine
    {
        get { return particleEngine; }
        set { particleEngine = value; }
    }

    public ISettings Settings
    {
        get { return settings; }
    }

    public EnemyManager EnemyManager
    {
        get { return enemyManager; }
        set { enemyManager = value; }
    }

    public ScreenSampler ScreenSampler
    {
        get { return screenSampler; }
    }

    public Level(Game game, Player player)
    {
        this.game = game;

        backgrounds = new List<BackgroundLayer>();

        // Get needed services
        settings = (ISettings)game.Services.GetService(typeof(ISettings));
        animationManager = (IAnimationManager)game.Services.GetService(typeof(IAnimationManager));
        spriteLibrary = (ISpriteLibrary)game.Services.GetService(typeof(ISpriteLibrary));
        enemyManager = new EnemyManager((IEnemyBank)this.game.Services.GetService(typeof(IEnemyBank)));
        weaponBank = (IWeaponBank)this.game.Services.GetService(typeof(IWeaponBank));
        modifierLibrary = (IPickupLibrary)this.game.Services.GetService(typeof(IPickupLibrary));

        this.player = player;
        playerStartValues = new ActorStartValues();
        nextState = state = LevelState.Start;
        particleEngine = (IParticleEngine)game.Services.GetService(typeof(IParticleEngine));
        screenSampler = new ScreenSampler(game, 0, 0, this.game.GraphicsDevice.Viewport.Width, this.game.GraphicsDevice.Viewport.Height);
        tileEngines = new List<TileEngine>();
        modifiers = new List<ModifierPickup>();

        activeObjects = new List<ActiveObject>();
        inactiveObjects = new List<ActiveObject>();
    }

    public void Update(GameTime gameTime)
    {
        // Switch based on current state
        switch (state)
        {
            case LevelState.Start:
                if (settings.InputMap.IsPressed(InputAction.MenuStart))
                    nextState = LevelState.Playing;
                break;
            case LevelState.Playing:
                UpdateLevel(gameTime);
                CheckCollisions();
                player.HandleInput();
                UpdateAnimations();

                // See if the player has fallen out of the level
                if (PlayerOutsideLevel())
                {
                    player.Kill();

                    // Do a little jump into screen
                    player.Velocity = new Vector2(0, player.JumpMagnitude * Math.Sign(player.Acceleration.Y));
                    nextState = LevelState.Dead;
                }

                // Check if the player's died
                if (player.Health <= 0)
                {
                    player.CanCollide = false;
                    player.Velocity = new Vector2(0, player.JumpMagnitude * Math.Sign(player.Acceleration.Y));

                    nextState = LevelState.Dead;
                }
                break;
            case LevelState.Dead:
                // Continue updating the Level after death, just don't check input
                UpdateLevel(gameTime);
                CheckCollisions();
                UpdateAnimations();

                // Don't let the player continue falling if it's outside of the level (i.e. falling)
                if (player.Velocity != Vector2.Zero &&
                    PlayerOffScreen() &&
                    player.Velocity.Y / player.Acceleration.Y > 0)
                {
                    player.Velocity = Vector2.Zero;
                    player.Acceleration = Vector2.Zero;
                }
                break;
            case LevelState.Finished:
                // Don't let the player continue falling if it's outside of the level (i.e. falling)
                if (player.Velocity != Vector2.Zero &&
                    PlayerOffScreen() &&
                    player.Velocity.Y / player.Acceleration.Y > 0)
                {
                    player.Velocity = Vector2.Zero;
                    player.Acceleration = Vector2.Zero;
                }

                // Slow down the player's movement in X if needed
                if (player.Velocity.X != 0)
                {
                    player.Velocity *= new Vector2(PLAYER_FINISH_SLOWDOWN, 1F);

                    // Set velocity to zero if it's very close
                    if (player.Velocity.X > -1 && player.Velocity.X < 1)
                        player.Velocity *= Vector2.UnitY;
                }

                UpdateLevel(gameTime);
                CheckCollisions();
                UpdateAnimations();
                break;
        }

        // Should the state be changed?
        if (nextState != state)
            ChangeState();
    }

    public void LoadXml(string path, ITileLibrary tileLibrary)
    {
        using (XmlReader reader = XmlReader.Create(path))
        {
            LoadXml(reader, tileLibrary);
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

                screenSampler.MaxWidth = width;
                screenSampler.MaxHeight = height;
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "PlayerStart")
                playerStartValues = ActorStartValues.FromXml(reader, "PlayerStart");

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Enemies")
                LoadEnemies(reader);

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "ActiveObjects")
                LoadActiveObjects(reader);

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Background")
            {
                int spriteKey = int.Parse(reader.GetAttribute("spriteKey"));
                float depth = float.Parse(reader.GetAttribute("depth"));
                string loopString = reader.GetAttribute("loop");
                bool loopX = loopString.Contains("x"); bool loopY = loopString.Contains("y");

                Sprite sprite = spriteLibrary.GetSprite(spriteKey);
                BackgroundLayer background = new BackgroundLayer(sprite, depth);
                background.LoopX = loopX; background.LoopY = loopY;

                if (!reader.IsEmptyElement)
                {
                    reader.ReadToFollowing("Position");
                    float x = float.Parse(reader.GetAttribute("x"));
                    float y = float.Parse(reader.GetAttribute("y"));
                    background.StartPosition = new Vector2(x, y);
                }

                backgrounds.Add(background);
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "Backgrounds")
            {
                backgrounds.Sort(BackgroundLayer.Compare);
            }

            // TileEngines?
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "TileEngines")
            {
                int engineCount = int.Parse(reader.GetAttribute("count"));

                for (int i = 0; i < engineCount; i++)
                {
                    // Add a new TileEngine
                    tileEngines.Add(new TileEngine(tileLibrary, 32, 32));

                    TileEngine tileEngine = tileEngines[i];

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

                enemyManager.EnqueueEnemy(type, startValues, patrolArea);
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

                LoadWeapons(reader);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Pickups")
            {
                if (reader.IsEmptyElement)
                    continue;

                LoadPickups(reader);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "LevelFinishTrigger")
            {
                int x = int.Parse(reader.GetAttribute("x"));
                int y = int.Parse(reader.GetAttribute("y"));

                Sprite triggerSprite = spriteLibrary.GetSprite(-1);
                EndLevelTrigger trigger = new EndLevelTrigger(this, triggerSprite);

                PickupContainer cont = new PickupContainer(trigger);
                cont.Position = new Vector2(x, y);
                cont.CollisionBox = new Rectangle(0, 0, triggerSprite.SourceRectangle.Width, triggerSprite.SourceRectangle.Height);
                cont.CanCollide = true;
                cont.IsActive = false;

                EnqueueActiveObject(cont);
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
                Weapon weapon = weaponBank.GetWeapon(key);

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

                EnqueueActiveObject(pickupCont);
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

                Pickup pickup = modifierLibrary.GetPickup(key);
                pickup.Level = this;

                PickupContainer cont = new PickupContainer(pickup);
                cont.Position = new Vector2(x, y);
                cont.CollisionBox = new Rectangle(0, 0, pickup.Sprite.SourceRectangle.Width, pickup.Sprite.SourceRectangle.Height);
                EnqueueActiveObject(cont);
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "Pickups")
                return;
        }
    }

    /// <summary>
    /// Resets the Level to its initial state.
    /// </summary>
    public void Reset()
    {
        // Reset background layers
        foreach (BackgroundLayer background in backgrounds)
            background.Position = background.StartPosition;

        // Reset enemies
        enemyManager.Reset();

        // Set and apply the start values of the player
        player.StartValues = playerStartValues;
        player.ApplyStartValues();
        player.CanCollide = true;
        player.CanTakeDamage = true;

        player.Health = settings.PlayerStartHealth;

        // Retrive the ammo count the player's weapons had at the start of the level
        player.ApplyStoredWeaponAmmoCount();

        // Set the start position of the screen sampler
        screenSampler.Position = Vector2.Zero;

        while (activeObjects.Count > 0)
        {
            activeObjects[0].Reset();
            EnqueueActiveObject(activeObjects[0]);
            activeObjects.RemoveAt(0);
        }

        // Clear out any modifiers
        modifiers.Clear();

        // Set the state of the level
        nextState = state = LevelState.Start;

        // Run a check so that any objects that's on screen is activated
        ActivateObjects();
    }

    /// <summary>
    /// Makes a soft reset of the level. Soft meaning that the player's weapons
    /// wont be taken from him and his position wont be touched. The level's
    /// state will also not be taken care of.
    /// </summary>
    public void SoftReset()
    {
        // Reset background layers
        foreach (BackgroundLayer background in backgrounds)
            background.Position = background.StartPosition;

        // Reset enemies
        enemyManager.Reset();

        player.CanCollide = true;
        player.CanTakeDamage = true;

        // Set the start position of the screen sampler
        screenSampler.Position = Vector2.Zero;

        while (activeObjects.Count > 0)
        {
            activeObjects[0].Reset();
            EnqueueActiveObject(activeObjects[0]);
            activeObjects.RemoveAt(0);
        }

        // Clear out any modifiers
        modifiers.Clear();

        // Run a check so that any objects that's on screen is activated
        ActivateObjects();
        enemyManager.ActivateVisible(screenSampler.ScreenRectangle);
    }

    /// <summary>
    /// Adds a modifer to the Level's collection of modifiers.
    /// </summary>
    /// <param name="modifier">The modifier to add.</param>
    public void AddModifier(ModifierPickup modifier)
    {
        modifiers.Add(modifier);
    }

    /// <summary>
    /// Adds an ActiveObject to the Level's list of ActiveObjects.
    /// </summary>
    /// <param name="obj">ActiveObject to add.</param>
    public void AddActiveObject(ActiveObject obj)
    {
        activeObjects.Add(obj);
    }

    /// <summary>
    /// Enqueues an ActiveObject to the Level's Queue of ActiveObjects.
    /// </summary>
    /// <param name="obj">ActiveObject to enqueue.</param>
    public void EnqueueActiveObject(ActiveObject obj)
    {
        inactiveObjects.Add(obj);
    }

    /// <summary>
    /// Draws the level.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to use for drawing.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        switch (state)
        {
            case LevelState.Start:
                string indexString = WorldIndex + " - " + levelIndex;
                Vector2 indexStringSize = settings.LevelIndexFont.MeasureString(indexString);
                Vector2 indexPosition = new Vector2();
                indexPosition.X = (screenSampler.Width - indexStringSize.X) / 2;
                indexPosition.Y = (screenSampler.Height - indexStringSize.Y) / 2;

                spriteBatch.DrawString(settings.LevelIndexFont, indexString, indexPosition, Color.White);
                break;
            case LevelState.Playing:
                DrawLevel(spriteBatch);
                break;
            case LevelState.Dead:
                DrawLevel(spriteBatch);

                String deadString = "You Have Died!";
                Vector2 deadStringSize = settings.PlayerDeadFont.MeasureString(deadString);
                Vector2 deadPos;
                deadPos.X = (screenSampler.Width - deadStringSize.X) / 2;
                deadPos.Y = (screenSampler.Width - deadStringSize.X) / 2;

                spriteBatch.DrawString(settings.PlayerDeadFont, deadString, deadPos, Color.White);
                break;
            case LevelState.Finished:
                DrawLevel(spriteBatch);

                String finishString = "Level Finished!";
                Vector2 finishStringSize = settings.PlayerDeadFont.MeasureString(finishString);
                Vector2 finishPos;
                finishPos.X = (screenSampler.Width - finishStringSize.X) / 2;
                finishPos.Y = (screenSampler.Width - finishStringSize.X) / 2;

                spriteBatch.DrawString(settings.PlayerDeadFont, finishString, finishPos, Color.White);
                break;
        }
    }

    /// <summary>
    /// Changes the current state of the level to the next selected one. Code
    /// related to the change between different states should be placed here.
    /// </summary>
    private void ChangeState()
    {
        switch (state)
        {
            case LevelState.Start:
                break;
            case LevelState.Playing:
                if (player.CurrentWeapon != null &&
                    player.CurrentWeapon.IsFiring)
                    player.CurrentWeapon.Stop();
                break;
            case LevelState.Dead:
                break;
            case LevelState.Finished:
                break;
        }

        state = nextState;

        switch (nextState)
        {
            case LevelState.Start:
                break;
            case LevelState.Playing:
                // Store the current ammo count of the player's retrieved weapons
                player.StoreWeaponAmmoCount();
                break;
            case LevelState.Dead:
                break;
            case LevelState.Finished:
                // Store the current ammo count of the player's retrieved weapons
                player.StoreWeaponAmmoCount();
                player.CurrentAnimationType = (int)ActorAnimation.Win;

                player.CanTakeDamage = false;
                break;
        }
    }

    private bool PlayerOffScreen()
    {
        Rectangle playerRect = player.CurrentAnimation.SourceRectangle;
        playerRect.X = (int)(player.Position.X - player.Origin.X);
        playerRect.Y = (int)(player.Position.Y - player.Origin.Y);

        return !screenSampler.ScreenRectangle.Intersects(playerRect);
    }

    /// <summary>
    /// Checks whether the player is outside of the Level area.
    /// </summary>
    /// <returns>True if the player is outside of the Level; false otherwise.</returns>
    private bool PlayerOutsideLevel()
    {
        bool result = false;

        Rectangle levelRect = new Rectangle();
        levelRect.Width = screenSampler.MaxWidth;
        levelRect.Height = screenSampler.MaxHeight;

        Rectangle playerRect = player.CurrentAnimation.SourceRectangle;
        playerRect.X = (int)(player.Position.X - player.Origin.X);
        playerRect.Y = (int)(player.Position.Y - player.Origin.Y);

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
        player.Update(gameTime);

        // Make sure the player's right and left edges are within the screen

        // Right edge
        if (player.Position.X < player.Origin.X)
        {
            player.Position *= Vector2.UnitY;
            player.Position += new Vector2(player.Origin.X, 0);
        }

        // Left edge
        if (player.Position.X - player.Origin.X + player.Width > screenSampler.MaxWidth)
        {
            player.Position *= Vector2.UnitY;
            player.Position += new Vector2(screenSampler.MaxWidth - player.Width + player.Origin.X, 0);
        }

        // EnemyManager
        enemyManager.Update(gameTime, player, screenSampler.ScreenRectangle);

        // ParticleEngine
        particleEngine.Update(gameTime);

        // Modifiers
        for (int i = modifiers.Count - 1; i >= 0; i--)
        {
            // Update modifier
            modifiers[i].Update(gameTime);

            // Remove modifier if it's gone inactive
            if (!modifiers[i].IsActive)
                modifiers.RemoveAt(i);
        }

        // Active ActiveObjects that are close to the screen.
        ActivateObjects();

        // Update all active ActiveObjects
        for (int i = 0; i < activeObjects.Count; i++)
        {
            activeObjects[i].Update(gameTime);
        }

        UpdateScreenSampler();
    }

    private void UpdateScreenSampler()
    {
        Vector2 positionDelta = screenSampler.Position;

        // Only update the screen position if the player is still alive
        if (player.Health > 0)
        {
            // Update the position of the screen sampler
            screenSampler.Position = player.Position - new Vector2(screenSampler.Width / 2, screenSampler.Height / 2);

            positionDelta -= screenSampler.Position;

            // Background
            foreach (BackgroundLayer background in backgrounds)
                background.Update(positionDelta);
        }
    }

    /// <summary>
    /// Activates the ActiveObjects that are within a certain distance from the
    /// screen area. The distance is specified by SCREEN_ACTIVATION_DISTANCE.
    /// </summary>
    private void ActivateObjects()
    {
        Rectangle screenRect = new Rectangle((int)screenSampler.Position.X,
            (int)screenSampler.Position.Y,
            screenSampler.Width,
            screenSampler.Height);

        screenRect.Inflate(SCREEN_ACTIVATION_DISTANCE, SCREEN_ACTIVATION_DISTANCE);

        for (int i = inactiveObjects.Count - 1; i >= 0; i--)
        {
            ActiveObject obj = inactiveObjects[i];

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
                activeObjects.Add(obj);
                inactiveObjects.RemoveAt(i);
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
        enemyManager.CheckCollisions(player);

        // Actors -> ParticleEngine
        particleEngine.CheckCollisions(player);
        particleEngine.CheckCollisions(enemyManager.ActivatedEnemies);

        // Actors -> ActiveObjects
        // Particles -> ActiveObjects
        for (int i = 0; i < activeObjects.Count; i++)
        {
            activeObjects[i].CheckCollision(player);
            activeObjects[i].CheckCollisions(enemyManager.ActivatedEnemies);
            activeObjects[i].CheckCollisions(particleEngine.Particles);
        }

        // ActiveObjects -> Particles
        particleEngine.CheckCollisions(activeObjects.ToArray());

        // ICollisionObjects  -> TileEngine
        for (int i = 0; i < tileEngines.Count; i++)
        {
            tileEngines[i].CheckCollision(player);
            tileEngines[i].CheckCollisions(particleEngine.Particles);
            tileEngines[i].CheckCollisions(enemyManager.ActivatedEnemies);
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
        if (state == LevelState.Playing)
            player.UpdateAnimation();

        enemyManager.UpdateAnimations();
    }

    private void DrawLevel(SpriteBatch spriteBatch)
    {
        // Background
        int backgroundIndex;
        for (backgroundIndex = 0; backgroundIndex < backgrounds.Count && backgrounds[backgroundIndex].Depth <= 1; backgroundIndex++)
            backgrounds[backgroundIndex].Draw(spriteBatch, screenSampler);

        // TileEngine
        for (int i = tileEngines.Count - 1; i >= 0; i--)
            tileEngines[i].Draw(spriteBatch, screenSampler.Position);

        // ActiveObjects
        for (int i = 0; i < activeObjects.Count; i++)
            activeObjects[i].Draw(spriteBatch, screenSampler.Position);

        // Enemies
        enemyManager.Draw(spriteBatch, screenSampler.Position);

        // Player
        player.Draw(spriteBatch, screenSampler.Position);

        // ParticleEngine
        particleEngine.Draw(spriteBatch, screenSampler.Position);

        // Foreground
        for (; backgroundIndex < backgrounds.Count; backgroundIndex++)
            backgrounds[backgroundIndex].Draw(spriteBatch, screenSampler);

        // UI
        DrawUI(spriteBatch);
    }

    private void DrawUI(SpriteBatch spriteBatch)
    {
        // HEALTH BAR

        float playerHealthPercentage = player.Health / settings.PlayerStartHealth;
        Rectangle fullHealthSource = settings.FullHealthUI.SourceRectangle;
        // 48 is the start of the health indicator in x
        // 115 is the width of the health indicator
        fullHealthSource.Width = 48 + (int)(115 * playerHealthPercentage);

        // Draw empty health bar first
        spriteBatch.Draw(settings.EmptyHealthUI.Texture,
            Vector2.Zero,
            settings.EmptyHealthUI.SourceRectangle,
            Color.White);

        // Draw the visible part of the full health bar
        spriteBatch.Draw(settings.FullHealthUI.Texture,
            Vector2.Zero,
            fullHealthSource,
            Color.White);


        // WORLD AND LEVEL NUMBERS
        string indexString = worldIndex + " - " + levelIndex;
        Vector2 indexSize = settings.LevelIndexFont.MeasureString(indexString);
        Vector2 indexPos = new Vector2();
        indexPos.X = (screenSampler.Width - indexSize.X) / 2;
        indexPos.Y = UI_INDEX_POS_Y;

        spriteBatch.DrawString(settings.LevelIndexFont, indexString, indexPos, Color.White);

        // CURRENT WEAPON AND AMMONUTION COUNT
        Weapon playerWeapon = player.CurrentWeapon;

        if (playerWeapon != null)
        {
            // Ammo is drawn below the weapon sprite
            Vector2 ammoPos = new Vector2(screenSampler.Width - 20, 0);

            string ammoString;
            if (playerWeapon.InfiniteAmmo)
                ammoString = "Inf";
            else
                ammoString = playerWeapon.AmmoCount + " / " + playerWeapon.MaxAmmo;

            Vector2 ammoStringSize = settings.LevelIndexFont.MeasureString(ammoString);
            ammoPos.X -= ammoStringSize.X;

            // Draw ammo
            spriteBatch.DrawString(settings.LevelIndexFont, ammoString, ammoPos, Color.White);

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
        Vector2 modifierPos = new Vector2(5, settings.FullHealthUI.SourceRectangle.Height + 5);
        foreach (ModifierPickup modifier in modifiers)
        {
            modifier.DrawTimer(spriteBatch, modifierPos, settings.LevelIndexFont);
            modifierPos.Y += modifier.Icon.SourceRectangle.Height + UI_MODIFIER_SPACING;
        }
    }

    public void SaveSession(GameSession session)
    {
        foreach (ModifierPickup modifier in modifiers)
            session.LevelModifiers.Add(new ModifierSave(modifier.ID, modifier.TimeRemaining));

        foreach (ActiveObject activeObject in activeObjects)
            session.ActivatedObjects.Add(activeObject.UniqueID, new ActiveObjectSave(activeObject.Position, activeObject.IsActive));

        enemyManager.SaveSession(session);
    }

    public void LoadSession(GameSession session)
    {
        // Make a soft reset to prepare for loading the new session
        SoftReset();

        // Get all active modifiers
        foreach (ModifierSave modifier in session.LevelModifiers)
        {
            ModifierPickup levelMod = modifierLibrary.GetPickup(modifier.ID) as ModifierPickup;
            levelMod.Level = this;
            levelMod.TimeRemaining = modifier.TimeLeft;
            levelMod.IsActive = true;
            modifiers.Add(levelMod);
        }

        // Inactivate all active objects
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            activeObjects[i].Reset();
            inactiveObjects.Add(activeObjects[i]);
            activeObjects.RemoveAt(i);
        }

        // Activate ActiveObjects
        for (int i = inactiveObjects.Count - 1; i >= 0; i--)
        {
            if (session.ActivatedObjects.ContainsKey(inactiveObjects[i].UniqueID))
            {
                // Move the object to the list of actived objects
                ActiveObject activeObj = inactiveObjects[i];
                inactiveObjects.RemoveAt(i);
                activeObjects.Add(activeObj);

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
                inactiveObjects[i].Reset();
        }

        enemyManager.LoadSession(session);

        // Have the screensampler move to the players position
        UpdateScreenSampler();

        // Update animations so that they reflect the state of the player and enemies
        UpdateAnimations();

        state = nextState = LevelState.Playing;
    }
}