using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Actors;

namespace PhysicaliaRemastered.Weapons;

public class Weapon
{
    // Major objects
    private Player player;
    private IParticleEngine particleEngine;
    private IAnimationManager animationManager;

    public Player Player
    {
        get => player;
        set => player = value;
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
        get => playerOffset;
        set => playerOffset = value;
    }

    public Sprite WeaponSprite
    {
        get => weaponSprite;
        set => weaponSprite = value;
    }

    /// <summary>
    /// Gets the ID of this weapon type.
    /// </summary>
    public int TypeID
    {
        get => weaponID;
        set => weaponID = value;
    }

    public Vector2 MuzzlePosition
    {
        get => muzzlePosition;
        set => muzzlePosition = value;
    }

    public float MaxDeviation
    {
        get => maxDeviation;
        set => maxDeviation = value;
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
        get => projectileID;
        set => projectileID = value;
    }

    /// <summary>
    /// Gets or sets the number of shots fired per second.
    /// </summary>
    public float FireRate
    {
        get => fireRate;
        set => fireRate = value;
    }

    public float Spread
    {
        get => spread;
        set => spread = value;
    }

    public int ProjectilesPerShot
    {
        get => projectilesPerShot;
        set => projectilesPerShot = value;
    }

    public bool IsFiring => firing;

    public bool WeaponFired => weaponFired;

    public int AmmoCount
    {
        get => ammoCount;
        set => ammoCount = Math.Min(value, maxAmmo);
    }

    public int MaxAmmo
    {
        get => maxAmmo;
        set => maxAmmo = value;
    }

    public bool InfiniteAmmo
    {
        get => infiniteAmmo;
        set => infiniteAmmo = value;
    }

    public int AmmoMemory
    {
        get => ammoMemory;
        set => ammoMemory = value;
    }

    // CollisionChecking
    private bool canCollide;
    private Rectangle collisionBox;
    private float collisionDamage;

    public Rectangle CollisionBox
    {
        get => collisionBox;
        set => collisionBox = value;
    }

    public bool CanCollide
    {
        get => canCollide;
        set => canCollide = value;
    }

    public float CollisionDamage
    {
        get => collisionDamage;
        set => collisionDamage = value;
    }

    public Weapon(IParticleEngine particleEngine, IAnimationManager animationManager)
    {
            player = null;
            this.animationManager = animationManager;
            this.particleEngine = particleEngine;

            firing = false;
            spread = 0F;
            projectilesPerShot = 1;
            projectileID = -1;
            fireRate = 0F;
            maxAmmo = 0;
            ammoCount = 0;
            infiniteAmmo = false;
            weaponFired = false;

            weaponAnim = null;
            weaponSprite = new Sprite();

            canCollide = false;
            collisionBox = Rectangle.Empty;
            collisionDamage = 0F;
        }

    public void LoadXml(XmlReader reader, ISpriteLibrary spriteLibrary, IAnimationManager animationManager)
    {
            // Setup according to xml
            while (reader.Read())
            {
                // Graphics
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Graphics")
                {
                    ParseGraphicsData(reader, spriteLibrary);
                }

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "MuzzlePosition")
                {
                    int x = int.Parse(reader.GetAttribute("x")); int y = int.Parse(reader.GetAttribute("y"));
                    muzzlePosition = new Vector2(x, y);
                }

                // Fire data
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "FireData")
                {
                    reader.ReadToFollowing("Projectile");
                    projectileID = int.Parse(reader.ReadElementContentAsString());

                    fireRate = int.Parse(reader.ReadElementContentAsString());

                    ammoCount = int.Parse(reader.GetAttribute("count"));
                    maxAmmo = int.Parse(reader.GetAttribute("max"));

                    infiniteAmmo = (ammoCount == -1 ? true : false);
                }

                // Collision
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Collision")
                {
                    canCollide = bool.Parse(reader.GetAttribute("canCollide"));
                    collisionDamage = float.Parse(reader.GetAttribute("collisionDamage"));
                    
                    reader.ReadToFollowing("CollisionRectangle");
                    collisionBox = ReadRectangle(reader);
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
                    weaponSprite = spriteLibrary.GetSprite(spriteKey);
                }

                // Animation element
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Animation")
                {
                    int key = int.Parse(reader.GetAttribute("key"));
                    Animation anim = animationManager.GetBankAnimation(key).Copy();
                    animationManager.AddPlaybackAnimation(anim);

                    weaponAnim = anim;
                }

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "PlayerOffset")
                {
                    int x = int.Parse(reader.GetAttribute("x")); int y = int.Parse(reader.GetAttribute("y"));
                    playerOffset = new Vector2(x, y);
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

    public void Update(GameTime gameTime)
    {
            weaponFired = false;

            // Fire projectiles if needed
            if (firing &&
                (infiniteAmmo || ammoCount > 0))
            {

                timeTillShot -= (float)gameTime.ElapsedGameTime.TotalSeconds * fireRate;

                // Loop to make sure that all shots there are time for are fired
                while (timeTillShot <= 0)
                {
                    weaponFired = true;

                    // Only fire a projectile if the weapon has a valid projectile id
                    if (particleEngine.HasDefinition(projectileID))
                    {
                        // Get the position of the muzzle relative to the level
                        Vector2 muzzle = muzzlePosition;

                        if ((player.SpriteFlip & SpriteEffects.FlipHorizontally) != 0)
                            muzzle.X = weaponAnim.SourceRectangle.Width - muzzlePosition.X - player.CollisionBox.Width;

                        if ((player.SpriteFlip & SpriteEffects.FlipVertically) != 0)
                            muzzle.Y = weaponAnim.SourceRectangle.Height - muzzlePosition.Y;

                        Vector2 playerTopLeft = player.Position - player.Origin - new Vector2(player.CollisionBox.X, player.CollisionBox.Y);
                        muzzle = playerTopLeft - playerOffset + muzzle;

                        // Apply deviation to the projectile
                        muzzle.Y -= maxDeviation / 2;
                        muzzle.Y += maxDeviation * (float)Math.Sin(Settings.Random.NextDouble());

                        float angleSide = MathHelper.Pi * ((player.SpriteFlip & SpriteEffects.FlipHorizontally) != 0 ? 1 : 0);
                        float angleStep = spread / projectilesPerShot;
                        float angle = projectilesPerShot == 1 ? 0 : spread / 2;
                        angle += angleSide;

                        for (int i = 0; i < projectilesPerShot; i++)
                        {
                            particleEngine.Add(projectileID, 1, muzzle, angle);
                            angle -= angleStep;
                        }
                    }

                    timeTillShot += 1 / fireRate;

                    if (!infiniteAmmo)
                    {
                        ammoCount--;

                        if (ammoCount <= 0)
                        {
                            StopFiring();
                            break;
                        }
                    }
                }
            }
        }

    public void StartFiring()
    {
            // Start animation
            if (ammoCount > 0 || infiniteAmmo)
            {
                weaponAnim.Play();
                firing = true;
            }
        }

    public void StopFiring()
    {
            timeTillShot = 0;

            // Stop animation
            if (firing)
            {
                weaponAnim.Stop();
                firing = false;
            }
        }

    public Weapon Copy(IAnimationManager animationManager)
    {
            //Weapon weapon = new Weapon(this.particleEngine, this.animationManager);
            Weapon weapon = (Weapon)MemberwiseClone();

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