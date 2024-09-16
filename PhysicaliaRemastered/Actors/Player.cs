using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Input;
using PhysicaliaRemastered.Weapons;
using Weapon = PhysicaliaRemastered.Weapons.NewWeapons.Weapon;

namespace PhysicaliaRemastered.Actors;

public class Player : Actor
{
    #region Movement fields

    public const float DEFAULT_JUMP_MAGNITUDE = -300F;
    public const float DEFAULT_WALK_SPEED = 100F;
    public const float DEFAULT_FALL_MOVEMENT_SPEED = 80F;

    public const float DEFAULT_INVINCIBLE_TIME = 1F;
    public const float DEFAULT_FLICKER_INTERVAL = 0.1F;

    private float jumpMagnitude;
    private float walkSpeed;
    private float fallMovementSpeed;

    // Life management
    private float invincibleTime;
    private float flickerInterval;
    private bool visible;

    public float JumpMagnitude
    {
        get { return this.jumpMagnitude; }
        set { this.jumpMagnitude = value; }
    }

    public float WalkSpeed
    {
        get { return this.walkSpeed; }
        set { this.walkSpeed = value; }
    }

    public float FallMovementSpeed
    {
        get { return this.fallMovementSpeed; }
        set { this.fallMovementSpeed = value; }
    }

    public bool Flickering
    {
        get { return this.invincibleTime > 0; }
        set
        {
                if (value)
                {
                    this.invincibleTime = DEFAULT_INVINCIBLE_TIME;
                }
                else
                {
                    this.invincibleTime = 0;
                    this.visible = true;
                }
            }
    }

    #endregion

    public override float Health
    {
        get { return base.Health; }
        set
        {
                base.Health = Math.Max(Math.Min(settings.PlayerStartHealth, value), 0);
            }
    }

    #region General Management

    private ISettings settings;

    public ISettings Settings
    {
        get { return this.settings; }
    }

    #endregion

    #region Weapons

    private Dictionary<int, Weapon> weapons;
    private int currentWeapon;

    public Weapon CurrentWeapon
    {
        get
        {
                if (this.weapons.ContainsKey(this.currentWeapon))
                    return this.weapons[this.currentWeapon];

                return null;
            }
    }

    #endregion

    public Player(ISettings settings)
    {
            this.settings = settings;

            this.jumpMagnitude = DEFAULT_JUMP_MAGNITUDE;
            this.walkSpeed = DEFAULT_WALK_SPEED;
            this.fallMovementSpeed = DEFAULT_FALL_MOVEMENT_SPEED;

            this.Health = settings.PlayerStartHealth;
            this.visible = true;

            this.weapons = new Dictionary<int, Weapon>();
        }

    /// <summary>
    /// Allows the Player to handle input.
    /// </summary>
    public void HandleInput()
    {
            Vector2 velocity = this.Velocity;
            velocity.X = 0;

            float moveSpeed = this.walkSpeed;
            if (this.CurrentAnimationType == (int)ActorAnimation.Fall ||
                this.CurrentAnimationType == (int)ActorAnimation.Jump)
                moveSpeed = this.fallMovementSpeed;

            // Walk
            if (this.Settings.InputMap.IsHolding(InputAction.WalkLeft))
                velocity.X = -moveSpeed;

            if (this.Settings.InputMap.IsHolding(InputAction.WalkRight))
                velocity.X = moveSpeed;

            // Jump
            if (this.Settings.InputMap.IsPressed(InputAction.Jump) &&
                this.CurrentAnimationType != (int)ActorAnimation.Jump &&
                this.CurrentAnimationType != (int)ActorAnimation.Fall)
                velocity.Y = this.jumpMagnitude * (this.Acceleration.Y > 0 ? 1 : -1);

            this.Velocity = velocity;

            // Weapon controls
            // Only check if the player has a weapon
            if (this.weapons.Count == 0)
                return;

            // Should the weapons be switched?
            if (this.Settings.InputMap.IsPressed(InputAction.NextWeapon))
            {
                int prevWeapon = this.currentWeapon;

                // Get the next weapon key
                foreach (int key in this.weapons.Keys)
                {
                    if (key > this.currentWeapon)
                    {
                        this.currentWeapon = key;
                        break;
                    }
                }

                // Was the previous weapon the last one?
                if (this.currentWeapon == prevWeapon &&
                    this.weapons.Count > 1)
                {
                    // Get the weapon before the current one
                    foreach (int key in this.weapons.Keys)
                    {
                        if (key < this.currentWeapon)
                            this.currentWeapon = key;
                        else
                            break;
                    }
                }

                if (this.weapons[prevWeapon].IsFiring && this.weapons.Count > 1)
                {
                    // Stop the previous weapon and start the next
                    this.weapons[prevWeapon].Stop();
                    this.weapons[this.currentWeapon].Start();
                }
            }

            if (this.Settings.InputMap.IsPressed(InputAction.PreviousWeapon))
            {
                // Get the weapon before the current one
                int nextWeapon = this.currentWeapon;
                foreach (int key in this.weapons.Keys)
                {
                    if (key < this.currentWeapon)
                    {
                        nextWeapon = key;
                    }
                    else
                        break;
                }

                // Was the previous weapon the first one?
                if (this.currentWeapon == nextWeapon &&
                    this.weapons.Count > 1)
                {
                    // Get the next weapon key
                    foreach (int key in this.weapons.Keys)
                    {
                        if (key > this.currentWeapon)
                        {
                            nextWeapon = key;
                        }
                    }
                }

                if (this.weapons[this.currentWeapon].IsFiring && this.weapons.Count > 1)
                {
                    // Stop the previous weapon and start the next
                    this.weapons[this.currentWeapon].Stop();
                    this.weapons[nextWeapon].Start();
                }

                this.currentWeapon = nextWeapon;
            }

            // Should the weapon be firing or stopped firing
            if (this.Settings.InputMap.IsPressed(InputAction.Shoot) &&
                !this.weapons[this.currentWeapon].IsFiring)
                this.weapons[this.currentWeapon].Start();

            if (this.Settings.InputMap.IsReleased(InputAction.Shoot) &&
                this.weapons[this.currentWeapon].IsFiring)
                this.weapons[this.currentWeapon].Stop();
        }

    #region Weapon control

    /// <summary>
    /// Clears the player's weapons.
    /// </summary>
    public void ClearWeapons()
    {
            this.weapons.Clear();
        }

    /// <summary>
    /// Adds a weapon to the player's arsenal. If the player already has a weapon
    /// with the same ID, then only the ammo of the new weapon is kept.
    /// </summary>
    /// <param name="weapon">Weapon to add.</param>
    public void AddWeapon(Weapon weapon)
    {
            // First weapon
            if (this.weapons.Count == 0)
                this.currentWeapon = weapon.WeaponID;

            // Keep only the ammo if the player already has a weapon of the same type
            if (this.weapons.ContainsKey(weapon.WeaponID))
                this.weapons[weapon.WeaponID].AmmoCount += weapon.AmmoCount;
            else
            {
                this.weapons.Add(weapon.WeaponID, weapon);
                weapon.Player = this;

                // Switch to weapon if it's better than the current one
                if (weapon.WeaponID > this.currentWeapon)
                    this.currentWeapon = weapon.WeaponID;
            }
        }

    /// <summary>
    /// Removes the weapon with the specified id from the player's
    /// collection of weapons.
    /// </summary>
    /// <param name="weaponID">Id of the weapon to remove.</param>
    public void RemoveWeapon(int weaponID, int ammoCount)
    {
            // If the weapon has a stored ammo count of 0 it was added
            // during the last level and should be removed. Also
            // weapons with infinite ammo should only be picked up once
            // and therefor removed when the player dies on the same level
            if (this.weapons.ContainsKey(weaponID) &&
                (this.weapons[weaponID].AmmoMemory == 0 ||
                this.weapons[weaponID].InfiniteAmmo))
            {
                this.weapons.Remove(weaponID);

                // If the removed weapon was the currently selected one a new key
                // must be found
                if (this.currentWeapon == weaponID)
                    foreach (int weapon in this.weapons.Keys)
                    {
                        this.currentWeapon = weapon;
                        break;
                    }
            }

        }

    public override void Update(GameTime gameTime)
    {
            base.Update(gameTime);

            // Update invincibility flicker
            if (this.invincibleTime > 0)
            {
                this.invincibleTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                this.flickerInterval -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (this.invincibleTime < 0)
                    this.visible = true;
                else if (this.flickerInterval <= 0)
                {
                    if (this.invincibleTime > 0)
                    {
                        this.visible = !this.visible;
                        this.flickerInterval = DEFAULT_FLICKER_INTERVAL;
                    }
                    else
                        this.visible = true;
                }
            }

            if (this.weapons.ContainsKey(this.currentWeapon))
                this.weapons[this.currentWeapon].Update(gameTime);
        }

    public void StoreWeaponAmmoCount()
    {
            foreach (Weapon weapon in this.weapons.Values)
                weapon.StoreAmmoCount();
        }

    public void ApplyStoredWeaponAmmoCount()
    {
            foreach (Weapon weapon in this.weapons.Values)
                weapon.ApplyStoredAmmoCount();
        }

    #endregion

    public void Kill()
    {
            if (!this.visible)
                this.visible = true;

            // Stop the player's motion in X
            this.Velocity *= Vector2.UnitY;

            // Stop firing if needed
            if (this.weapons.ContainsKey(this.currentWeapon) &&
                this.weapons[this.currentWeapon].IsFiring)
                this.weapons[this.currentWeapon].Stop();
        }

    #region ICollisionObject members

    public override ObjectType Type
    {
        get { return ObjectType.Player; }
    }

    public override void TakeDamage(float damageLevel)
    {
            // Invincible?
            if (this.invincibleTime > 0)
                return;

            // Decreaes health
            this.Health -= damageLevel;

            // Decrease life count if the player died
            if (this.Health <= 0)
            {
                this.Kill();
            }
            else
                this.invincibleTime = DEFAULT_INVINCIBLE_TIME;
        }

    public override void OnCollision(ICollisionObject collidedObject, BoxSide collisionSides, Vector2 position, Vector2 velocity)
    {
            if (collidedObject.Type == ObjectType.Tile)
            {
                this.Position = position;
                this.Velocity = velocity;

                // TODO: Fix so that player can't shoot through walls

            }
        }

    #endregion

    public override void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
            if (!this.visible)
                return;

            // Draw player
            base.Draw(spriteBatch, offsetPosition);

            // Draw weapon
            if (this.weapons.Count != 0 &&
                this.weapons.ContainsKey(this.currentWeapon) &&
                this.CurrentAnimationType != (int)ActorAnimation.Die &&
                this.CurrentAnimationType != (int)ActorAnimation.Win)
                this.weapons[this.currentWeapon].Draw(spriteBatch, offsetPosition, this.SpriteFlip);
        }

    #region Session management

    public void NewSession()
    {
            this.weapons.Clear();

            this.invincibleTime = 0;
            this.flickerInterval = DEFAULT_FLICKER_INTERVAL;
            this.visible = true;
        }

    public void LoadSession(GameSession session, WeaponBank weaponBank)
    {
            this.CurrentAnimationType = 0;

            this.invincibleTime = 0;
            this.flickerInterval = DEFAULT_FLICKER_INTERVAL;
            this.visible = true;

            // Setup player
            this.Position = session.PlayerValues.Position;
            this.Velocity = session.PlayerValues.Velocity;
            this.Acceleration = session.PlayerValues.Acceleration;

            this.Health = session.PlayerHealth;

            // Clear previous weapons
            if (this.weapons.Count > 0)
                this.weapons.Clear();

            // Set the current weapon
            this.currentWeapon = session.SelectedWeapon;

            // Load in weapons and ammo
            foreach (int weaponID in session.WeaponSaves.Keys)
            {
                WeaponSave weaponSave = session.WeaponSaves[weaponID];

                // Get the saved weapon
                Weapon weapon = weaponBank.GetWeapon(weaponID).Copy();

                // Set the ammo
                weapon.AmmoCount = weaponSave.AmmoCount;
                weapon.AmmoMemory = weapon.AmmoCount == -1 ? -1 : weaponSave.StoredAmmo;

                weapon.Player = this;

                this.weapons.Add(weapon.WeaponID, weapon);
            }
        }

    public void SaveSession(GameSession session)
    {
            ActorStartValues playerValues = new ActorStartValues();
            playerValues.Position = this.Position;
            playerValues.Velocity = this.Velocity;
            playerValues.Acceleration = this.Acceleration;

            session.PlayerValues = playerValues;
            session.PlayerHealth = this.Health;

            // Weapons
            session.SelectedWeapon = this.currentWeapon;

            foreach (int weaponID in this.weapons.Keys)
                session.WeaponSaves.Add(weaponID, new WeaponSave(this.weapons[weaponID].AmmoCount, this.weapons[weaponID].AmmoMemory));
        }

    #endregion
}