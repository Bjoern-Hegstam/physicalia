using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Actors;

namespace PhysicaliaRemastered.Weapons;

public class Weapon
{
    #region Fields and Properties

    // Major objects
    private Player player;
    private IParticleEngine particleEngine;
    private IAnimationManager animationManager;

    public Player Player
    {
        get { return this.player; }
        set { this.player = value; }
    }

    // Graphics
    private Animation weaponAnim;
    private int weaponID;
    private Sprite weaponSprite;
    private Vector2 playerOffset;
    private Vector2 muzzlePosition;
    private float maxDeviation;

    /// <summary>
    /// Gets or sets the player's position relative to the weapon. The offset
    /// represents the position of the player's top-left corner on the weapon
    /// sprite.
    /// </summary>
    public Vector2 PlayerOffset
    {
        get { return this.playerOffset; }
        set { this.playerOffset = value; }
    }

    public Sprite WeaponSprite
    {
        get { return this.weaponSprite; }
        set { this.weaponSprite = value; }
    }

    /// <summary>
    /// Gets the ID of this weapon type.
    /// </summary>
    public int TypeID
    {
        get { return this.weaponID; }
        set { this.weaponID = value; }
    }

    public Vector2 MuzzlePosition
    {
        get { return this.muzzlePosition; }
        set { this.muzzlePosition = value; }
    }

    public float MaxDeviation
    {
        get { return this.maxDeviation; }
        set { this.maxDeviation = value; }
    }

    // Weapon control
    private bool weaponFired;

    private int projectileID;
    private float fireRate; // Shots/s
    private float spread;
    private int projectilesPerShot;
    private bool firing;
    private int ammoCount;
    private int maxAmmo;
    private bool infiniteAmmo;
    protected float timeTillShot;

    private int ammoMemory;

    /// <summary>
    /// Gets or sets the id of the particle type that represent a shot
    /// from the weapon.
    /// </summary>
    public int ProjectileID
    {
        get { return this.projectileID; }
        set { this.projectileID = value; }
    }

    /// <summary>
    /// Gets or sets the number of shots fired per second.
    /// </summary>
    public float FireRate
    {
        get { return this.fireRate; }
        set { this.fireRate = value; }
    }

    public float Spread
    {
        get { return this.spread; }
        set { this.spread = value; }
    }

    public int ProjectilesPerShot
    {
        get { return this.projectilesPerShot; }
        set { this.projectilesPerShot = value; }
    }

    public bool IsFiring
    {
        get { return this.firing; }
    }

    public bool WeaponFired
    {
        get { return this.weaponFired; }
    }

    public int AmmoCount
    {
        get { return this.ammoCount; }
        set { this.ammoCount = Math.Min(value, this.maxAmmo); }
    }

    public int MaxAmmo
    {
        get { return this.maxAmmo; }
        set { this.maxAmmo = value; }
    }

    public bool InfiniteAmmo
    {
        get { return this.infiniteAmmo; }
        set { this.infiniteAmmo = value; }
    }

    public int AmmoMemory
    {
        get { return this.ammoMemory; }
        set { this.ammoMemory = value; }
    }

    // CollisionChecking
    private bool canCollide;
    private Rectangle collisionBox;
    private float collisionDamage;

    public Rectangle CollisionBox
    {
        get { return this.collisionBox; }
        set { this.collisionBox = value; }
    }

    public bool CanCollide
    {
        get { return this.canCollide; }
        set { this.canCollide = value; }
    }

    public float CollisionDamage
    {
        get { return this.collisionDamage; }
        set { this.collisionDamage = value; }
    }

    #endregion

    public Weapon(IParticleEngine particleEngine, IAnimationManager animationManager)
    {
            this.player = null;
            this.animationManager = animationManager;
            this.particleEngine = particleEngine;

            this.firing = false;
            this.spread = 0F;
            this.projectilesPerShot = 1;
            this.projectileID = -1;
            this.fireRate = 0F;
            this.maxAmmo = 0;
            this.ammoCount = 0;
            this.infiniteAmmo = false;
            this.weaponFired = false;

            this.weaponAnim = null;
            this.weaponSprite = new Sprite();

            this.canCollide = false;
            this.collisionBox = Rectangle.Empty;
            this.collisionDamage = 0F;
        }

    #region Xml reading

    public void LoadXml(XmlReader reader, ISpriteLibrary spriteLibrary, IAnimationManager animationManager)
    {
            // Setup according to xml
            while (reader.Read())
            {
                // Graphics
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Graphics")
                {
                    this.ParseGraphicsData(reader, spriteLibrary);
                }

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "MuzzlePosition")
                {
                    int x = int.Parse(reader.GetAttribute("x")); int y = int.Parse(reader.GetAttribute("y"));
                    this.muzzlePosition = new Vector2(x, y);
                }

                // Fire data
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "FireData")
                {
                    reader.ReadToFollowing("Projectile");
                    this.projectileID = int.Parse(reader.ReadElementContentAsString());

                    this.fireRate = int.Parse(reader.ReadElementContentAsString());

                    this.ammoCount = int.Parse(reader.GetAttribute("count"));
                    this.maxAmmo = int.Parse(reader.GetAttribute("max"));

                    this.infiniteAmmo = (this.ammoCount == -1 ? true : false);
                }

                // Collision
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Collision")
                {
                    this.canCollide = bool.Parse(reader.GetAttribute("canCollide"));
                    this.collisionDamage = float.Parse(reader.GetAttribute("collisionDamage"));
                    
                    reader.ReadToFollowing("CollisionRectangle");
                    this.collisionBox = this.ReadRectangle(reader);
                }

                // End of weapon element?
                if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.LocalName == "Weapon")
                    return;
            }
        }

    private void ParseGraphicsData(XmlReader reader, ISpriteLibrary spriteLibrary)
    {
            while (reader.Read())
            {
                // Sprite element
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Sprite")
                {
                    int spriteKey = int.Parse(reader.GetAttribute(0));
                    this.weaponSprite = spriteLibrary.GetSprite(spriteKey);
                }

                // Animation element
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Animation")
                {
                    int key = int.Parse(reader.GetAttribute("key"));
                    Animation anim = this.animationManager.GetBankAnimation(key).Copy();
                    this.animationManager.AddPlaybackAnimation(anim);

                    this.weaponAnim = anim;
                }

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "PlayerOffset")
                {
                    int x = int.Parse(reader.GetAttribute("x")); int y = int.Parse(reader.GetAttribute("y"));
                    this.playerOffset = new Vector2(x, y);
                }

                // End of graphics element?
                if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.LocalName == "Graphics")
                    return;
            }
        }

    private Rectangle ReadRectangle(XmlReader reader)
    {
            int x = int.Parse(reader.GetAttribute("x"));
            int y = int.Parse(reader.GetAttribute("y"));
            int width = int.Parse(reader.GetAttribute("width"));
            int height = int.Parse(reader.GetAttribute("height"));

            return new Rectangle(x, y, width, height);
        }

    #endregion

    #region Firing

    public void Update(GameTime gameTime)
    {
            this.weaponFired = false;

            // Fire projectiles if needed
            if (this.firing &&
                (this.infiniteAmmo || this.ammoCount > 0))
            {

                this.timeTillShot -= (float)gameTime.ElapsedGameTime.TotalSeconds * this.fireRate;

                // Loop to make sure that all shots there are time for are fired
                while (this.timeTillShot <= 0)
                {
                    this.weaponFired = true;

                    // Only fire a projectile if the weapon has a valid projectile id
                    if (this.particleEngine.HasDefinition(this.projectileID))
                    {
                        // Get the position of the muzzle relative to the level
                        Vector2 muzzle = this.muzzlePosition;

                        if ((this.player.SpriteFlip & SpriteEffects.FlipHorizontally) != 0)
                            muzzle.X = this.weaponAnim.SourceRectangle.Width - this.muzzlePosition.X - this.player.CollisionBox.Width;

                        if ((this.player.SpriteFlip & SpriteEffects.FlipVertically) != 0)
                            muzzle.Y = this.weaponAnim.SourceRectangle.Height - this.muzzlePosition.Y;

                        Vector2 playerTopLeft = this.player.Position - this.player.Origin - new Vector2(this.player.CollisionBox.X, this.player.CollisionBox.Y);
                        muzzle = playerTopLeft - this.playerOffset + muzzle;

                        // Apply deviation to the projectile
                        muzzle.Y -= this.maxDeviation / 2;
                        muzzle.Y += this.maxDeviation * (float)Math.Sin(Settings.Random.NextDouble());

                        float angleSide = MathHelper.Pi * ((this.player.SpriteFlip & SpriteEffects.FlipHorizontally) != 0 ? 1 : 0);
                        float angleStep = this.spread / this.projectilesPerShot;
                        float angle = this.projectilesPerShot == 1 ? 0 : this.spread / 2;
                        angle += angleSide;

                        for (int i = 0; i < this.projectilesPerShot; i++)
                        {
                            this.particleEngine.Add(this.projectileID, 1, muzzle, angle);
                            angle -= angleStep;
                        }
                    }

                    this.timeTillShot += 1 / this.fireRate;

                    if (!this.infiniteAmmo)
                    {
                        this.ammoCount--;

                        if (this.ammoCount <= 0)
                        {
                            this.StopFiring();
                            break;
                        }
                    }
                }
            }
        }

    public void StartFiring()
    {
            // Start animation
            if (this.ammoCount > 0 || this.infiniteAmmo)
            {
                this.weaponAnim.Play();
                this.firing = true;
            }
        }

    public void StopFiring()
    {
            this.timeTillShot = 0;

            // Stop animation
            if (this.firing)
            {
                this.weaponAnim.Stop();
                this.firing = false;
            }
        }

    #endregion

    public Weapon Copy(IAnimationManager animationManager)
    {
            //Weapon weapon = new Weapon(this.particleEngine, this.animationManager);
            Weapon weapon = (Weapon)this.MemberwiseClone();

            // Copy animation and add to the IAnimationManager
           /*

            eapon.weaponAnim = this.weaponAnim.Copy();

            his.animationManager.AddPlaybackAnimation(weapon.weaponAnim);

            //
            return weapon;
        }

    #region Drawing

    public void Draw(SpriteBatch spriteBatch, Vector2 positionOffset, SpriteEffects spriteEffects)
    {
            // Weapon origin is at the same position as the player's origin
            Vector2 origin = this.player.Origin + new Vector2(this.player.CollisionBox.X, this.player.CollisionBox.Y);
            //Vector2 origin = this.playerOffset + this.player.Origin + new Vector2(this.player.CollisionBox.X, this.player.CollisionBox.Y);
            // Worldposition of the weapon
            Vector2 position = this.player.Position;

            // If the player is flipped horizontally then we need to
            // subtract the width of its collisionbox to properly
            // place the weapon
            if ((spriteEffects & SpriteEffects.FlipHorizontally) != 0)
            {
                position.X -= this.player.CollisionBox.Width;
                origin.X -= this.playerOffset.X;
            }
            else
                origin.X += this.playerOffset.X;

            if ((spriteEffects & SpriteEffects.FlipVertically) != 0)
                origin.Y -= this.playerOffset.Y;
            else
                origin.Y += this.playerOffset.Y;

            spriteBatch.Draw(this.weaponAnim.Texture,
                             position - positionOffset,
                             this.weaponAnim.SourceRectangle,
                             Color.White,
                             0.0F,
                             origin,
                             1.0F,
                             spriteEffects,
                             1.0F);
        }

    #endregion

    #region Reset methods

    /// <summary>
    /// Stores the weapon's current ammunition count for later retrival.
    /// </summary>
    public void StoreAmmoCount()
    {
            this.ammoMemory = this.ammoCount;
        }

    /// <summary>
    /// Sets the weapon's ammunition count to the previously stored value.
    /// </summary>
    public void ApplyStoredAmmoCount()
    {
            this.ammoCount = this.ammoMemory;
        }

    #endregion
}