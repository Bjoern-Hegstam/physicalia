using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.GameManagement;
using PhysicaliaRemastered.Input;
using PhysicaliaRemastered.Weapons;
using XNALibrary.Collision;
using XNALibrary.TileEngine;
using Weapon = PhysicaliaRemastered.Weapons.Weapon;

namespace PhysicaliaRemastered.Actors;

public class Player : Actor
{
    public const float DefaultMaxHealth = 100;
    public const float DefaultJumpMagnitude = -300F;
    public const float DefaultWalkSpeed = 100F;
    public const float DefaultFallMovementSpeed = 80F;

    public const float DefaultInvincibleTime = 1F;
    public const float DefaultFlickerInterval = 0.1F;

    // Life management
    private float _invincibleTime;
    private float _flickerInterval;
    private bool _visible = true;

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

    public InputSettings InputSettings { get; }

    private readonly Dictionary<int, Weapon> _weapons = new();
    private int _currentWeapon;

    public Weapon? CurrentWeapon => _weapons.GetValueOrDefault(_currentWeapon);

    public Player(InputSettings inputSettings)
    {
        InputSettings = inputSettings;

        JumpMagnitude = DefaultJumpMagnitude;
        WalkSpeed = DefaultWalkSpeed;
        FallMovementSpeed = DefaultFallMovementSpeed;

        Health = 100;
    }

    /// <summary>
    /// Allows the Player to handle input.
    /// </summary>
    public void HandleInput()
    {
        Vector2 velocity = Velocity;
        velocity.X = 0;

        float moveSpeed = WalkSpeed;
        if (CurrentState is ActorState.Falling or ActorState.Jumping)
        {
            moveSpeed = FallMovementSpeed;
        }

        // Walk
        if (InputSettings.InputMap.IsHolding(InputAction.WalkLeft))
        {
            velocity.X = -moveSpeed;
        }

        if (InputSettings.InputMap.IsHolding(InputAction.WalkRight))
        {
            velocity.X = moveSpeed;
        }

        // Jump
        if (InputSettings.InputMap.IsPressed(InputAction.Jump) &&
            CurrentState != ActorState.Jumping &&
            CurrentState != ActorState.Falling)
        {
            velocity.Y = JumpMagnitude * (Acceleration.Y > 0 ? 1 : -1);
        }

        Velocity = velocity;

        // Weapon controls
        // Only check if the player has a weapon
        if (_weapons.Count == 0)
        {
            return;
        }

        // Should the weapons be switched?
        if (InputSettings.InputMap.IsPressed(InputAction.NextWeapon))
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
                    {
                        _currentWeapon = key;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (_weapons[prevWeapon].IsFiring && _weapons.Count > 1)
            {
                // Stop the previous weapon and start the next
                _weapons[prevWeapon].Stop();
                _weapons[_currentWeapon].Start();
            }
        }

        if (InputSettings.InputMap.IsPressed(InputAction.PreviousWeapon))
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
                {
                    break;
                }
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
        if (InputSettings.InputMap.IsPressed(InputAction.Shoot) &&
            !_weapons[_currentWeapon].IsFiring)
        {
            _weapons[_currentWeapon].Start();
        }

        if (InputSettings.InputMap.IsReleased(InputAction.Shoot) &&
            _weapons[_currentWeapon].IsFiring)
        {
            _weapons[_currentWeapon].Stop();
        }
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
        {
            _currentWeapon = weapon.WeaponId;
        }

        // Keep only the ammo if the player already has a weapon of the same type
        if (!_weapons.TryAdd(weapon.WeaponId, weapon))
        {
            _weapons[weapon.WeaponId].AmmoCount += weapon.AmmoCount;
        }
        else
        {
            weapon.Player = this;

            // Switch to weapon if it's better than the current one
            if (weapon.WeaponId > _currentWeapon)
            {
                _currentWeapon = weapon.WeaponId;
            }
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
             _weapons[weaponId].HasInfiniteAmmo))
        {
            _weapons.Remove(weaponId);

            // If the removed weapon was the currently selected one a new key
            // must be found
            if (_currentWeapon == weaponId)
            {
                foreach (int weapon in _weapons.Keys)
                {
                    _currentWeapon = weapon;
                    break;
                }
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
            {
                _visible = true;
            }
            else if (_flickerInterval <= 0)
            {
                if (_invincibleTime > 0)
                {
                    _visible = !_visible;
                    _flickerInterval = DefaultFlickerInterval;
                }
                else
                {
                    _visible = true;
                }
            }
        }

        if (_weapons.ContainsKey(_currentWeapon))
        {
            _weapons[_currentWeapon].Update(gameTime);
        }
    }

    public void StoreWeaponAmmoCount()
    {
        foreach (Weapon weapon in _weapons.Values)
        {
            weapon.StoreAmmoCount();
        }
    }

    public void ApplyStoredWeaponAmmoCount()
    {
        foreach (Weapon weapon in _weapons.Values)
        {
            weapon.ApplyStoredAmmoCount();
        }
    }

    public void Kill()
    {
        if (!_visible)
        {
            _visible = true;
        }

        // Stop the player's motion in X
        Velocity *= Vector2.UnitY;

        // Stop firing if needed
        if (_weapons.ContainsKey(_currentWeapon) &&
            _weapons[_currentWeapon].IsFiring)
        {
            _weapons[_currentWeapon].Stop();
        }
    }

    public override void TakeDamage(float damageLevel)
    {
        // Invincible?
        if (_invincibleTime > 0)
        {
            return;
        }

        // Decrease health
        Health -= damageLevel;

        // Decrease life count if the player died
        if (Health <= 0)
        {
            Kill();
        }
        else
        {
            _invincibleTime = DefaultInvincibleTime;
        }
    }

    public override void OnCollision(ICollidable collidedObject, List<BoxSide> collidedSides,
        Vector2 suggestedNewPosition, Vector2 suggestedNewVelocity)
    {
        if (collidedObject is not Tile)
        {
            return;
        }

        Position = suggestedNewPosition;
        Velocity = suggestedNewVelocity;
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 viewportPosition)
    {
        if (!_visible)
        {
            return;
        }

        // Draw player
        base.Draw(spriteBatch, viewportPosition);

        // Draw weapon
        if (_weapons.Count != 0 &&
            _weapons.ContainsKey(_currentWeapon) &&
            CurrentState != ActorState.Dying &&
            CurrentState != ActorState.Celebrating)
        {
            _weapons[_currentWeapon].Draw(spriteBatch, viewportPosition);
        }
    }

    public void LoadGame(SaveGame saveGame, WeaponLibrary weaponLibrary)
    {
        CurrentState = 0;

        _invincibleTime = 0;
        _flickerInterval = DefaultFlickerInterval;
        _visible = true;

        // Setup player
        Position = saveGame.PlayerValues.Position;
        Velocity = saveGame.PlayerValues.Velocity;
        Acceleration = saveGame.PlayerValues.Acceleration;

        Health = saveGame.PlayerHealth;

        // Clear previous weapons
        if (_weapons.Count > 0)
        {
            _weapons.Clear();
        }

        // Set the current weapon
        _currentWeapon = saveGame.SelectedWeapon;

        // Load in weapons and ammo
        foreach (int weaponId in saveGame.WeaponSaves.Keys)
        {
            WeaponSave weaponSave = saveGame.WeaponSaves[weaponId];

            // Get the saved weapon
            Weapon weapon = weaponLibrary.GetWeapon(weaponId).Copy();

            // Set the ammo
            weapon.AmmoCount = weaponSave.AmmoCount;
            weapon.AmmoMemory = weapon.AmmoCount == -1 ? -1 : weaponSave.StoredAmmo;

            weapon.Player = this;

            _weapons.Add(weapon.WeaponId, weapon);
        }
    }

    public void SaveGame(SaveGame saveGame)
    {
        var playerValues = new ActorStartValues
        {
            Position = Position,
            Velocity = Velocity,
            Acceleration = Acceleration
        };

        saveGame.PlayerValues = playerValues;
        saveGame.PlayerHealth = Health;

        // Weapons
        saveGame.SelectedWeapon = _currentWeapon;

        foreach (int weaponId in _weapons.Keys)
        {
            saveGame.WeaponSaves.Add(weaponId,
                new WeaponSave(_weapons[weaponId].AmmoCount, _weapons[weaponId].AmmoMemory));
        }
    }
}