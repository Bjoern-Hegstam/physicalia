using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNALibrary;
using XNALibrary.ParticleEngine;

namespace PhysicaliaRemastered.Weapons;

/// <summary>
/// Enum with values representing the different firing modes a weapon can use.
/// THe type of mode used also decides how vibration will be controlled.
/// </summary>
public enum FireMode
{
    /// <summary>
    /// A single shot is fired for each time the weapon is started. The
    /// controller then vibrates for an amount of seconds equal to 1 / ShotsPerSecond.
    /// </summary>
    SingleShot,

    /// <summary>
    /// The weapon fires a shot once every 2 / ShotPerSecond. The
    /// controller vibrates for each shot and then stops after 1 / shotPerSecond.
    /// </summary>
    MultiShot,

    /// <summary>
    /// The weapon fires a continuous stream of shots every 1 / ShotsPerSecond.
    /// The controller stays vibrating until the weapon is either stopped
    /// or runs out of ammunition.
    /// </summary>
    Continuous
}

public class ProjectileWeapon(int weaponId, ParticleEngine particleEngine) : Weapon(weaponId, particleEngine)
{
    private FireMode _fireMode;
    private Vector2 _warmupVibration = Vector2.Zero;
    private Vector2 _fireVibration = Vector2.Zero;

    private Vector2 _muzzlePosition;
    private float _maxDeviation;

    private int _projectilesPerShot = 1;
    private float _spread;

    private int _shotsFired;

    public override void Start()
    {
        GamePad.SetVibration(PlayerIndex.One, _warmupVibration.X, _warmupVibration.Y);

        _shotsFired = 0;

        base.Start();
    }

    public override void Stop()
    {
        GamePad.SetVibration(PlayerIndex.One, 0F, 0F);

        base.Stop();
    }

    protected override void OnStartFire()
    {
        // Coninuous fire means the vibration only needs to be started once
        // when the weapon starts firing
        if (_fireMode == FireMode.Continuous)
        {
            GamePad.SetVibration(PlayerIndex.One, _fireVibration.X, _fireVibration.Y);
        }
    }

    protected override void FireWeapon()
    {
        if (_fireMode == FireMode.SingleShot)
        {
            // Start vibrating if this is the first shot
            if (_shotsFired == 0)
            {
                GamePad.SetVibration(PlayerIndex.One, _fireVibration.X, _fireVibration.Y);
                FireShot();
                _shotsFired++;
            }
            else
            {
                // Stop the weapon
                Stop();
            }
        }
        else if (_fireMode == FireMode.MultiShot)
        {
            // Switch between starting and stopping the vibration
            if (_shotsFired % 2 == 0)
            {
                GamePad.SetVibration(PlayerIndex.One, _fireVibration.X, _fireVibration.Y);
                FireShot();
            }
            else
                // Stop vibration and dont fire any shots
            {
                GamePad.SetVibration(PlayerIndex.One, 0F, 0F);
            }

            // Increase so the other action will be taken next time
            _shotsFired++;
        }
        else
        {
            FireShot();
        }
    }

    private void FireShot()
    {
        if (ParticleId == null)
        {
            return;
        }

        // Get needed fire data
        Vector2 muzzle = GetMuzzlePosition();

        // Fire as many projectiles as specified.
        for (var i = 0; i < _projectilesPerShot; i++)
        {
            float angle = GetFireAngle(i);

            // Fire the projectile
            ParticleEngine.Add((int)ParticleId, 1, muzzle, angle);
        }

        // Decrease ammunition and count the number of fired shots
        AmmoCount--;
    }

    private Vector2 GetMuzzlePosition()
    {
        // Get the world position of the muzzle
        Vector2 muzzle = _muzzlePosition;

        // Adjust muzzle position to accomodate for any SpriteEffects applied
        // to the player.
        if ((Player.SpriteFlip & SpriteEffects.FlipHorizontally) != 0)
        {
            muzzle.X = WeaponFireAnimation.CurrentFrame.SourceRectangle.Width - _muzzlePosition.X -
                       Player.CollisionBoxDefinition.Width;
        }

        if ((Player.SpriteFlip & SpriteEffects.FlipVertically) != 0)
        {
            muzzle.Y = WeaponFireAnimation.CurrentFrame.SourceRectangle.Height - _muzzlePosition.Y;
        }

        // Adjust to world coordinates
        Vector2 playerTopLeft = Player.WorldCollisionBox.Location.ToVector2();
        muzzle = playerTopLeft - PlayerOffset + muzzle;

        // Randomly offset muzzle's position in Y within the given max deviation
        muzzle.Y += _maxDeviation * (float)Math.Sin(MathHelper.TwoPi * Random.Shared.NextDouble());

        return muzzle;
    }

    private float GetFireAngle(int projNum)
    {
        // Get the angle at which to fire the projectiles
        float angleSide = MathHelper.Pi * ((Player.SpriteFlip & SpriteEffects.FlipHorizontally) != 0 ? 1 : 0);

        if (_projectilesPerShot == 1)
        {
            return angleSide;
        }

        float angleStep = _spread / (_projectilesPerShot - 1);

        float angle = _spread / 2;
        angle += angleSide;
        angle -= projNum * angleStep;

        return angle;
    }

    public override void LoadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "FireMode" })
            {
                _fireMode = Enum.Parse<FireMode>(reader.ReadString());
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Vibration" })
            {
                reader.ReadToFollowing("Warmup");
                float low = float.Parse(reader.GetAttribute("low") ?? throw new ResourceLoadException());
                float high = float.Parse(reader.GetAttribute("high") ?? throw new ResourceLoadException());

                _warmupVibration = new Vector2(low, high);

                reader.ReadToFollowing("Fire");
                low = float.Parse(reader.GetAttribute("low") ?? throw new ResourceLoadException());
                high = float.Parse(reader.GetAttribute("high") ?? throw new ResourceLoadException());

                _fireVibration = new Vector2(low, high);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "MuzzlePosition" })
            {
                float x = float.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
                float y = float.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());
                _muzzlePosition = new Vector2(x, y);

                _maxDeviation = float.Parse(reader.GetAttribute("maxDeviation") ?? throw new ResourceLoadException());
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Ammunition" })
            {
                MaxAmmo = int.Parse(reader.GetAttribute("max") ?? throw new ResourceLoadException());
                AmmoCount = int.Parse(reader.GetAttribute("count") ?? throw new ResourceLoadException());

                _projectilesPerShot =
                    int.Parse(reader.GetAttribute("projectilesPerShot") ?? throw new ResourceLoadException());
                _spread = float.Parse(reader.GetAttribute("spread") ?? throw new ResourceLoadException());
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "WeaponData" })
            {
                return;
            }
        }
    }
}