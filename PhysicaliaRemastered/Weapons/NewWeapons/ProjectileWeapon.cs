using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PhysicaliaRemastered.GameManagement;
using XNALibrary.ParticleEngine;

namespace PhysicaliaRemastered.Weapons.NewWeapons;

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
    /// The weapon fires a shot once every 2 / ShotperSecond. The
    /// controller vibrates for each shot and and then stops after 1 / shotPerSecond.
    /// </summary>
    MultiShot,

    /// <summary>
    /// The weapon fires a continous stream of shots every 1 / ShotsPerSecond.
    /// The controller stays vibrating until the weapon is either stopped
    /// or runs out of ammunition.
    /// </summary>
    Continuous
}

public class ProjectileWeapon : Weapon
{
    private FireMode _fireMode;
    private Vector2 _warmupVibration;
    private Vector2 _fireVibration;

    private Vector2 _muzzlePosition;
    private float _maxDeviation;

    private int _projectilesPerShot;
    private float _spread;

    private int _shotsFired;

    public ProjectileWeapon(int weaponId, IParticleEngine particleEngine)
        : base(weaponId, particleEngine)
    {
        _warmupVibration = _fireVibration = Vector2.Zero;
        _maxDeviation = 0F;
        _projectilesPerShot = 1;
        _spread = 0;
    }

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
            GamePad.SetVibration(PlayerIndex.One, _fireVibration.X, _fireVibration.Y);
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
                return;
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
                GamePad.SetVibration(PlayerIndex.One, 0F, 0F);

            // Increase so the other action will be taken next time
            _shotsFired++;
        }
        else
            FireShot();
    }

    private void FireShot()
    {
        // Get needed fire data
        Vector2 muzzle = GetMuzzlePosition();

        // Fire as many projectiles as specified.
        for (int i = 0; i < _projectilesPerShot; i++)
        {
            float angle = GetFireAngle(i);

            // Fire the projectile
            ParticleEngine.Add(ParticleId, 1, muzzle, angle);
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
            muzzle.X = WeaponFireAnimation.SourceRectangle.Width - _muzzlePosition.X - Player.CollisionBox.Width;

        if ((Player.SpriteFlip & SpriteEffects.FlipVertically) != 0)
            muzzle.Y = WeaponFireAnimation.SourceRectangle.Height - _muzzlePosition.Y;

        // Adjust to world coordinates
        Vector2 playerTopLeft =
            Player.Position - Player.Origin - new Vector2(Player.CollisionBox.X, Player.CollisionBox.Y);
        muzzle = playerTopLeft - PlayerOffset + muzzle;

        // Randomly offset muzzle's position in Y within the given max deviation
        muzzle.Y += _maxDeviation * (float)Math.Sin(MathHelper.TwoPi * Settings.Random.NextDouble());

        return muzzle;
    }

    private float GetFireAngle(int projNum)
    {
        // Get the angle at which to fire the projectiles
        float angleSide = MathHelper.Pi * ((Player.SpriteFlip & SpriteEffects.FlipHorizontally) != 0 ? 1 : 0);

        if (_projectilesPerShot == 1)
            return angleSide;
        else
        {
            float angleStep = _spread / (_projectilesPerShot - 1);

            float angle = _spread / 2;
            angle += angleSide;
            angle -= projNum * angleStep;

            return angle;
        }
    }

    public override void LoadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "FireMode")
            {
                _fireMode = (FireMode)Enum.Parse(typeof(FireMode), reader.ReadString());
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Vibration")
            {
                float low, high;

                reader.ReadToFollowing("Warmup");
                low = float.Parse(reader.GetAttribute("low"));
                high = float.Parse(reader.GetAttribute("high"));

                _warmupVibration = new Vector2(low, high);

                reader.ReadToFollowing("Fire");
                low = float.Parse(reader.GetAttribute("low"));
                high = float.Parse(reader.GetAttribute("high"));

                _fireVibration = new Vector2(low, high);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "MuzzlePosition")
            {
                float x = float.Parse(reader.GetAttribute("x"));
                float y = float.Parse(reader.GetAttribute("y"));
                _muzzlePosition = new Vector2(x, y);

                _maxDeviation = float.Parse(reader.GetAttribute("maxDeviation"));
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Ammunition")
            {
                MaxAmmo = int.Parse(reader.GetAttribute("max"));
                AmmoCount = int.Parse(reader.GetAttribute("count"));

                _projectilesPerShot = int.Parse(reader.GetAttribute("projectilesPerShot"));
                _spread = float.Parse(reader.GetAttribute("spread"));
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "WeaponData")
                return;
        }
    }
}