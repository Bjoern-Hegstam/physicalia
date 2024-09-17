using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.GameManagement;
using PhysicaliaRemastered.Input;
using PhysicaliaRemastered.Weapons;
using XNALibrary.Graphics.TileEngine;
using XNALibrary.Interfaces;
using Weapon = PhysicaliaRemastered.Weapons.NewWeapons.Weapon;

namespace PhysicaliaRemastered.Actors;

public class Player : Actor
{
    public const float DefaultJumpMagnitude = -300F;
    public const float DefaultWalkSpeed = 100F;
    public const float DefaultFallMovementSpeed = 80F;

    public const float DefaultInvincibleTime = 1F;
    public const float DefaultFlickerInterval = 0.1F;

    // Life management
    private float _invincibleTime;
    private float _flickerInterval;
    private bool _visible;

    public float JumpMagnitude { get; set; }

    public float WalkSpeed { get; set; }

    public float FallMovementSpeed { get; set; }

    public bool Flickering
    {
        get => _invincibleTime > 0;
        set
        {
            if (value)
            {
                _invincibleTime = DefaultInvincibleTime;
            }
            else
            {
                _invincibleTime = 0;
                _visible = true;
            }
        }
    }

    public override float Health
    {
        get => base.Health;
        set => base.Health = Math.Max(Math.Min(Settings.PlayerStartHealth, value), 0);
    }

    public ISettings Settings { get; }

    private readonly Dictionary<int, Weapon> _weapons;
    private int _currentWeapon;

    public Weapon CurrentWeapon
    {
        get
        {
            if (_weapons.ContainsKey(_currentWeapon))
                return _weapons[_currentWeapon];

            return null;
        }
    }

    public Player(ISettings settings)
    {
        Settings = settings;

        JumpMagnitude = DefaultJumpMagnitude;
        WalkSpeed = DefaultWalkSpeed;
        FallMovementSpeed = DefaultFallMovementSpeed;

        Health = settings.PlayerStartHealth;
        _visible = true;

        _weapons = new Dictionary<int, Weapon>();
    }

    /// <summary>
    /// Allows the Player to handle input.
    /// </summary>
    public void HandleInput()
    {
        Vector2 velocity = Velocity;
        velocity.X = 0;

        float moveSpeed = WalkSpeed;
        if (CurrentAnimationType == (int)ActorAnimation.Fall ||
            CurrentAnimationType == (int)ActorAnimation.Jump)
            moveSpeed = FallMovementSpeed;

        // Walk
        if (Settings.InputMap.IsHolding(InputAction.WalkLeft))
            velocity.X = -moveSpeed;

        if (Settings.InputMap.IsHolding(InputAction.WalkRight))
            velocity.X = moveSpeed;

        // Jump
        if (Settings.InputMap.IsPressed(InputAction.Jump) &&
            CurrentAnimationType != (int)ActorAnimation.Jump &&
            CurrentAnimationType != (int)ActorAnimation.Fall)
            velocity.Y = JumpMagnitude * (Acceleration.Y > 0 ? 1 : -1);

        Velocity = velocity;

        // Weapon controls
        // Only check if the player has a weapon
        if (_weapons.Count == 0)
            return;

        // Should the weapons be switched?
        if (Settings.InputMap.IsPressed(InputAction.NextWeapon))
        {
            int prevWeapon = _currentWeapon;

            // Get the next weapon key
            foreach (int key in _weapons.Keys)
            {
                if (key > _currentWeapon)
                {
                    _currentWeapon = key;
                    break;
                }
            }

            // Was the previous weapon the last one?
            if (_currentWeapon == prevWeapon &&
                _weapons.Count > 1)
            {
                // Get the weapon before the current one
                foreach (int key in _weapons.Keys)
                {
                    if (key < _currentWeapon)
                        _currentWeapon = key;
                    else
                        break;
                }
            }

            if (_weapons[prevWeapon].IsFiring && _weapons.Count > 1)
            {
                // Stop the previous weapon and start the next
                _weapons[prevWeapon].Stop();
                _weapons[_currentWeapon].Start();
            }
        }

        if (Settings.InputMap.IsPressed(InputAction.PreviousWeapon))
        {
            // Get the weapon before the current one
            int nextWeapon = _currentWeapon;
            foreach (int key in _weapons.Keys)
            {
                if (key < _currentWeapon)
                {
                    nextWeapon = key;
                }
                else
                    break;
            }

            // Was the previous weapon the first one?
            if (_currentWeapon == nextWeapon &&
                _weapons.Count > 1)
            {
                // Get the next weapon key
                foreach (int key in _weapons.Keys)
                {
                    if (key > _currentWeapon)
                    {
                        nextWeapon = key;
                    }
                }
            }

            if (_weapons[_currentWeapon].IsFiring && _weapons.Count > 1)
            {
                // Stop the previous weapon and start the next
                _weapons[_currentWeapon].Stop();
                _weapons[nextWeapon].Start();
            }

            _currentWeapon = nextWeapon;
        }

        // Should the weapon be firing or stopped firing
        if (Settings.InputMap.IsPressed(InputAction.Shoot) &&
            !_weapons[_currentWeapon].IsFiring)
            _weapons[_currentWeapon].Start();

        if (Settings.InputMap.IsReleased(InputAction.Shoot) &&
            _weapons[_currentWeapon].IsFiring)
            _weapons[_currentWeapon].Stop();
    }

    /// <summary>
    /// Clears the player's weapons.
    /// </summary>
    public void ClearWeapons()
    {
        _weapons.Clear();
    }

    /// <summary>
    /// Adds a weapon to the player's arsenal. If the player already has a weapon
    /// with the same ID, then only the ammo of the new weapon is kept.
    /// </summary>
    /// <param name="weapon">Weapon to add.</param>
    public void AddWeapon(Weapon weapon)
    {
        // First weapon
        if (_weapons.Count == 0)
            _currentWeapon = weapon.WeaponId;

        // Keep only the ammo if the player already has a weapon of the same type
        if (_weapons.ContainsKey(weapon.WeaponId))
            _weapons[weapon.WeaponId].AmmoCount += weapon.AmmoCount;
        else
        {
            _weapons.Add(weapon.WeaponId, weapon);
            weapon.Player = this;

            // Switch to weapon if it's better than the current one
            if (weapon.WeaponId > _currentWeapon)
                _currentWeapon = weapon.WeaponId;
        }
    }

    /// <summary>
    /// Removes the weapon with the specified id from the player's
    /// collection of weapons.
    /// </summary>
    /// <param name="weaponId">Id of the weapon to remove.</param>
    public void RemoveWeapon(int weaponId, int ammoCount)
    {
        // If the weapon has a stored ammo count of 0 it was added
        // during the last level and should be removed. Also
        // weapons with infinite ammo should only be picked up once
        // and therefor removed when the player dies on the same level
        if (_weapons.ContainsKey(weaponId) &&
            (_weapons[weaponId].AmmoMemory == 0 ||
             _weapons[weaponId].InfiniteAmmo))
        {
            _weapons.Remove(weaponId);

            // If the removed weapon was the currently selected one a new key
            // must be found
            if (_currentWeapon == weaponId)
                foreach (int weapon in _weapons.Keys)
                {
                    _currentWeapon = weapon;
                    break;
                }
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Update invincibility flicker
        if (_invincibleTime > 0)
        {
            _invincibleTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            _flickerInterval -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_invincibleTime < 0)
                _visible = true;
            else if (_flickerInterval <= 0)
            {
                if (_invincibleTime > 0)
                {
                    _visible = !_visible;
                    _flickerInterval = DefaultFlickerInterval;
                }
                else
                    _visible = true;
            }
        }

        if (_weapons.ContainsKey(_currentWeapon))
            _weapons[_currentWeapon].Update(gameTime);
    }

    public void StoreWeaponAmmoCount()
    {
        foreach (Weapon weapon in _weapons.Values)
            weapon.StoreAmmoCount();
    }

    public void ApplyStoredWeaponAmmoCount()
    {
        foreach (Weapon weapon in _weapons.Values)
            weapon.ApplyStoredAmmoCount();
    }

    public void Kill()
    {
        if (!_visible)
            _visible = true;

        // Stop the player's motion in X
        Velocity *= Vector2.UnitY;

        // Stop firing if needed
        if (_weapons.ContainsKey(_currentWeapon) &&
            _weapons[_currentWeapon].IsFiring)
            _weapons[_currentWeapon].Stop();
    }

    public override ObjectType Type => ObjectType.Player;

    public override void TakeDamage(float damageLevel)
    {
        // Invincible?
        if (_invincibleTime > 0)
            return;

        // Decreaes health
        Health -= damageLevel;

        // Decrease life count if the player died
        if (Health <= 0)
        {
            Kill();
        }
        else
            _invincibleTime = DefaultInvincibleTime;
    }

    public override void OnCollision(ICollisionObject collidedObject, BoxSide collisionSides, Vector2 position,
        Vector2 velocity)
    {
        if (collidedObject.Type == ObjectType.Tile)
        {
            Position = position;
            Velocity = velocity;

            // TODO: Fix so that player can't shoot through walls
        }
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
        if (!_visible)
            return;

        // Draw player
        base.Draw(spriteBatch, offsetPosition);

        // Draw weapon
        if (_weapons.Count != 0 &&
            _weapons.ContainsKey(_currentWeapon) &&
            CurrentAnimationType != (int)ActorAnimation.Die &&
            CurrentAnimationType != (int)ActorAnimation.Win)
            _weapons[_currentWeapon].Draw(spriteBatch, offsetPosition, SpriteFlip);
    }

    public void NewSession()
    {
        _weapons.Clear();

        _invincibleTime = 0;
        _flickerInterval = DefaultFlickerInterval;
        _visible = true;
    }

    public void LoadSession(GameSession session, WeaponBank weaponBank)
    {
        CurrentAnimationType = 0;

        _invincibleTime = 0;
        _flickerInterval = DefaultFlickerInterval;
        _visible = true;

        // Setup player
        Position = session.PlayerValues.Position;
        Velocity = session.PlayerValues.Velocity;
        Acceleration = session.PlayerValues.Acceleration;

        Health = session.PlayerHealth;

        // Clear previous weapons
        if (_weapons.Count > 0)
            _weapons.Clear();

        // Set the current weapon
        _currentWeapon = session.SelectedWeapon;

        // Load in weapons and ammo
        foreach (int weaponId in session.WeaponSaves.Keys)
        {
            WeaponSave weaponSave = session.WeaponSaves[weaponId];

            // Get the saved weapon
            Weapon weapon = weaponBank.GetWeapon(weaponId).Copy();

            // Set the ammo
            weapon.AmmoCount = weaponSave.AmmoCount;
            weapon.AmmoMemory = weapon.AmmoCount == -1 ? -1 : weaponSave.StoredAmmo;

            weapon.Player = this;

            _weapons.Add(weapon.WeaponId, weapon);
        }
    }

    public void SaveSession(GameSession session)
    {
        ActorStartValues playerValues = new ActorStartValues();
        playerValues.Position = Position;
        playerValues.Velocity = Velocity;
        playerValues.Acceleration = Acceleration;

        session.PlayerValues = playerValues;
        session.PlayerHealth = Health;

        // Weapons
        session.SelectedWeapon = _currentWeapon;

        foreach (int weaponId in _weapons.Keys)
            session.WeaponSaves.Add(weaponId,
                new WeaponSave(_weapons[weaponId].AmmoCount, _weapons[weaponId].AmmoMemory));
    }
}