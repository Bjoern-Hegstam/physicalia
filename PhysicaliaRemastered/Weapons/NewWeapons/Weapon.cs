using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PhysicaliaRemastered.Actors;
using XNALibrary.Animation;
using XNALibrary.ParticleEngine;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.Weapons.NewWeapons;

/// <summary>
/// Abstract class providing the basic characteristics of a weapon. The class
/// provides properties needed for managing over the weapon and methods for
/// operating it.
/// </summary>
public abstract class Weapon
{
    private Vector2 _playerOffset;

    protected bool FiredOnUpdate;

    private float _timeTillWeaponStart;

    private float _timeTillShot;


    private int _ammoCount;

    // Collision fields

    /// <summary>
    /// Gets a value denoting the weapon's type.
    /// </summary>
    public int WeaponId { get; set; }

    /// <summary>
    /// Gets or sets a reference to the player who owns the weapon.
    /// </summary>
    public Player Player { get; set; }

    /// <summary>
    /// Gets or sets the particleengine uesd by the weapon.
    /// </summary>
    public ParticleEngine ParticleEngine { get; set; }

    /// <summary>
    /// Gets or sets the position of the player's origin relative to the
    /// weapon's origin.
    /// </summary>
    public Vector2 PlayerOffset
    {
        get => _playerOffset;
        set => _playerOffset = value;
    }

    /// <summary>
    /// Gets or sets the id of the particle that the weapon can fire types of.
    /// </summary>
    public int ParticleId { get; set; }

    /// <summary>
    /// Gets or sets the Sprite that should be used for drawing the weapon when
    /// it's not used by the player.
    /// </summary>
    public Sprite WeaponSprite { get; set; }

    public Animation WarmupAnimation { get; set; }

    /// <summary>
    /// Gets or sets the animation used by the weapon when it gets to
    /// draw itself.
    /// </summary>
    public Animation WeaponFireAnimation { get; set; }

    /// <summary>
    /// Gets a value denoting whether the weapon is currently firing. The property
    /// can also be set by inheriting classes.
    /// </summary>
    public bool IsFiring { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the weapon was fired during the last
    /// call to its Update method. Can be set by inheriting classes.
    /// </summary>
    public bool WeaponFired
    {
        get => FiredOnUpdate;
        protected set => FiredOnUpdate = value;
    }

    /// <summary>
    /// Gets or sets the weapon's current amount of ammunition.
    /// </summary>
    public int AmmoCount
    {
        get => _ammoCount;
        set => _ammoCount = Math.Min(value, MaxAmmo);
    }

    /// <summary>
    /// Gets or sets the maximum amount of ammunition that the weapons can have.
    /// </summary>
    public int MaxAmmo { get; set; }

    /// <summary>
    /// Gets or sets a value denoting whether the weapon has an infinite supply
    /// of ammunition.
    /// </summary>
    public bool InfiniteAmmo { get; set; }

    /// <summary>
    /// Gets or sets the ammunition count previously stored in the weapon.
    /// </summary>
    public int AmmoMemory { get; set; }

    public Rectangle CollisionBox { get; set; }

    public bool CanCollide { get; set; }

    public float CollisionDamage { get; set; }

    public float WeaponWarmUp { get; set; }

    public float ShotsPerSecond { get; set; }

    public Weapon(int weaponId, ParticleEngine particleEngine)
    {
        WeaponId = weaponId;

        Player = null;
        _playerOffset = Vector2.Zero;
        ParticleEngine = particleEngine;

        WeaponSprite = new Sprite();
        WeaponFireAnimation = null;

        AmmoMemory = MaxAmmo = _ammoCount = 0;
        InfiniteAmmo = false;

        IsFiring = false;
        FiredOnUpdate = false;

        WeaponWarmUp = _timeTillWeaponStart = 5F;
        ShotsPerSecond = _timeTillShot = 0F;

        CanCollide = false;
        CollisionBox = Rectangle.Empty;
        CollisionDamage = 0F;
    }

    /// <summary>
    /// Called when the weapon is fired. Deriving classes can here decide how
    /// for example ammunition should be handled.
    /// </summary>
    protected abstract void FireWeapon();

    /// <summary>
    /// Called when the weapon starts firing.
    /// </summary>
    protected abstract void OnStartFire();

    public abstract void LoadXml(XmlReader reader);

    public virtual void Start()
    {
        if (_ammoCount > 0 || InfiniteAmmo)
        {
            // Prepare weapon
            _timeTillWeaponStart = WeaponWarmUp;
            _timeTillShot = 0;

            // Start animation and set the weapon's status to firing
            IsFiring = true;

            // Start playing warmup animation if needed otherwise start 
            if (WeaponWarmUp > 0)
            {
                WarmupAnimation.Play();
            }
            else
            {
                WeaponFireAnimation.Play();
                OnStartFire();
            }
        }
    }

    public virtual void Stop()
    {
        if (IsFiring)
        {
            // Leave the weapon in the same state as when we started
            _timeTillWeaponStart = WeaponWarmUp;
            _timeTillShot = 0;

            // Stop any vibration
            GamePad.SetVibration(PlayerIndex.One, 0, 0);

            // Stop the warm up animation ot be on the safe side
            WarmupAnimation.Stop();

            IsFiring = false;
            WeaponFireAnimation.Stop();
        }
    }

    /// <summary>
    /// Stores the current amount of ammunition.
    /// </summary>
    public void StoreAmmoCount()
    {
        AmmoMemory = _ammoCount;
    }

    /// <summary>
    /// Restores the ammunition amount to the previous stored one.
    /// </summary>
    public void ApplyStoredAmmoCount()
    {
        _ammoCount = AmmoMemory;
    }

    /// <summary>
    /// Creates a copy of the current Weapon.
    /// </summary>
    /// <returns>A copy of the current Weapon.</returns>
    public Weapon Copy()
    {
        var weapon = (Weapon)MemberwiseClone();

        return weapon;
    }

    /// <summary>
    /// Updates the weapon.
    /// </summary>
    /// <param name="gameTime"></param>
    public void Update(GameTime gameTime)
    {
        // Start by assuming that the weapon hasn't been fired
        FiredOnUpdate = false;

        // See if the weapon is firing
        if (IsFiring)
        {
            // See if the weapon is warming up
            if (_timeTillWeaponStart > 0)
            {
                _timeTillWeaponStart -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Go from warm up to firing if it's time
                if (_timeTillWeaponStart <= 0)
                {
                    WarmupAnimation.Stop();
                    WeaponFireAnimation.Play();

                    OnStartFire();
                }
            }
            else if (_timeTillShot > 0)
            {
                _timeTillShot -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                // Fire the weapon
                FireWeapon();

                // Let others know that the weapon was fired
                FiredOnUpdate = true;

                // Prepare for the next round if we're still firing
                // and have the ammunition needed
                if (IsFiring && (_ammoCount > 0 || InfiniteAmmo))
                {
                    // Prepare weapon
                    _timeTillShot += 1 / ShotsPerSecond;
                }
                else
                {
                    // Weapon can no longer be fired and must therefore be stopped
                    Stop();
                }
            }
        }
    }

    /// <summary>
    /// Draws the weapon with its origin at the player's origin, but offset as
    /// specified by the Weapon.PlayerOffset property.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to use for drawing.</param>
    /// <param name="positionOffset">The position of the screen.</param>
    /// <param name="spriteEffects">Effects to apply to the weapon in order
    /// for it to correspond with the player.</param>
    public void Draw(SpriteBatch spriteBatch, Vector2 positionOffset, SpriteEffects spriteEffects)
    {
        // TODO: Methods only works for animations with width 64px

        // Weapon origin is at the same position as the player's origin.
        // This makes it much easier to accurately position the weapon
        // when using SpriteEffects.
        Vector2 origin = Player.Origin + new Vector2(Player.CollisionBox.X, Player.CollisionBox.Y);

        // Worldposition of the weapon
        Vector2 position = Player.Position;

        // If the player is flipped horizontally then we need to
        // subtract the width of its collisionbox to properly
        // place the weapon
        if ((spriteEffects & SpriteEffects.FlipHorizontally) != 0)
        {
            position.X -= Player.CollisionBox.Width;
            origin.X -= _playerOffset.X;
        }
        else
        {
            origin.X += _playerOffset.X;
        }

        if ((spriteEffects & SpriteEffects.FlipVertically) != 0)
        {
            origin.Y -= _playerOffset.Y;
        }
        else
        {
            origin.Y += _playerOffset.Y;
        }

        Animation weaponAnim = null;

        if (_timeTillWeaponStart > 0)
        {
            weaponAnim = WarmupAnimation;
        }
        else
        {
            weaponAnim = WeaponFireAnimation;
        }

        spriteBatch.Draw(weaponAnim.Texture,
            position - positionOffset,
            weaponAnim.SourceRectangle,
            Color.White,
            0.0F,
            origin,
            1.0F,
            spriteEffects,
            1.0F);
    }
}