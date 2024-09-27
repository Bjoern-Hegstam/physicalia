using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PhysicaliaRemastered.Actors;
using XNALibrary.Animation;
using XNALibrary.ParticleEngine;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.Weapons;

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

    public Rectangle WorldCollisionBox => new(
        (int)(Player!.Position.X + (Player!.IsFlippedHorizontally
            ? -CollisionBox.Width - PlayerOffset.X - CollisionBox.X
            : PlayerOffset.X + CollisionBox.X)),
        (int)(Player!.Position.Y + (Player!.IsFlippedVertically
            ? -CollisionBox.Height - PlayerOffset.X - CollisionBox.Y
            : PlayerOffset.Y + CollisionBox.Y)),
        CollisionBox.Width,
        CollisionBox.Height
    );

    protected Vector2 WorldWeaponPosition =>
        Player!.Position + new Vector2(
            Player.IsFlippedHorizontally
                ? -CurrentAnimation!.CurrentFrame.SourceRectangle.Width - PlayerOffset.X
                : PlayerOffset.X,
            Player.IsFlippedVertically
                ? -CurrentAnimation!.CurrentFrame.SourceRectangle.Height - PlayerOffset.Y
                : PlayerOffset.Y
        );

    public bool CanCollide { get; set; } = false;

    public float CollisionDamage { get; set; } = 0F;

    protected abstract void FireWeapon();

    protected abstract void OnStartFire();

    public abstract void LoadXml(XmlReader reader);

    public virtual void Start()
    {
        if (_ammoCount <= 0 && !HasInfiniteAmmo)
        {
            return;
        }

        _timeTillWeaponStart = WeaponWarmUpSeconds;
        _timeTillShot = 0;

        IsFiring = true;

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

    public virtual void Stop()
    {
        if (!IsFiring)
        {
            return;
        }

        _timeTillWeaponStart = WeaponWarmUpSeconds;
        _timeTillShot = 0;

        GamePad.SetVibration(PlayerIndex.One, 0, 0);

        WarmupAnimation?.Stop();

        IsFiring = false;
        WeaponFireAnimation?.Stop();
    }

    public void StoreAmmoCount()
    {
        AmmoMemory = _ammoCount;
    }

    public void ApplyStoredAmmoCount()
    {
        _ammoCount = AmmoMemory;
    }

    public Weapon Copy()
    {
        var weapon = (Weapon)MemberwiseClone();

        return weapon;
    }

    public void Update(GameTime gameTime)
    {
        WeaponFiredDuringLastUpdate = false;

        if (!IsFiring)
        {
            return;
        }

        if (_timeTillWeaponStart > 0)
        {
            _timeTillWeaponStart -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_timeTillWeaponStart > 0)
            {
                return;
            }

            WarmupAnimation?.Stop();
            WeaponFireAnimation?.Play();

            OnStartFire();
        }
        else if (_timeTillShot > 0)
        {
            _timeTillShot -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        else
        {
            FireWeapon();

            WeaponFiredDuringLastUpdate = true;

            if (IsFiring && (_ammoCount > 0 || HasInfiniteAmmo))
            {
                _timeTillShot += 1 / ShotsPerSecond;
            }
            else
            {
                Stop();
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 viewportPosition)
    {
        if (Player is null)
        {
            return;
        }

        Vector2 weaponPosition = WorldWeaponPosition;

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
            spriteBatch.DrawRectangle(WorldCollisionBox, Color.Red, viewportPosition);
        }
#endif
    }
}