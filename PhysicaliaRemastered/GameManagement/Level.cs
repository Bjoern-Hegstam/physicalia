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
using PhysicaliaRemastered.Weapons.NewWeapons;
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
    private const float UiIndexPosY = 10F;
    private const float UiModifierSpacing = 5F;

    private const int ScreenActivationDistance = 20;

    private const float PlayerFinishSlowdown = 0.95F;

    private readonly Game _game;

    // States

    public LevelState State { get; private set; }

    public LevelState NextState { get; set; }

    // View
    private readonly List<BackgroundLayer> _backgrounds;

    // Gameplay
    private readonly List<ModifierPickup> _modifiers;
    private readonly List<TileEngine> _tileEngines;
    private ActorStartValues _playerStartValues;
    private readonly IWeaponBank _weaponBank;
    private readonly IPickupLibrary _modifierLibrary;

    // ActiveObjects

    // Lists of ActiveObjects
    private readonly List<ActiveObject> _activeObjects;
    private readonly List<ActiveObject> _inactiveObjects;

    public int WorldIndex { get; set; }

    public int LevelIndex { get; set; }

    public Player Player { get; set; }

    public IAnimationManager AnimationManager { get; set; }

    public ISpriteLibrary SpriteLibrary { get; set; }

    public IParticleEngine ParticleEngine { get; set; }

    public ISettings Settings { get; }

    public EnemyManager EnemyManager { get; set; }

    public ScreenSampler ScreenSampler { get; }

    public Level(Game game, Player player)
    {
        _game = game;

        _backgrounds = new List<BackgroundLayer>();

        // Get needed services
        Settings = (ISettings)game.Services.GetService(typeof(ISettings));
        AnimationManager = (IAnimationManager)game.Services.GetService(typeof(IAnimationManager));
        SpriteLibrary = (ISpriteLibrary)game.Services.GetService(typeof(ISpriteLibrary));
        EnemyManager = new EnemyManager((IEnemyBank)_game.Services.GetService(typeof(IEnemyBank)));
        _weaponBank = (IWeaponBank)_game.Services.GetService(typeof(IWeaponBank));
        _modifierLibrary = (IPickupLibrary)_game.Services.GetService(typeof(IPickupLibrary));

        Player = player;
        _playerStartValues = new ActorStartValues();
        NextState = State = LevelState.Start;
        ParticleEngine = (IParticleEngine)game.Services.GetService(typeof(IParticleEngine));
        ScreenSampler = new ScreenSampler(game, 0, 0, _game.GraphicsDevice.Viewport.Width,
            _game.GraphicsDevice.Viewport.Height);
        _tileEngines = new List<TileEngine>();
        _modifiers = new List<ModifierPickup>();

        _activeObjects = new List<ActiveObject>();
        _inactiveObjects = new List<ActiveObject>();
    }

    public void Update(GameTime gameTime)
    {
        // Switch based on current state
        switch (State)
        {
            case LevelState.Start:
                if (Settings.InputMap.IsPressed(InputAction.MenuStart))
                    NextState = LevelState.Playing;
                break;
            case LevelState.Playing:
                UpdateLevel(gameTime);
                CheckCollisions();
                Player.HandleInput();
                UpdateAnimations();

                // See if the player has fallen out of the level
                if (PlayerOutsideLevel())
                {
                    Player.Kill();

                    // Do a little jump into screen
                    Player.Velocity = new Vector2(0, Player.JumpMagnitude * Math.Sign(Player.Acceleration.Y));
                    NextState = LevelState.Dead;
                }

                // Check if the player's died
                if (Player.Health <= 0)
                {
                    Player.CanCollide = false;
                    Player.Velocity = new Vector2(0, Player.JumpMagnitude * Math.Sign(Player.Acceleration.Y));

                    NextState = LevelState.Dead;
                }

                break;
            case LevelState.Dead:
                // Continue updating the Level after death, just don't check input
                UpdateLevel(gameTime);
                CheckCollisions();
                UpdateAnimations();

                // Don't let the player continue falling if it's outside of the level (i.e. falling)
                if (Player.Velocity != Vector2.Zero &&
                    PlayerOffScreen() &&
                    Player.Velocity.Y / Player.Acceleration.Y > 0)
                {
                    Player.Velocity = Vector2.Zero;
                    Player.Acceleration = Vector2.Zero;
                }

                break;
            case LevelState.Finished:
                // Don't let the player continue falling if it's outside of the level (i.e. falling)
                if (Player.Velocity != Vector2.Zero &&
                    PlayerOffScreen() &&
                    Player.Velocity.Y / Player.Acceleration.Y > 0)
                {
                    Player.Velocity = Vector2.Zero;
                    Player.Acceleration = Vector2.Zero;
                }

                // Slow down the player's movement in X if needed
                if (Player.Velocity.X != 0)
                {
                    Player.Velocity *= new Vector2(PlayerFinishSlowdown, 1F);

                    // Set velocity to zero if it's very close
                    if (Player.Velocity.X > -1 && Player.Velocity.X < 1)
                        Player.Velocity *= Vector2.UnitY;
                }

                UpdateLevel(gameTime);
                CheckCollisions();
                UpdateAnimations();
                break;
        }

        // Should the state be changed?
        if (NextState != State)
            ChangeState();
    }

    public void LoadXml(string path, ITileLibrary tileLibrary)
    {
        using XmlReader reader = XmlReader.Create(path);
        LoadXml(reader, tileLibrary);
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

                ScreenSampler.MaxWidth = width;
                ScreenSampler.MaxHeight = height;
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "PlayerStart")
                _playerStartValues = ActorStartValues.FromXml(reader, "PlayerStart");

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
                bool loopX = loopString.Contains("x");
                bool loopY = loopString.Contains("y");

                Sprite sprite = SpriteLibrary.GetSprite(spriteKey);
                BackgroundLayer background = new BackgroundLayer(sprite, depth);
                background.LoopX = loopX;
                background.LoopY = loopY;

                if (!reader.IsEmptyElement)
                {
                    reader.ReadToFollowing("Position");
                    float x = float.Parse(reader.GetAttribute("x"));
                    float y = float.Parse(reader.GetAttribute("y"));
                    background.StartPosition = new Vector2(x, y);
                }

                _backgrounds.Add(background);
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "Backgrounds")
            {
                _backgrounds.Sort(BackgroundLayer.Compare);
            }

            // TileEngines?
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "TileEngines")
            {
                int engineCount = int.Parse(reader.GetAttribute("count"));

                for (int i = 0; i < engineCount; i++)
                {
                    // Add a new TileEngine
                    _tileEngines.Add(new TileEngine(tileLibrary, 32, 32));

                    TileEngine tileEngine = _tileEngines[i];

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

                EnemyManager.EnqueueEnemy(type, startValues, patrolArea);
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

                Sprite triggerSprite = SpriteLibrary.GetSprite(-1);
                EndLevelTrigger trigger = new EndLevelTrigger(this, triggerSprite);

                PickupContainer cont = new PickupContainer(trigger);
                cont.Position = new Vector2(x, y);
                cont.CollisionBox = new Rectangle(0, 0, triggerSprite.SourceRectangle.Width,
                    triggerSprite.SourceRectangle.Height);
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
                Weapon weapon = _weaponBank.GetWeapon(key);

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

                Pickup pickup = _modifierLibrary.GetPickup(key);
                pickup.Level = this;

                PickupContainer cont = new PickupContainer(pickup);
                cont.Position = new Vector2(x, y);
                cont.CollisionBox = new Rectangle(0, 0, pickup.Sprite.SourceRectangle.Width,
                    pickup.Sprite.SourceRectangle.Height);
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
        foreach (BackgroundLayer background in _backgrounds)
            background.Position = background.StartPosition;

        // Reset enemies
        EnemyManager.Reset();

        // Set and apply the start values of the player
        Player.StartValues = _playerStartValues;
        Player.ApplyStartValues();
        Player.CanCollide = true;
        Player.CanTakeDamage = true;

        Player.Health = Settings.PlayerStartHealth;

        // Retrive the ammo count the player's weapons had at the start of the level
        Player.ApplyStoredWeaponAmmoCount();

        // Set the start position of the screen sampler
        ScreenSampler.Position = Vector2.Zero;

        while (_activeObjects.Count > 0)
        {
            _activeObjects[0].Reset();
            EnqueueActiveObject(_activeObjects[0]);
            _activeObjects.RemoveAt(0);
        }

        // Clear out any modifiers
        _modifiers.Clear();

        // Set the state of the level
        NextState = State = LevelState.Start;

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
        foreach (BackgroundLayer background in _backgrounds)
            background.Position = background.StartPosition;

        // Reset enemies
        EnemyManager.Reset();

        Player.CanCollide = true;
        Player.CanTakeDamage = true;

        // Set the start position of the screen sampler
        ScreenSampler.Position = Vector2.Zero;

        while (_activeObjects.Count > 0)
        {
            _activeObjects[0].Reset();
            EnqueueActiveObject(_activeObjects[0]);
            _activeObjects.RemoveAt(0);
        }

        // Clear out any modifiers
        _modifiers.Clear();

        // Run a check so that any objects that's on screen is activated
        ActivateObjects();
        EnemyManager.ActivateVisible(ScreenSampler.ScreenRectangle);
    }

    /// <summary>
    /// Adds a modifer to the Level's collection of modifiers.
    /// </summary>
    /// <param name="modifier">The modifier to add.</param>
    public void AddModifier(ModifierPickup modifier)
    {
        _modifiers.Add(modifier);
    }

    /// <summary>
    /// Adds an ActiveObject to the Level's list of ActiveObjects.
    /// </summary>
    /// <param name="obj">ActiveObject to add.</param>
    public void AddActiveObject(ActiveObject obj)
    {
        _activeObjects.Add(obj);
    }

    /// <summary>
    /// Enqueues an ActiveObject to the Level's Queue of ActiveObjects.
    /// </summary>
    /// <param name="obj">ActiveObject to enqueue.</param>
    public void EnqueueActiveObject(ActiveObject obj)
    {
        _inactiveObjects.Add(obj);
    }

    /// <summary>
    /// Draws the level.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to use for drawing.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        switch (State)
        {
            case LevelState.Start:
                string indexString = WorldIndex + " - " + LevelIndex;
                Vector2 indexStringSize = Settings.LevelIndexFont.MeasureString(indexString);
                Vector2 indexPosition = new Vector2();
                indexPosition.X = (ScreenSampler.Width - indexStringSize.X) / 2;
                indexPosition.Y = (ScreenSampler.Height - indexStringSize.Y) / 2;

                spriteBatch.DrawString(Settings.LevelIndexFont, indexString, indexPosition, Color.White);
                break;
            case LevelState.Playing:
                DrawLevel(spriteBatch);
                break;
            case LevelState.Dead:
                DrawLevel(spriteBatch);

                String deadString = "You Have Died!";
                Vector2 deadStringSize = Settings.PlayerDeadFont.MeasureString(deadString);
                Vector2 deadPos;
                deadPos.X = (ScreenSampler.Width - deadStringSize.X) / 2;
                deadPos.Y = (ScreenSampler.Width - deadStringSize.X) / 2;

                spriteBatch.DrawString(Settings.PlayerDeadFont, deadString, deadPos, Color.White);
                break;
            case LevelState.Finished:
                DrawLevel(spriteBatch);

                String finishString = "Level Finished!";
                Vector2 finishStringSize = Settings.PlayerDeadFont.MeasureString(finishString);
                Vector2 finishPos;
                finishPos.X = (ScreenSampler.Width - finishStringSize.X) / 2;
                finishPos.Y = (ScreenSampler.Width - finishStringSize.X) / 2;

                spriteBatch.DrawString(Settings.PlayerDeadFont, finishString, finishPos, Color.White);
                break;
        }
    }

    /// <summary>
    /// Changes the current state of the level to the next selected one. Code
    /// related to the change between different states should be placed here.
    /// </summary>
    private void ChangeState()
    {
        switch (State)
        {
            case LevelState.Start:
                break;
            case LevelState.Playing:
                if (Player.CurrentWeapon != null &&
                    Player.CurrentWeapon.IsFiring)
                    Player.CurrentWeapon.Stop();
                break;
            case LevelState.Dead:
                break;
            case LevelState.Finished:
                break;
        }

        State = NextState;

        switch (NextState)
        {
            case LevelState.Start:
                break;
            case LevelState.Playing:
                // Store the current ammo count of the player's retrieved weapons
                Player.StoreWeaponAmmoCount();
                break;
            case LevelState.Dead:
                break;
            case LevelState.Finished:
                // Store the current ammo count of the player's retrieved weapons
                Player.StoreWeaponAmmoCount();
                Player.CurrentAnimationType = (int)ActorAnimation.Win;

                Player.CanTakeDamage = false;
                break;
        }
    }

    private bool PlayerOffScreen()
    {
        Rectangle playerRect = Player.CurrentAnimation.SourceRectangle;
        playerRect.X = (int)(Player.Position.X - Player.Origin.X);
        playerRect.Y = (int)(Player.Position.Y - Player.Origin.Y);

        return !ScreenSampler.ScreenRectangle.Intersects(playerRect);
    }

    /// <summary>
    /// Checks whether the player is outside of the Level area.
    /// </summary>
    /// <returns>True if the player is outside of the Level; false otherwise.</returns>
    private bool PlayerOutsideLevel()
    {
        bool result = false;

        Rectangle levelRect = new Rectangle();
        levelRect.Width = ScreenSampler.MaxWidth;
        levelRect.Height = ScreenSampler.MaxHeight;

        Rectangle playerRect = Player.CurrentAnimation.SourceRectangle;
        playerRect.X = (int)(Player.Position.X - Player.Origin.X);
        playerRect.Y = (int)(Player.Position.Y - Player.Origin.Y);

        if (!levelRect.Intersects(playerRect))
        {
            // The player can only fall outside the level in Y
            if (playerRect.Bottom <= levelRect.Top &&
                Player.Acceleration.Y > 0)
                result = false;
            else if (playerRect.Top >= levelRect.Bottom &&
                     Player.Acceleration.Y < 0)
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
        Player.Update(gameTime);

        // Make sure the player's right and left edges are within the screen

        // Right edge
        if (Player.Position.X < Player.Origin.X)
        {
            Player.Position *= Vector2.UnitY;
            Player.Position += new Vector2(Player.Origin.X, 0);
        }

        // Left edge
        if (Player.Position.X - Player.Origin.X + Player.Width > ScreenSampler.MaxWidth)
        {
            Player.Position *= Vector2.UnitY;
            Player.Position += new Vector2(ScreenSampler.MaxWidth - Player.Width + Player.Origin.X, 0);
        }

        // EnemyManager
        EnemyManager.Update(gameTime, Player, ScreenSampler.ScreenRectangle);

        // ParticleEngine
        ParticleEngine.Update(gameTime);

        // Modifiers
        for (int i = _modifiers.Count - 1; i >= 0; i--)
        {
            // Update modifier
            _modifiers[i].Update(gameTime);

            // Remove modifier if it's gone inactive
            if (!_modifiers[i].IsActive)
                _modifiers.RemoveAt(i);
        }

        // Active ActiveObjects that are close to the screen.
        ActivateObjects();

        // Update all active ActiveObjects
        for (int i = 0; i < _activeObjects.Count; i++)
        {
            _activeObjects[i].Update(gameTime);
        }

        UpdateScreenSampler();
    }

    private void UpdateScreenSampler()
    {
        Vector2 positionDelta = ScreenSampler.Position;

        // Only update the screen position if the player is still alive
        if (Player.Health > 0)
        {
            // Update the position of the screen sampler
            ScreenSampler.Position = Player.Position - new Vector2(ScreenSampler.Width / 2, ScreenSampler.Height / 2);

            positionDelta -= ScreenSampler.Position;

            // Background
            foreach (BackgroundLayer background in _backgrounds)
                background.Update(positionDelta);
        }
    }

    /// <summary>
    /// Activates the ActiveObjects that are within a certain distance from the
    /// screen area. The distance is specified by SCREEN_ACTIVATION_DISTANCE.
    /// </summary>
    private void ActivateObjects()
    {
        Rectangle screenRect = new Rectangle((int)ScreenSampler.Position.X,
            (int)ScreenSampler.Position.Y,
            ScreenSampler.Width,
            ScreenSampler.Height);

        screenRect.Inflate(ScreenActivationDistance, ScreenActivationDistance);

        for (int i = _inactiveObjects.Count - 1; i >= 0; i--)
        {
            ActiveObject obj = _inactiveObjects[i];

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
                _activeObjects.Add(obj);
                _inactiveObjects.RemoveAt(i);
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
        EnemyManager.CheckCollisions(Player);

        // Actors -> ParticleEngine
        ParticleEngine.CheckCollisions(Player);
        ParticleEngine.CheckCollisions(EnemyManager.ActivatedEnemies);

        // Actors -> ActiveObjects
        // Particles -> ActiveObjects
        for (int i = 0; i < _activeObjects.Count; i++)
        {
            _activeObjects[i].CheckCollision(Player);
            _activeObjects[i].CheckCollisions(EnemyManager.ActivatedEnemies);
            _activeObjects[i].CheckCollisions(ParticleEngine.Particles);
        }

        // ActiveObjects -> Particles
        ParticleEngine.CheckCollisions(_activeObjects.ToArray());

        // ICollisionObjects  -> TileEngine
        for (int i = 0; i < _tileEngines.Count; i++)
        {
            _tileEngines[i].CheckCollision(Player);
            _tileEngines[i].CheckCollisions(ParticleEngine.Particles);
            _tileEngines[i].CheckCollisions(EnemyManager.ActivatedEnemies);
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
        if (State == LevelState.Playing)
            Player.UpdateAnimation();

        EnemyManager.UpdateAnimations();
    }

    private void DrawLevel(SpriteBatch spriteBatch)
    {
        // Background
        int backgroundIndex;
        for (backgroundIndex = 0;
             backgroundIndex < _backgrounds.Count && _backgrounds[backgroundIndex].Depth <= 1;
             backgroundIndex++)
            _backgrounds[backgroundIndex].Draw(spriteBatch, ScreenSampler);

        // TileEngine
        for (int i = _tileEngines.Count - 1; i >= 0; i--)
            _tileEngines[i].Draw(spriteBatch, ScreenSampler.Position);

        // ActiveObjects
        for (int i = 0; i < _activeObjects.Count; i++)
            _activeObjects[i].Draw(spriteBatch, ScreenSampler.Position);

        // Enemies
        EnemyManager.Draw(spriteBatch, ScreenSampler.Position);

        // Player
        Player.Draw(spriteBatch, ScreenSampler.Position);

        // ParticleEngine
        ParticleEngine.Draw(spriteBatch, ScreenSampler.Position);

        // Foreground
        for (; backgroundIndex < _backgrounds.Count; backgroundIndex++)
            _backgrounds[backgroundIndex].Draw(spriteBatch, ScreenSampler);

        // UI
        DrawUi(spriteBatch);
    }

    private void DrawUi(SpriteBatch spriteBatch)
    {
        // HEALTH BAR

        float playerHealthPercentage = Player.Health / Settings.PlayerStartHealth;
        Rectangle fullHealthSource = Settings.FullHealthUi.SourceRectangle;
        // 48 is the start of the health indicator in x
        // 115 is the width of the health indicator
        fullHealthSource.Width = 48 + (int)(115 * playerHealthPercentage);

        // Draw empty health bar first
        spriteBatch.Draw(Settings.EmptyHealthUi.Texture,
            Vector2.Zero,
            Settings.EmptyHealthUi.SourceRectangle,
            Color.White);

        // Draw the visible part of the full health bar
        spriteBatch.Draw(Settings.FullHealthUi.Texture,
            Vector2.Zero,
            fullHealthSource,
            Color.White);


        // WORLD AND LEVEL NUMBERS
        string indexString = WorldIndex + " - " + LevelIndex;
        Vector2 indexSize = Settings.LevelIndexFont.MeasureString(indexString);
        Vector2 indexPos = new Vector2();
        indexPos.X = (ScreenSampler.Width - indexSize.X) / 2;
        indexPos.Y = UiIndexPosY;

        spriteBatch.DrawString(Settings.LevelIndexFont, indexString, indexPos, Color.White);

        // CURRENT WEAPON AND AMMONUTION COUNT
        Weapon playerWeapon = Player.CurrentWeapon;

        if (playerWeapon != null)
        {
            // Ammo is drawn below the weapon sprite
            Vector2 ammoPos = new Vector2(ScreenSampler.Width - 20, 0);

            string ammoString;
            if (playerWeapon.InfiniteAmmo)
                ammoString = "Inf";
            else
                ammoString = playerWeapon.AmmoCount + " / " + playerWeapon.MaxAmmo;

            Vector2 ammoStringSize = Settings.LevelIndexFont.MeasureString(ammoString);
            ammoPos.X -= ammoStringSize.X;

            // Draw ammo
            spriteBatch.DrawString(Settings.LevelIndexFont, ammoString, ammoPos, Color.White);

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
        Vector2 modifierPos = new Vector2(5, Settings.FullHealthUi.SourceRectangle.Height + 5);
        foreach (ModifierPickup modifier in _modifiers)
        {
            modifier.DrawTimer(spriteBatch, modifierPos, Settings.LevelIndexFont);
            modifierPos.Y += modifier.Icon.SourceRectangle.Height + UiModifierSpacing;
        }
    }

    public void SaveSession(GameSession session)
    {
        foreach (ModifierPickup modifier in _modifiers)
            session.LevelModifiers.Add(new ModifierSave(modifier.Id, modifier.TimeRemaining));

        foreach (ActiveObject activeObject in _activeObjects)
            session.ActivatedObjects.Add(activeObject.UniqueId,
                new ActiveObjectSave(activeObject.Position, activeObject.IsActive));

        EnemyManager.SaveSession(session);
    }

    public void LoadSession(GameSession session)
    {
        // Make a soft reset to prepare for loading the new session
        SoftReset();

        // Get all active modifiers
        foreach (ModifierSave modifier in session.LevelModifiers)
        {
            ModifierPickup levelMod = _modifierLibrary.GetPickup(modifier.Id) as ModifierPickup;
            levelMod.Level = this;
            levelMod.TimeRemaining = modifier.TimeLeft;
            levelMod.IsActive = true;
            _modifiers.Add(levelMod);
        }

        // Inactivate all active objects
        for (int i = _activeObjects.Count - 1; i >= 0; i--)
        {
            _activeObjects[i].Reset();
            _inactiveObjects.Add(_activeObjects[i]);
            _activeObjects.RemoveAt(i);
        }

        // Activate ActiveObjects
        for (int i = _inactiveObjects.Count - 1; i >= 0; i--)
        {
            if (session.ActivatedObjects.ContainsKey(_inactiveObjects[i].UniqueId))
            {
                // Move the object to the list of actived objects
                ActiveObject activeObj = _inactiveObjects[i];
                _inactiveObjects.RemoveAt(i);
                _activeObjects.Add(activeObj);

                // Setup object
                ActiveObjectSave save = session.ActivatedObjects[activeObj.UniqueId];
                activeObj.Position = save.Position;
                activeObj.IsActive = save.IsActive;
            }
            else
                // Since the load of a new session is only prepared for
                // with a soft reset, there could be active objects that where
                // activated that should inactive. Therefore all ActiveObjects
                // not affected by the new session are reset
                _inactiveObjects[i].Reset();
        }

        EnemyManager.LoadSession(session);

        // Have the screensampler move to the players position
        UpdateScreenSampler();

        // Update animations so that they reflect the state of the player and enemies
        UpdateAnimations();

        State = NextState = LevelState.Playing;
    }
}