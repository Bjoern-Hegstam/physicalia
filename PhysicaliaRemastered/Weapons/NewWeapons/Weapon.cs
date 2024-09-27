using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PhysicaliaRemastered.Actors;
using XNALibrary.Animation;
using XNALibrary.Graphics;
using XNALibrary.ParticleEngine;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.Weapons.NewWeapons;

/// <summary>
/// Abstract class providing the basic characteristics of a weapon. The class
/// provides properties needed for managing over the weapon and methods for
/// operating it.
/// </summary>
public abstract class Weapon(int weaponId, ParticleEngine particleEngine)
{
    private float _timeTillWeaponStart;
    private float _timeTillShot;
    private int _ammoCount;

    public int WeaponId { get; set; } = weaponId;

    public Player? Player { get; set; } = null;
    public Vector2 PlayerOffset { get; set; } = Vector2.Zero;

    public ParticleEngine ParticleEngine { get; set; } = particleEngine;
    public int? ParticleId { get; set; }

    public Sprite? WeaponSprite { get; set; } = null;

    public Animation? WarmupAnimation { get; set; }
    public Animation? WeaponFireAnimation { get; set; } = null;

    public Animation? CurrentAnimation => _timeTillWeaponStart > 0 ? WarmupAnimation : WeaponFireAnimation;

    public float WeaponWarmUpSeconds { get; set; } = 5F;
    public float ShotsPerSecond { get; set; } = 0F;

    public bool IsFiring { get; protected set; }
    public bool WeaponFiredDuringLastUpdate { get; protected set; }

    public int AmmoCount
    {
        get => _ammoCount;
        set => _ammoCount = Math.Min(value, MaxAmmo);
    }

    public int MaxAmmo { get; set; } = 0;
    public bool HasInfiniteAmmo { get; set; } = false;
    public int AmmoMemory { get; set; }

    public Rectangle CollisionBox { get; set; } = Rectangle.Empty;

    public Rectangle AbsoluteCollisionBox => new(
        (int)(Player!.Position.X + (Player!.IsFlippedHorizontally
            ? -CollisionBox.Width - PlayerOffset.X - CollisionBox.X
            : PlayerOffset.X + CollisionBox.X)),
        (int)(Player!.Position.Y + (Player!.IsFlippedVertically
            ? -CollisionBox.Height - PlayerOffset.X - CollisionBox.Y
            : PlayerOffset.Y + CollisionBox.Y)),
        CollisionBox.Width,
        CollisionBox.Height
    );


    public bool CanCollide { get; set; } = false;
    public float CollisionDamage { get; set; } = 0F;

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
        if (_ammoCount > 0 || HasInfiniteAmmo)
        {
            // Prepare weapon
            _timeTillWeaponStart = WeaponWarmUpSeconds;
            _timeTillShot = 0;

            // Start animation and set the weapon's status to firing
            IsFiring = true;

            // Start playing warmup animation if needed otherwise start 
            if (WeaponWarmUpSeconds > 0)
            {
                WarmupAnimation.Play();
            }
            else
            {
                WeaponFireAnimation?.Play();
                OnStartFire();
            }
        }
    }

    public virtual void Stop()
    {
        if (IsFiring)
        {
            // Leave the weapon in the same state as when we started
            _timeTillWeaponStart = WeaponWarmUpSeconds;
            _timeTillShot = 0;

            // Stop any vibration
            GamePad.SetVibration(PlayerIndex.One, 0, 0);

            // Stop the warm up animation ot be on the safe side
            WarmupAnimation?.Stop();

            IsFiring = false;
            WeaponFireAnimation?.Stop();
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
        WeaponFiredDuringLastUpdate = false;

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
                    WarmupAnimation?.Stop();
                    WeaponFireAnimation?.Play();

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
                WeaponFiredDuringLastUpdate = true;

                // Prepare for the next round if we're still firing
                // and have the ammunition needed
                if (IsFiring && (_ammoCount > 0 || HasInfiniteAmmo))
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

    public void Draw(SpriteBatch spriteBatch, Vector2 viewportPosition)
    {
        if (Player is null)
        {
            return;
        }

        Vector2 weaponPosition = Player.Position + new Vector2(
            Player.IsFlippedHorizontally
                ? -CurrentAnimation!.CurrentFrame.SourceRectangle.Width - PlayerOffset.X
                : PlayerOffset.X,
            Player.IsFlippedVertically
                ? -CurrentAnimation!.CurrentFrame.SourceRectangle.Height - PlayerOffset.Y
                : PlayerOffset.Y
        );

#if DEBUG
        var boundingBox = new Rectangle(
            weaponPosition.ToPoint(),
            CurrentAnimation!.CurrentFrame.SourceRectangle.Size
        );

        spriteBatch.DrawRectangle(boundingBox, Color.Gray, viewportPosition);
#endif

        spriteBatch.Draw(
            CurrentAnimation.CurrentFrame.Texture,
            weaponPosition - viewportPosition,
            CurrentAnimation.CurrentFrame.SourceRectangle,
            Color.White,
            0.0F,
            Vector2.Zero,
            1.0F,
            Player.SpriteFlip,
            1.0F
        );
#if DEBUG
        if (CanCollide)
        {
            spriteBatch.DrawRectangle(AbsoluteCollisionBox, Color.Red, viewportPosition);
        }
#endif
    }
}