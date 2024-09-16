using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
    #region Fields

    private FireMode fireMode;
    private Vector2 warmupVibration;
    private Vector2 fireVibration;

    private Vector2 muzzlePosition;
    private float maxDeviation;

    private int projectilesPerShot;
    private float spread;

    private int shotsFired;

    #endregion

    #region Constructor

    public ProjectileWeapon(int weaponID, IParticleEngine particleEngine)
        : base(weaponID, particleEngine)
    {
            this.warmupVibration = this.fireVibration = Vector2.Zero;
            this.maxDeviation = 0F;
            this.projectilesPerShot = 1;
            this.spread = 0;
        }

    #endregion

    #region Weapon members

    public override void Start()
    {
            GamePad.SetVibration(PlayerIndex.One, this.warmupVibration.X, this.warmupVibration.Y);

            this.shotsFired = 0;

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
            if (this.fireMode == FireMode.Continuous)
                GamePad.SetVibration(PlayerIndex.One, this.fireVibration.X, this.fireVibration.Y);
        }

    protected override void FireWeapon()
    {
            if (this.fireMode == FireMode.SingleShot)
            {
                // Start vibrating if this is the first shot
                if (this.shotsFired == 0)
                {
                    GamePad.SetVibration(PlayerIndex.One, this.fireVibration.X, this.fireVibration.Y);
                    this.FireShot();
                    this.shotsFired++;
                }
                else
                {
                    // Stop the weapon
                    this.Stop();
                    return;
                }
            }
            else if (this.fireMode == FireMode.MultiShot)
            {
                // Switch between starting and stopping the vibration
                if (this.shotsFired % 2 == 0)
                {
                    GamePad.SetVibration(PlayerIndex.One, this.fireVibration.X, this.fireVibration.Y);
                    this.FireShot();
                }
                else
                    // Stop vibration and dont fire any shots
                    GamePad.SetVibration(PlayerIndex.One, 0F, 0F);

                // Increase so the other action will be taken next time
                this.shotsFired++;
            }
            else
                this.FireShot();
        }

    private void FireShot()
    {
            // Get needed fire data
            Vector2 muzzle = this.GetMuzzlePosition();

            // Fire as many projectiles as specified.
            for (int i = 0; i < this.projectilesPerShot; i++)
            {
                float angle = this.GetFireAngle(i);

                // Fire the projectile
                this.ParticleEngine.Add(this.ParticleID, 1, muzzle, angle);
            }

            // Decrease ammunition and count the number of fired shots
            this.AmmoCount--;
        }

    private Vector2 GetMuzzlePosition()
    {
            // Get the world position of the muzzle
            Vector2 muzzle = this.muzzlePosition;

            // Adjust muzzle position to accomodate for any SpriteEffects applied
            // to the player.
            if ((this.Player.SpriteFlip & SpriteEffects.FlipHorizontally) != 0)
                muzzle.X = this.WeaponFireAnimation.SourceRectangle.Width - this.muzzlePosition.X - this.Player.CollisionBox.Width;

            if ((this.Player.SpriteFlip & SpriteEffects.FlipVertically) != 0)
                muzzle.Y = this.WeaponFireAnimation.SourceRectangle.Height - this.muzzlePosition.Y;

            // Adjust to world coordinates
            Vector2 playerTopLeft = this.Player.Position - this.Player.Origin - new Vector2(this.Player.CollisionBox.X, this.Player.CollisionBox.Y);
            muzzle = playerTopLeft - this.PlayerOffset + muzzle;

            // Randomly offset muzzle's position in Y within the given max deviation
            muzzle.Y += this.maxDeviation * (float)Math.Sin(MathHelper.TwoPi * Settings.Random.NextDouble());

            return muzzle;
        }

    private float GetFireAngle(int projNum)
    {
            // Get the angle at which to fire the projectiles
            float angleSide = MathHelper.Pi * ((this.Player.SpriteFlip & SpriteEffects.FlipHorizontally) != 0 ? 1 : 0);

            if (this.projectilesPerShot == 1)
                return angleSide;
            else
            {
                float angleStep = this.spread / (this.projectilesPerShot - 1);

                float angle = this.spread / 2;
                angle += angleSide;
                angle -= projNum * angleStep;

                return angle;
            }
        }

    public override void LoadXml(System.Xml.XmlReader reader)
    {
            while (reader.Read())
            {
                if (reader.NodeType == System.Xml.XmlNodeType.Element &&
                    reader.LocalName == "FireMode")
                {
                    this.fireMode = (FireMode)Enum.Parse(typeof(FireMode), reader.ReadString());
                }

                if (reader.NodeType == System.Xml.XmlNodeType.Element &&
                    reader.LocalName == "Vibration")
                {
                    float low, high;
                    
                    reader.ReadToFollowing("Warmup");
                    low = float.Parse(reader.GetAttribute("low"));
                    high = float.Parse(reader.GetAttribute("high"));

                    this.warmupVibration = new Vector2(low, high);

                    reader.ReadToFollowing("Fire");
                    low = float.Parse(reader.GetAttribute("low"));
                    high = float.Parse(reader.GetAttribute("high"));

                    this.fireVibration = new Vector2(low, high);
                }

                if (reader.NodeType == System.Xml.XmlNodeType.Element &&
                    reader.LocalName == "MuzzlePosition")
                {
                    float x = float.Parse(reader.GetAttribute("x"));
                    float y = float.Parse(reader.GetAttribute("y"));
                    this.muzzlePosition = new Vector2(x, y);
                    
                    this.maxDeviation = float.Parse(reader.GetAttribute("maxDeviation"));
                }

                if (reader.NodeType == System.Xml.XmlNodeType.Element &&
                    reader.LocalName == "Ammunition")
                {
                    this.MaxAmmo = int.Parse(reader.GetAttribute("max"));
                    this.AmmoCount = int.Parse(reader.GetAttribute("count"));

                    this.projectilesPerShot = int.Parse(reader.GetAttribute("projectilesPerShot"));
                    this.spread = float.Parse(reader.GetAttribute("spread"));
                }

                if (reader.NodeType == System.Xml.XmlNodeType.EndElement &&
                    reader.LocalName == "WeaponData")
                    return;
            }
        }

    #endregion
}