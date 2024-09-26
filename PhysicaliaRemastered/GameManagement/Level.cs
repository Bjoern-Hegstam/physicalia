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
using XNALibrary;
using XNALibrary.Collision;
using XNALibrary.ParticleEngine;
using XNALibrary.Sprites;
using XNALibrary.TileEngine;
using Viewport = XNALibrary.Graphics.Viewport;

namespace PhysicaliaRemastered.GameManagement;

public enum LevelState
{
    Start,
    Playing,
    Dead,
    Finished
}

public class Level(Game game, Player player)
{
    public int WorldIndex { get; set; }
    public int LevelIndex { get; set; }

    public LevelState State { get; private set; } = LevelState.Start;
    public LevelState NextState { get; set; } = LevelState.Start;

    public Player Player { get; } = player;

    public Viewport Viewport { get; } = new(
        0,
        0,
        game.GraphicsDevice.Viewport.Width,
        game.GraphicsDevice.Viewport.Height
    );

    private const float UiIndexPosY = 10F;
    private const float UiModifierSpacing = 5F;

    private const int ScreenActivationDistance = 20;

    private const float PlayerFinishSlowdown = 0.95F;

    private readonly List<BackgroundLayer> _backgrounds = [];
    private readonly List<ModifierPickup> _modifiers = [];
    private readonly List<TileEngine> _tileEngines = [];
    private readonly List<ActiveObject> _activeObjects = [];
    private readonly List<ActiveObject> _inactiveObjects = [];

    private ActorStartValues _playerStartValues;

    private readonly EnemyManager _enemyManager = new(game.Services.GetService<EnemyLibrary>());

    private Fonts Fonts => game.Services.GetService<Fonts>();
    private InputSettings InputSettings => game.Services.GetService<InputSettings>();
    private SpriteLibrary SpriteLibrary => game.Services.GetService<SpriteLibrary>();
    private ParticleEngine ParticleEngine => game.Services.GetService<ParticleEngine>();
    private WeaponLibrary WeaponLibrary => game.Services.GetService<WeaponLibrary>();
    private PickupTemplateLibrary ModifierTemplateLibrary => game.Services.GetService<PickupTemplateLibrary>();

    private Sprite FullHealthUi => SpriteLibrary.GetSprite(new SpriteId("health-bar--full"));
    private Sprite EmptyHealthUi => SpriteLibrary.GetSprite(new SpriteId("health-bar--empty"));

    public void Update(GameTime gameTime)
    {
        if (State == LevelState.Start)
        {
            if (InputSettings.InputMap.IsPressed(InputAction.MenuStart))
            {
                NextState = LevelState.Playing;
            }
        }
        else if (State == LevelState.Playing)
        {
            UpdateLevel(gameTime);
            CheckCollisions();
            Player.HandleInput();
            UpdateActorState();

            if (IsPlayerOutsideLevel())
            {
                Player.Kill();

                // Do a little jump into screen
                Player.Velocity = new Vector2(0, Player.JumpMagnitude * Math.Sign(Player.Acceleration.Y));
                NextState = LevelState.Dead;
            }
            else if (Player.Health <= 0)
            {
                Player.CanCollide = false;
                Player.Velocity = new Vector2(0, Player.JumpMagnitude * Math.Sign(Player.Acceleration.Y));

                NextState = LevelState.Dead;
            }
        }
        else if (State == LevelState.Dead)
        {
            // Continue updating the Level after death, just don't check input
            UpdateLevel(gameTime);
            CheckCollisions();
            UpdateActorState();

            // Don't let the player continue falling if it's outside of the level (i.e. falling)
            if (Player.Velocity != Vector2.Zero && PlayerOffScreen() &&
                Player.Velocity.Y / Player.Acceleration.Y > 0)
            {
                Player.Velocity = Vector2.Zero;
                Player.Acceleration = Vector2.Zero;
            }
        }
        else if (State == LevelState.Finished)
        {
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
                if (Player.Velocity.X is > -1 and < 1)
                {
                    Player.Velocity *= Vector2.UnitY;
                }
            }

            UpdateLevel(gameTime);
            CheckCollisions();
            UpdateActorState();
        }

        // Should the state be changed?
        if (NextState != State)
        {
            ChangeState();
        }
    }

    public void LoadXml(XmlReader reader, TileLibrary tileLibrary)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Level" })
            {
                int width = int.Parse(reader.GetAttribute("width") ?? throw new ResourceLoadException());
                int height = int.Parse(reader.GetAttribute("height") ?? throw new ResourceLoadException());

                Viewport.MaxWidth = width;
                Viewport.MaxHeight = height;
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "PlayerStart" })
            {
                _playerStartValues = ActorStartValues.FromXml(reader, "PlayerStart");
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Enemies" })
            {
                LoadEnemies(reader);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "ActiveObjects" })
            {
                LoadActiveObjects(reader);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Background" })
            {
                var spriteId =
                    new SpriteId(reader.GetAttribute("spriteKey") ?? throw new ResourceLoadException());
                float depth = float.Parse(reader.GetAttribute("depth") ?? throw new ResourceLoadException());
                string loopString = reader.GetAttribute("loop") ?? throw new ResourceLoadException();
                bool loopX = loopString.Contains('x');
                bool loopY = loopString.Contains('y');

                Sprite sprite = SpriteLibrary.GetSprite(spriteId);
                var background = new BackgroundLayer(sprite, depth)
                {
                    LoopX = loopX,
                    LoopY = loopY
                };

                if (!reader.IsEmptyElement)
                {
                    reader.ReadToFollowing("Position");
                    float x = float.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
                    float y = float.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());
                    background.StartPosition = new Vector2(x, y);
                }

                _backgrounds.Add(background);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Backgrounds" })
            {
                _backgrounds.Sort(BackgroundLayer.Compare);
            }

            // TileEngines?
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "TileEngines" })
            {
                int engineCount = int.Parse(reader.GetAttribute("count") ?? throw new ResourceLoadException());

                for (var i = 0; i < engineCount; i++)
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
    /// Loads in enemies as specified by the xml read by the XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    private void LoadEnemies(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Enemy" })
            {
                int type = int.Parse(reader.GetAttribute("type") ?? throw new ResourceLoadException());
                ActorStartValues startValues = ActorStartValues.FromXml(reader, "StartValues");

                reader.ReadToFollowing("PatrolArea");
                int x = int.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
                int y = int.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());
                int width = int.Parse(reader.GetAttribute("width") ?? throw new ResourceLoadException());
                int height = int.Parse(reader.GetAttribute("height") ?? throw new ResourceLoadException());
                var patrolArea = new Rectangle(x, y, width, height);

                _enemyManager.EnqueueEnemy(type, startValues, patrolArea);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Enemies" })
            {
                return;
            }
        }
    }

    /// <summary>
    /// Loads in ActiveObjects as specified by the xml read by the XmlReader.
    /// </summary>
    /// <param name="reader"></param>
    private void LoadActiveObjects(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Weapons" })
            {
                if (reader.IsEmptyElement)
                {
                    continue;
                }

                LoadWeapons(reader);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Pickups" })
            {
                if (reader.IsEmptyElement)
                {
                    continue;
                }

                LoadPickups(reader);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "LevelFinishTrigger" })
            {
                int x = int.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
                int y = int.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());

                Sprite triggerSprite = SpriteLibrary.GetSprite(new SpriteId("end-level-trigger"));
                var trigger = new EndLevelTrigger(this, triggerSprite);

                var cont = new PickupContainer(trigger)
                {
                    Position = new Vector2(x, y),
                    CollisionBox = new Rectangle(0, 0, triggerSprite.SourceRectangle.Width,
                        triggerSprite.SourceRectangle.Height),
                    CanCollide = true,
                    IsActive = false
                };

                EnqueueActiveObject(cont);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "ActiveObjects" })
            {
                return;
            }
        }
    }

    private void LoadWeapons(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Weapon" })
            {
                int key = int.Parse(reader.GetAttribute("key") ?? throw new ResourceLoadException());
                Weapon weapon = WeaponLibrary.GetWeapon(key);

                var weaponPickup = new WeaponPickup(this, weapon);
                var pickupCont = new PickupContainer(weaponPickup);

                reader.ReadToFollowing("Position");
                float x = float.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
                float y = float.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());
                pickupCont.Position = new Vector2(x, y);

                reader.ReadToFollowing("CollisionBox");
                int xBox = int.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
                int yBox = int.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());
                int width = int.Parse(reader.GetAttribute("width") ?? throw new ResourceLoadException());
                int height = int.Parse(reader.GetAttribute("height") ?? throw new ResourceLoadException());
                pickupCont.CollisionBox = new Rectangle(xBox, yBox, width, height);

                EnqueueActiveObject(pickupCont);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Weapons" })
            {
                return;
            }
        }
    }

    private void LoadPickups(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Pickup" })
            {
                int id = int.Parse(reader.GetAttribute("key") ?? throw new ResourceLoadException());
                PickupTemplateId templateId = new PickupTemplateId(id);
                int x = int.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
                int y = int.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());

                Pickup pickup = ModifierTemplateLibrary.CreatePickup(templateId);
                pickup.Level = this;

                var cont = new PickupContainer(pickup)
                {
                    Position = new Vector2(x, y),
                    CollisionBox = new Rectangle(0, 0, pickup.Sprite.SourceRectangle.Width,
                        pickup.Sprite.SourceRectangle.Height)
                };
                EnqueueActiveObject(cont);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Pickups" })
            {
                return;
            }
        }
    }

    /// <summary>
    /// Resets the Level to its initial state.
    /// </summary>
    public void Reset()
    {
        foreach (BackgroundLayer background in _backgrounds)
        {
            background.Position = background.StartPosition;
        }

        _enemyManager.Reset();

        Player.StartValues = _playerStartValues;
        Player.ApplyStartValues();
        Player.CanCollide = true;
        Player.CanTakeDamage = true;
        Player.Health = Player.DefaultMaxHealth;
        Player.ApplyStoredWeaponAmmoCount();

        Viewport.Position = Vector2.Zero;

        while (_activeObjects.Count > 0)
        {
            _activeObjects[0].Reset();
            EnqueueActiveObject(_activeObjects[0]);
            _activeObjects.RemoveAt(0);
        }

        _modifiers.Clear();

        NextState = State = LevelState.Start;

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
        {
            background.Position = background.StartPosition;
        }

        // Reset enemies
        _enemyManager.Reset();

        Player.CanCollide = true;
        Player.CanTakeDamage = true;

        // Set the start position of the screen sampler
        Viewport.Position = Vector2.Zero;

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
        _enemyManager.ActivateVisibleEnemies(Viewport);
    }

    public void AddModifier(ModifierPickup modifier)
    {
        _modifiers.Add(modifier);
    }

    public void EnqueueActiveObject(ActiveObject obj)
    {
        _inactiveObjects.Add(obj);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        switch (State)
        {
            case LevelState.Start:
                string indexString = WorldIndex + " - " + LevelIndex;
                Vector2 indexStringSize = Fonts.LevelIndex.MeasureString(indexString);
                var indexPosition = new Vector2
                {
                    X = (Viewport.Width - indexStringSize.X) / 2,
                    Y = (Viewport.Height - indexStringSize.Y) / 2
                };

                spriteBatch.DrawString(Fonts.LevelIndex, indexString, indexPosition, Color.White);
                break;
            case LevelState.Playing:
                DrawLevel(spriteBatch);
                break;
            case LevelState.Dead:
                DrawLevel(spriteBatch);

                var deadString = "You Have Died!";
                Vector2 deadStringSize = Fonts.PlayerDead.MeasureString(deadString);
                Vector2 deadPos;
                deadPos.X = (Viewport.Width - deadStringSize.X) / 2;
                deadPos.Y = (Viewport.Width - deadStringSize.X) / 2;

                spriteBatch.DrawString(Fonts.PlayerDead, deadString, deadPos, Color.White);
                break;
            case LevelState.Finished:
                DrawLevel(spriteBatch);

                const string finishString = "Level Finished!";
                Vector2 finishStringSize = Fonts.PlayerDead.MeasureString(finishString);
                var finishPos = new Vector2
                {
                    X = (game.GraphicsDevice.Viewport.Width - finishStringSize.X) / 2,
                    Y = (game.GraphicsDevice.Viewport.Height - finishStringSize.Y) / 2
                };

                spriteBatch.DrawString(Fonts.PlayerDead, finishString, finishPos, Color.White);
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
                if (Player.CurrentWeapon is { IsFiring: true })
                {
                    Player.CurrentWeapon.Stop();
                }

                break;
            case LevelState.Dead:
                break;
            case LevelState.Finished:
                break;
            default:
                throw new ArgumentOutOfRangeException();
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
                Player.CurrentState = ActorState.Celebrating;

                Player.CanTakeDamage = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool PlayerOffScreen()
    {
        return !Viewport.IsOnScreen(Player.AbsoluteCollisionBox);
    }

    private bool IsPlayerOutsideLevel()
    {
        var levelRect = new Rectangle
        {
            Width = Viewport.MaxWidth,
            Height = Viewport.MaxHeight
        };

        Rectangle playerRect = Player.AbsoluteCollisionBox;

        if (levelRect.Intersects(playerRect))
        {
            return false;
        }

        // The player can only fall outside the level in Y
        if (playerRect.Bottom <= levelRect.Top &&
            Player.Acceleration.Y > 0)
        {
            return false;
        }

        if (playerRect.Top >= levelRect.Bottom &&
            Player.Acceleration.Y < 0)
        {
            return false;
        }

        return true;
    }

    private void UpdateLevel(GameTime gameTime)
    {
        Player.Update(gameTime);

        EnsurePlayerIsWithinLevelBounds();

        _enemyManager.Update(gameTime, Player, Viewport);
        ParticleEngine.Update(gameTime);

        for (int i = _modifiers.Count - 1; i >= 0; i--)
        {
            _modifiers[i].Update(gameTime);

            if (!_modifiers[i].IsActive)
            {
                _modifiers.RemoveAt(i);
            }
        }

        ActivateObjects();

        foreach (ActiveObject activeObject in _activeObjects)
        {
            activeObject.Update(gameTime);
        }

        UpdateScreenSampler();
    }

    private void EnsurePlayerIsWithinLevelBounds()
    {
        if (Player.AbsoluteCollisionBox.Left < 0)
        {
            Player.Position *= Vector2.UnitY;
        }

        if (Player.AbsoluteCollisionBox.Right > Viewport.MaxWidth)
        {
            Player.Position *= Vector2.UnitY;
            Player.Position += new Vector2(Viewport.MaxWidth - Player.CollisionBox.Width, 0);
        }
    }

    private void UpdateScreenSampler()
    {
        if (Player.Health <= 0)
        {
            return;
        }

        Vector2 prevScreenSamplerPosition = Viewport.Position;
        Viewport.Position = Player.Position - new Vector2(Viewport.Width / 2f, Viewport.Height / 2f);

        Vector2 positionDelta = prevScreenSamplerPosition - Viewport.Position;

        foreach (BackgroundLayer background in _backgrounds)
        {
            background.Update(positionDelta);
        }
    }

    /// <summary>
    /// Activates the ActiveObjects that are within a certain distance from the screen area.
    /// </summary>
    private void ActivateObjects()
    {
        var screenRect = new Rectangle((int)Viewport.Position.X,
            (int)Viewport.Position.Y,
            Viewport.Width,
            Viewport.Height);

        screenRect.Inflate(ScreenActivationDistance, ScreenActivationDistance);

        for (int i = _inactiveObjects.Count - 1; i >= 0; i--)
        {
            ActiveObject obj = _inactiveObjects[i];

            Rectangle collBox = obj.CollisionBox;

            // Set the position to be in level coordinates
            collBox.X += (int)obj.Position.X;
            collBox.Y += (int)obj.Position.Y;

            if (!collBox.Intersects(screenRect))
            {
                continue;
            }

            obj.IsActive = true;

            _activeObjects.Add(obj);
            _inactiveObjects.RemoveAt(i);
        }
    }

    /// <summary>
    /// Checks for collisions between the currently active objects in the level.
    /// </summary>
    private void CheckCollisions()
    {
        _enemyManager.CheckCollisions(Player);

        ParticleEngine.CheckCollisions(Player);
        ParticleEngine.CheckCollisions(_enemyManager.ActivatedEnemies);

        foreach (ActiveObject activeObject in _activeObjects)
        {
            activeObject.CheckCollision(Player);
            activeObject.CheckCollisions(_enemyManager.ActivatedEnemies);
            activeObject.CheckCollisions(ParticleEngine.Particles);
        }

        // ActiveObjects -> Particles
        ParticleEngine.CheckCollisions(_activeObjects.ToArray());

        // ICollisionObjects  -> TileEngine
        foreach (TileEngine tileEngine in _tileEngines)
        {
            tileEngine.CheckCollision(Player);
            tileEngine.CheckCollisions(ParticleEngine.Particles);
            tileEngine.CheckCollisions(_enemyManager.ActivatedEnemies);
        }
    }

    /// <summary>
    /// Updates the state of all actors. Doing this only once per update
    /// reduces the risk of flickering that can occur if an actor often switches
    /// between states.
    /// </summary>
    private void UpdateActorState()
    {
        if (State == LevelState.Playing)
        {
            Player.UpdateActorState();
        }

        _enemyManager.UpdateActorStates();
    }

    private void DrawLevel(SpriteBatch spriteBatch)
    {
        // Background
        int backgroundIndex;
        for (backgroundIndex = 0;
             backgroundIndex < _backgrounds.Count && _backgrounds[backgroundIndex].Depth <= 1;
             backgroundIndex++)
        {
            _backgrounds[backgroundIndex].Draw(spriteBatch);
        }

        // TileEngine
        for (int i = _tileEngines.Count - 1; i >= 0; i--)
        {
            _tileEngines[i].Draw(spriteBatch, Viewport.Position);
        }

        // ActiveObjects
        foreach (ActiveObject activeObject in _activeObjects)
        {
            activeObject.Draw(spriteBatch, Viewport.Position);
        }

        // Enemies
        _enemyManager.Draw(spriteBatch, Viewport.Position);

        // Player
        Player.Draw(spriteBatch, Viewport.Position);

        // ParticleEngine
        ParticleEngine.Draw(spriteBatch, Viewport.Position);

        // Foreground
        for (; backgroundIndex < _backgrounds.Count; backgroundIndex++)
        {
            _backgrounds[backgroundIndex].Draw(spriteBatch);
        }

        // UI
        DrawUi(spriteBatch);
    }

    private void DrawUi(SpriteBatch spriteBatch)
    {
        // HEALTH BAR
        float playerHealthPercentage = Player.Health / Player.DefaultMaxHealth;
        Rectangle fullHealthSource = FullHealthUi.SourceRectangle;
        // 48 is the start of the health indicator in x
        // 115 is the width of the health indicator
        fullHealthSource.Width = 48 + (int)(115 * playerHealthPercentage);

        // Draw empty health bar first
        spriteBatch.Draw(EmptyHealthUi.Texture,
            Vector2.Zero,
            EmptyHealthUi.SourceRectangle,
            Color.White);

        // Draw the visible part of the full health bar
        spriteBatch.Draw(FullHealthUi.Texture,
            Vector2.Zero,
            fullHealthSource,
            Color.White);

        // WORLD AND LEVEL NUMBERS
        string indexString = WorldIndex + " - " + LevelIndex;
        Vector2 indexSize = Fonts.LevelIndex.MeasureString(indexString);
        var indexPos = new Vector2
        {
            X = (Viewport.Width - indexSize.X) / 2,
            Y = UiIndexPosY
        };

        spriteBatch.DrawString(Fonts.LevelIndex, indexString, indexPos, Color.White);

        // CURRENT WEAPON AND AMMUNITION COUNT
        Weapon? playerWeapon = Player.CurrentWeapon;

        if (playerWeapon != null)
        {
            // Ammo is drawn below the weapon sprite
            var ammoPos = new Vector2(Viewport.Width - 20, 0);

            string ammoString;
            if (playerWeapon.HasInfiniteAmmo)
            {
                ammoString = "Inf";
            }
            else
            {
                ammoString = playerWeapon.AmmoCount + " / " + playerWeapon.MaxAmmo;
            }

            Vector2 ammoStringSize = Fonts.LevelIndex.MeasureString(ammoString);
            ammoPos.X -= ammoStringSize.X;

            // Draw ammo
            spriteBatch.DrawString(Fonts.LevelIndex, ammoString, ammoPos, Color.White);

            var weaponPos = new Vector2(470, 5);
            if (playerWeapon.WeaponSprite != null)
            {
                spriteBatch.Draw(
                    playerWeapon.WeaponSprite?.Texture,
                    weaponPos,
                    playerWeapon.WeaponSprite?.SourceRectangle,
                    Color.White
                );
            }
        }

        // MODIFIERS
        // A little extra spacing is added (5 px)
        var modifierPos = new Vector2(5, (float)FullHealthUi.SourceRectangle.Height + 5);
        foreach (ModifierPickup modifier in _modifiers)
        {
            modifier.DrawTimer(spriteBatch, modifierPos, Fonts.LevelIndex);
            modifierPos.Y += modifier.Icon.SourceRectangle.Height + UiModifierSpacing;
        }
    }

    public void SaveGame(SaveGame saveGame)
    {
        foreach (ModifierPickup modifier in _modifiers)
        {
            saveGame.LevelModifiers.Add(new ModifierSave(modifier.TemplateId.Id, modifier.TimeRemaining));
        }

        foreach (ActiveObject activeObject in _activeObjects)
        {
            saveGame.ActivatedObjects.Add(activeObject.UniqueId,
                new ActiveObjectSave(activeObject.Position, activeObject.IsActive));
        }

        _enemyManager.SaveGame(saveGame);
    }

    public void LoadGame(SaveGame saveGame)
    {
        SoftReset();

        foreach (ModifierSave modifier in saveGame.LevelModifiers)
        {
            var levelMod = ModifierTemplateLibrary.CreatePickup(new PickupTemplateId(modifier.Id)) as ModifierPickup;
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
            if (saveGame.ActivatedObjects.TryGetValue(_inactiveObjects[i].UniqueId,
                    out ActiveObjectSave activeObjectSave))
            {
                // Move the object to the list of activated objects
                ActiveObject activeObj = _inactiveObjects[i];
                _inactiveObjects.RemoveAt(i);
                _activeObjects.Add(activeObj);

                // Setup object
                activeObj.Position = activeObjectSave.Position;
                activeObj.IsActive = activeObjectSave.IsActive;
            }
            else
                // Since the load of a new game is only prepared for
                // with a soft reset, there could be active objects that where
                // activated that should inactive. Therefore, all ActiveObjects
                // not affected by the new game are reset.
            {
                _inactiveObjects[i].Reset();
            }
        }

        _enemyManager.LoadGame(saveGame);

        // Have the screen sampler move to the players position
        UpdateScreenSampler();

        // Update animations so that they reflect the state of the player and enemies
        UpdateActorState();

        State = NextState = LevelState.Playing;
    }
}