using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PhysicaliaRemastered.Actors;

namespace PhysicaliaRemastered.Weapons.NewWeapons;

/// <summary>
/// Abstract class providing the basic characteristics of a weapon. The class
/// provides properties needed for managing over the weapon and methods for
/// operating it.
/// </summary>
public abstract class Weapon
{
    private int weaponID;

    private Player player;
    private IParticleEngine particleEngine;
    private int particleID;

    private Sprite sprite;
    private Animation warmupAnimation;
    private Animation fireAnimation;
    private Vector2 playerOffset;

    private bool firing;
    protected bool firedOnUpdate;

    private float weaponWarmupTime;
    private float timeTillWeaponStart;

    private float shotsPerSecond;
    private float timeTillShot;


    private int ammoCount;
    private int maxAmmo;
    private int ammoMemory;
    private bool infiniteAmmo;

    // Collision fields
    private bool canCollide;
    private Rectangle collisionBox;
    private float collisionDamage;

    /// <summary>
    /// Gets a value denoting the weapon's type.
    /// </summary>
    public int WeaponID
    {
        get => weaponID;
        set => weaponID = value;
    }

    /// <summary>
    /// Gets or sets a reference to the player who owns the weapon.
    /// </summary>
    public Player Player
    {
        get => player;
        set => player = value;
    }

    /// <summary>
    /// Gets or sets the particleengine uesd by the weapon.
    /// </summary>
    public IParticleEngine ParticleEngine
    {
        get => particleEngine;
        set => particleEngine = value;
    }

    /// <summary>
    /// Gets or sets the position of the player's origin relative to the
    /// weapon's origin.
    /// </summary>
    public Vector2 PlayerOffset
    {
        get => playerOffset;
        set => playerOffset = value;
    }

    /// <summary>
    /// Gets or sets the id of the particle that the weapon can fire types of.
    /// </summary>
    public int ParticleID
    {
        get => particleID;
        set => particleID = value;
    }

    /// <summary>
    /// Gets or sets the Sprite that should be used for drawing the weapon when
    /// it's not used by the player.
    /// </summary>
    public Sprite WeaponSprite
    {
        get => sprite;
        set => sprite = value;
    }

    public Animation WarmupAnimation
    {
        get => warmupAnimation;
        set => warmupAnimation = value;
    }

    /// <summary>
    /// Gets or sets the animation used by the weapon when it gets to
    /// draw itself.
    /// </summary>
    public Animation WeaponFireAnimation
    {
        get => fireAnimation;
        set => fireAnimation = value;
    }

    /// <summary>
    /// Gets a value denoting whether the weapon is currently firing. The property
    /// can also be set by inheriting classes.
    /// </summary>
    public bool IsFiring
    {
        get => firing;
        protected set => firing = value;
    }

    /// <summary>
    /// Gets a value indicating whether the weapon was fired during the last
    /// call to its Update method. Can be set by inheriting classes.
    /// </summary>
    public bool WeaponFired
    {
        get => firedOnUpdate;
        protected set => firedOnUpdate = value;
    }

    /// <summary>
    /// Gets or sets the weapon's current amount of ammunition.
    /// </summary>
    public int AmmoCount
    {
        get => ammoCount;
        set => ammoCount = Math.Min(value, maxAmmo);
    }

    /// <summary>
    /// Gets or sets the maximum amount of ammunition that the weapons can have.
    /// </summary>
    public int MaxAmmo
    {
        get => maxAmmo;
        set => maxAmmo = value;
    }

    /// <summary>
    /// Gets or sets a value denoting whether the weapon has an infinite supply
    /// of ammunition.
    /// </summary>
    public bool InfiniteAmmo
    {
        get => infiniteAmmo;
        set => infiniteAmmo = value;
    }

    /// <summary>
    /// Gets or sets the ammunition count previously stored in the weapon.
    /// </summary>
    public int AmmoMemory
    {
        get => ammoMemory;
        set => ammoMemory = value;
    }

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

    public float WeaponWarmUp
    {
        get => weaponWarmupTime;
        set => weaponWarmupTime = value;
    }

    public float ShotsPerSecond
    {
        get => shotsPerSecond;
        set => shotsPerSecond = value;
    }

    public Weapon(int weaponID, IParticleEngine particleEngine)
    {
        this.weaponID = weaponID;

        player = null;
        playerOffset = Vector2.Zero;
        this.particleEngine = particleEngine;

        sprite = new Sprite();
        fireAnimation = null;

        ammoMemory = maxAmmo = ammoCount = 0;
        infiniteAmmo = false;

        firing = false;
        firedOnUpdate = false;

        weaponWarmupTime = timeTillWeaponStart = 5F;
        shotsPerSecond = timeTillShot = 0F;

        canCollide = false;
        collisionBox = Rectangle.Empty;
        collisionDamage = 0F;
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
        if (ammoCount > 0 || infiniteAmmo)
        {
            // Prepare weapon
            timeTillWeaponStart = weaponWarmupTime;
            timeTillShot = 0;

            // Start animation and set the weapon's status to firing
            firing = true;

            // Start playing warmup animation if needed otherwise start 
            if (weaponWarmupTime > 0)
                warmupAnimation.Play();
            else
            {
                fireAnimation.Play();
                OnStartFire();
            }
        }
    }
        
    public virtual void Stop()
    {
        if (firing)
        {
            // Leave the weapon in the same state as when we started
            timeTillWeaponStart = weaponWarmupTime;
            timeTillShot = 0;

            // Stop any vibration
            GamePad.SetVibration(PlayerIndex.One, 0, 0);

            // Stop the warm up animation ot be on the safe side
            warmupAnimation.Stop();

            firing = false;
            fireAnimation.Stop();
        }
    }

    /// <summary>
    /// Stores the current amount of ammunition.
    /// </summary>
    public void StoreAmmoCount()
    {
        ammoMemory = ammoCount;
    }

    /// <summary>
    /// Restores the ammunition amount to the previous stored one.
    /// </summary>
    public void ApplyStoredAmmoCount()
    {
        ammoCount = ammoMemory;
    }

    /// <summary>
    /// Creates a copy of the current Weapon.
    /// </summary>
    /// <returns>A copy of the current Weapon.</returns>
    public Weapon Copy()
    {
        Weapon weapon = (Weapon)MemberwiseClone();

        return weapon;
    }

    /// <summary>
    /// Updates the weapon.
    /// </summary>
    /// <param name="gameTime"></param>
    public void Update(GameTime gameTime)
    {
        // Start by assuming that the weapon hasn't been fired
        firedOnUpdate = false;

        // See if the weapon is firing
        if (firing)
        {
            // See if the weapon is warming up
            if (timeTillWeaponStart > 0)
            {
                timeTillWeaponStart -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Go from warm up to firing if it's time
                if (timeTillWeaponStart <= 0)
                {
                    warmupAnimation.Stop();
                    fireAnimation.Play();

                    OnStartFire();
                }
            }
            else if (timeTillShot > 0)
            {
                timeTillShot -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                // Fire the weapon
                FireWeapon();

                // Let others know that the weapon was fired
                firedOnUpdate = true;

                // Prepare for the next round if we're still firing
                // and have the ammunition needed
                if (firing && (ammoCount > 0 || infiniteAmmo))
                {
                    // Prepare weapon
                    timeTillShot += 1 / shotsPerSecond;
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
        Vector2 origin = player.Origin + new Vector2(player.CollisionBox.X, player.CollisionBox.Y);

        // Worldposition of the weapon
        Vector2 position = player.Position;

        // If the player is flipped horizontally then we need to
        // subtract the width of its collisionbox to properly
        // place the weapon
        if ((spriteEffects & SpriteEffects.FlipHorizontally) != 0)
        {
            position.X -= player.CollisionBox.Width;
            origin.X -= playerOffset.X;
        }
        else
            origin.X += playerOffset.X;

        if ((spriteEffects & SpriteEffects.FlipVertically) != 0)
            origin.Y -= playerOffset.Y;
        else
            origin.Y += playerOffset.Y;

        Animation weaponAnim = null;
            
        if (timeTillWeaponStart > 0)
            weaponAnim = warmupAnimation;
        else
            weaponAnim = fireAnimation;

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