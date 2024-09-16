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
        get => jumpMagnitude;
        set => jumpMagnitude = value;
    }

    public float WalkSpeed
    {
        get => walkSpeed;
        set => walkSpeed = value;
    }

    public float FallMovementSpeed
    {
        get => fallMovementSpeed;
        set => fallMovementSpeed = value;
    }

    public bool Flickering
    {
        get => invincibleTime > 0;
        set
        {
                if (value)
                {
                    invincibleTime = DEFAULT_INVINCIBLE_TIME;
                }
                else
                {
                    invincibleTime = 0;
                    visible = true;
                }
            }
    }

    public override float Health
    {
        get => base.Health;
        set => base.Health = Math.Max(Math.Min(settings.PlayerStartHealth, value), 0);
    }

    private ISettings settings;

    public ISettings Settings => settings;

    private Dictionary<int, Weapon> weapons;
    private int currentWeapon;

    public Weapon CurrentWeapon
    {
        get
        {
                if (weapons.ContainsKey(currentWeapon))
                    return weapons[currentWeapon];

                return null;
            }
    }

    public Player(ISettings settings)
    {
            this.settings = settings;

            jumpMagnitude = DEFAULT_JUMP_MAGNITUDE;
            walkSpeed = DEFAULT_WALK_SPEED;
            fallMovementSpeed = DEFAULT_FALL_MOVEMENT_SPEED;

            Health = settings.PlayerStartHealth;
            visible = true;

            weapons = new Dictionary<int, Weapon>();
        }

    /// <summary>
    /// Allows the Player to handle input.
    /// </summary>
    public void HandleInput()
    {
            Vector2 velocity = Velocity;
            velocity.X = 0;

            float moveSpeed = walkSpeed;
            if (CurrentAnimationType == (int)ActorAnimation.Fall ||
                CurrentAnimationType == (int)ActorAnimation.Jump)
                moveSpeed = fallMovementSpeed;

            // Walk
            if (Settings.InputMap.IsHolding(InputAction.WalkLeft))
                velocity.X = -moveSpeed;

            if (Settings.InputMap.IsHolding(InputAction.WalkRight))
                velocity.X = moveSpeed;

            // Jump
            if (Settings.InputMap.IsPressed(InputAction.Jump) &&
                CurrentAnimationType != (int)ActorAnimation.Jump &&
                CurrentAnimationType != (int)ActorAnimation.Fall)
                velocity.Y = jumpMagnitude * (Acceleration.Y > 0 ? 1 : -1);

            Velocity = velocity;

            // Weapon controls
            // Only check if the player has a weapon
            if (weapons.Count == 0)
                return;

            // Should the weapons be switched?
            if (Settings.InputMap.IsPressed(InputAction.NextWeapon))
            {
                int prevWeapon = currentWeapon;

                // Get the next weapon key
                foreach (int key in weapons.Keys)
                {
                    if (key > currentWeapon)
                    {
                        currentWeapon = key;
                        break;
                    }
                }

                // Was the previous weapon the last one?
                if (currentWeapon == prevWeapon &&
                    weapons.Count > 1)
                {
                    // Get the weapon before the current one
                    foreach (int key in weapons.Keys)
                    {
                        if (key < currentWeapon)
                            currentWeapon = key;
                        else
                            break;
                    }
                }

                if (weapons[prevWeapon].IsFiring && weapons.Count > 1)
                {
                    // Stop the previous weapon and start the next
                    weapons[prevWeapon].Stop();
                    weapons[currentWeapon].Start();
                }
            }

            if (Settings.InputMap.IsPressed(InputAction.PreviousWeapon))
            {
                // Get the weapon before the current one
                int nextWeapon = currentWeapon;
                foreach (int key in weapons.Keys)
                {
                    if (key < currentWeapon)
                    {
                        nextWeapon = key;
                    }
                    else
                        break;
                }

                // Was the previous weapon the first one?
                if (currentWeapon == nextWeapon &&
                    weapons.Count > 1)
                {
                    // Get the next weapon key
                    foreach (int key in weapons.Keys)
                    {
                        if (key > currentWeapon)
                        {
                            nextWeapon = key;
                        }
                    }
                }

                if (weapons[currentWeapon].IsFiring && weapons.Count > 1)
                {
                    // Stop the previous weapon and start the next
                    weapons[currentWeapon].Stop();
                    weapons[nextWeapon].Start();
                }

                currentWeapon = nextWeapon;
            }

            // Should the weapon be firing or stopped firing
            if (Settings.InputMap.IsPressed(InputAction.Shoot) &&
                !weapons[currentWeapon].IsFiring)
                weapons[currentWeapon].Start();

            if (Settings.InputMap.IsReleased(InputAction.Shoot) &&
                weapons[currentWeapon].IsFiring)
                weapons[currentWeapon].Stop();
        }

    /// <summary>
    /// Clears the player's weapons.
    /// </summary>
    public void ClearWeapons()
    {
            weapons.Clear();
        }

    /// <summary>
    /// Adds a weapon to the player's arsenal. If the player already has a weapon
    /// with the same ID, then only the ammo of the new weapon is kept.
    /// </summary>
    /// <param name="weapon">Weapon to add.</param>
    public void AddWeapon(Weapon weapon)
    {
            // First weapon
            if (weapons.Count == 0)
                currentWeapon = weapon.WeaponID;

            // Keep only the ammo if the player already has a weapon of the same type
            if (weapons.ContainsKey(weapon.WeaponID))
                weapons[weapon.WeaponID].AmmoCount += weapon.AmmoCount;
            else
            {
                weapons.Add(weapon.WeaponID, weapon);
                weapon.Player = this;

                // Switch to weapon if it's better than the current one
                if (weapon.WeaponID > currentWeapon)
                    currentWeapon = weapon.WeaponID;
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
            if (weapons.ContainsKey(weaponID) &&
                (weapons[weaponID].AmmoMemory == 0 ||
                weapons[weaponID].InfiniteAmmo))
            {
                weapons.Remove(weaponID);

                // If the removed weapon was the currently selected one a new key
                // must be found
                if (currentWeapon == weaponID)
                    foreach (int weapon in weapons.Keys)
                    {
                        currentWeapon = weapon;
                        break;
                    }
            }

        }

    public override void Update(GameTime gameTime)
    {
            base.Update(gameTime);

            // Update invincibility flicker
            if (invincibleTime > 0)
            {
                invincibleTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                flickerInterval -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (invincibleTime < 0)
                    visible = true;
                else if (flickerInterval <= 0)
                {
                    if (invincibleTime > 0)
                    {
                        visible = !visible;
                        flickerInterval = DEFAULT_FLICKER_INTERVAL;
                    }
                    else
                        visible = true;
                }
            }

            if (weapons.ContainsKey(currentWeapon))
                weapons[currentWeapon].Update(gameTime);
        }

    public void StoreWeaponAmmoCount()
    {
            foreach (Weapon weapon in weapons.Values)
                weapon.StoreAmmoCount();
        }

    public void ApplyStoredWeaponAmmoCount()
    {
            foreach (Weapon weapon in weapons.Values)
                weapon.ApplyStoredAmmoCount();
        }

    public void Kill()
    {
            if (!visible)
                visible = true;

            // Stop the player's motion in X
            Velocity *= Vector2.UnitY;

            // Stop firing if needed
            if (weapons.ContainsKey(currentWeapon) &&
                weapons[currentWeapon].IsFiring)
                weapons[currentWeapon].Stop();
        }

    public override ObjectType Type => ObjectType.Player;

    public override void TakeDamage(float damageLevel)
    {
            // Invincible?
            if (invincibleTime > 0)
                return;

            // Decreaes health
            Health -= damageLevel;

            // Decrease life count if the player died
            if (Health <= 0)
            {
                Kill();
            }
            else
                invincibleTime = DEFAULT_INVINCIBLE_TIME;
        }

    public override void OnCollision(ICollisionObject collidedObject, BoxSide collisionSides, Vector2 position, Vector2 velocity)
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
            if (!visible)
                return;

            // Draw player
            base.Draw(spriteBatch, offsetPosition);

            // Draw weapon
            if (weapons.Count != 0 &&
                weapons.ContainsKey(currentWeapon) &&
                CurrentAnimationType != (int)ActorAnimation.Die &&
                CurrentAnimationType != (int)ActorAnimation.Win)
                weapons[currentWeapon].Draw(spriteBatch, offsetPosition, SpriteFlip);
        }

    public void NewSession()
    {
            weapons.Clear();

            invincibleTime = 0;
            flickerInterval = DEFAULT_FLICKER_INTERVAL;
            visible = true;
        }

    public void LoadSession(GameSession session, WeaponBank weaponBank)
    {
            CurrentAnimationType = 0;

            invincibleTime = 0;
            flickerInterval = DEFAULT_FLICKER_INTERVAL;
            visible = true;

            // Setup player
            Position = session.PlayerValues.Position;
            Velocity = session.PlayerValues.Velocity;
            Acceleration = session.PlayerValues.Acceleration;

            Health = session.PlayerHealth;

            // Clear previous weapons
            if (weapons.Count > 0)
                weapons.Clear();

            // Set the current weapon
            currentWeapon = session.SelectedWeapon;

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

                weapons.Add(weapon.WeaponID, weapon);
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
            session.SelectedWeapon = currentWeapon;

            foreach (int weaponID in weapons.Keys)
                session.WeaponSaves.Add(weaponID, new WeaponSave(weapons[weaponID].AmmoCount, weapons[weaponID].AmmoMemory));
        }
}