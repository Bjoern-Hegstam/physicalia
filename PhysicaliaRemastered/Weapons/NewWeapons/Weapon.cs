using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PhysicaliaRemastered.Actors;

namespace PhysicaliaRemastered.Weapons.NewWeapons
{
    /// <summary>
    /// Abstract class providing the basic characteristics of a weapon. The class
    /// provides properties needed for managing over the weapon and methods for
    /// operating it.
    /// </summary>
    public abstract class Weapon
    {
        #region Fields

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

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value denoting the weapon's type.
        /// </summary>
        public int WeaponID
        {
            get { return this.weaponID; }
            set { this.weaponID = value; }
        }

        /// <summary>
        /// Gets or sets a reference to the player who owns the weapon.
        /// </summary>
        public Player Player
        {
            get { return this.player; }
            set { this.player = value; }
        }

        /// <summary>
        /// Gets or sets the particleengine uesd by the weapon.
        /// </summary>
        public IParticleEngine ParticleEngine
        {
            get { return this.particleEngine; }
            set { this.particleEngine = value; }
        }

        /// <summary>
        /// Gets or sets the position of the player's origin relative to the
        /// weapon's origin.
        /// </summary>
        public Vector2 PlayerOffset
        {
            get { return this.playerOffset; }
            set { this.playerOffset = value; }
        }

        /// <summary>
        /// Gets or sets the id of the particle that the weapon can fire types of.
        /// </summary>
        public int ParticleID
        {
            get { return this.particleID; }
            set { this.particleID = value; }
        }

        /// <summary>
        /// Gets or sets the Sprite that should be used for drawing the weapon when
        /// it's not used by the player.
        /// </summary>
        public Sprite WeaponSprite
        {
            get { return this.sprite; }
            set { this.sprite = value; }
        }

        public Animation WarmupAnimation
        {
            get { return this.warmupAnimation; }
            set { this.warmupAnimation = value; }
        }

        /// <summary>
        /// Gets or sets the animation used by the weapon when it gets to
        /// draw itself.
        /// </summary>
        public Animation WeaponFireAnimation
        {
            get { return this.fireAnimation; }
            set { this.fireAnimation = value; }
        }

        /// <summary>
        /// Gets a value denoting whether the weapon is currently firing. The property
        /// can also be set by inheriting classes.
        /// </summary>
        public bool IsFiring
        {
            get { return this.firing; }
            protected set { this.firing = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the weapon was fired during the last
        /// call to its Update method. Can be set by inheriting classes.
        /// </summary>
        public bool WeaponFired
        {
            get { return this.firedOnUpdate; }
            protected set { this.firedOnUpdate = value; }
        }

        #region Ammunition properties

        /// <summary>
        /// Gets or sets the weapon's current amount of ammunition.
        /// </summary>
        public int AmmoCount
        {
            get { return this.ammoCount; }
            set
            {
                this.ammoCount = Math.Min(value, this.maxAmmo);
            }
        }

        /// <summary>
        /// Gets or sets the maximum amount of ammunition that the weapons can have.
        /// </summary>
        public int MaxAmmo
        {
            get { return this.maxAmmo; }
            set { this.maxAmmo = value; }
        }

        /// <summary>
        /// Gets or sets a value denoting whether the weapon has an infinite supply
        /// of ammunition.
        /// </summary>
        public bool InfiniteAmmo
        {
            get { return this.infiniteAmmo; }
            set { this.infiniteAmmo = value; }
        }

        /// <summary>
        /// Gets or sets the ammunition count previously stored in the weapon.
        /// </summary>
        public int AmmoMemory
        {
            get { return this.ammoMemory; }
            set { this.ammoMemory = value; }
        }

        #endregion

        #region Collision properties

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

        #region Time properties

        public float WeaponWarmUp
        {
            get { return this.weaponWarmupTime; }
            set { this.weaponWarmupTime = value; }
        }

        public float ShotsPerSecond
        {
            get { return this.shotsPerSecond; }
            set { this.shotsPerSecond = value; }
        }

        #endregion

        #endregion

        #region Constructors

        public Weapon(int weaponID, IParticleEngine particleEngine)
        {
            this.weaponID = weaponID;

            this.player = null;
            this.playerOffset = Vector2.Zero;
            this.particleEngine = particleEngine;

            this.sprite = new Sprite();
            this.fireAnimation = null;

            this.ammoMemory = this.maxAmmo = this.ammoCount = 0;
            this.infiniteAmmo = false;

            this.firing = false;
            this.firedOnUpdate = false;

            this.weaponWarmupTime = this.timeTillWeaponStart = 5F;
            this.shotsPerSecond = this.timeTillShot = 0F;

            this.canCollide = false;
            this.collisionBox = Rectangle.Empty;
            this.collisionDamage = 0F;
        }

        #endregion

        #region Abstract methods

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

        #endregion

        #region Virtual methods

        public virtual void Start()
        {
            if (this.ammoCount > 0 || this.infiniteAmmo)
            {
                // Prepare weapon
                this.timeTillWeaponStart = this.weaponWarmupTime;
                this.timeTillShot = 0;

                // Start animation and set the weapon's status to firing
                this.firing = true;

                // Start playing warmup animation if needed otherwise start 
                if (this.weaponWarmupTime > 0)
                    this.warmupAnimation.Play();
                else
                {
                    this.fireAnimation.Play();
                    this.OnStartFire();
                }
            }
        }
        
        public virtual void Stop()
        {
            if (this.firing)
            {
                // Leave the weapon in the same state as when we started
                this.timeTillWeaponStart = this.weaponWarmupTime;
                this.timeTillShot = 0;

                // Stop any vibration
                GamePad.SetVibration(PlayerIndex.One, 0, 0);

                // Stop the warm up animation ot be on the safe side
                this.warmupAnimation.Stop();

                this.firing = false;
                this.fireAnimation.Stop();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Stores the current amount of ammunition.
        /// </summary>
        public void StoreAmmoCount()
        {
            this.ammoMemory = this.ammoCount;
        }

        /// <summary>
        /// Restores the ammunition amount to the previous stored one.
        /// </summary>
        public void ApplyStoredAmmoCount()
        {
            this.ammoCount = this.ammoMemory;
        }

        /// <summary>
        /// Creates a copy of the current Weapon.
        /// </summary>
        /// <returns>A copy of the current Weapon.</returns>
        public Weapon Copy()
        {
            Weapon weapon = (Weapon)this.MemberwiseClone();

            return weapon;
        }

        /// <summary>
        /// Updates the weapon.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            // Start by assuming that the weapon hasn't been fired
            this.firedOnUpdate = false;

            // See if the weapon is firing
            if (this.firing)
            {
                // See if the weapon is warming up
                if (this.timeTillWeaponStart > 0)
                {
                    this.timeTillWeaponStart -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Go from warm up to firing if it's time
                    if (this.timeTillWeaponStart <= 0)
                    {
                        this.warmupAnimation.Stop();
                        this.fireAnimation.Play();

                        this.OnStartFire();
                    }
                }
                else if (this.timeTillShot > 0)
                {
                    this.timeTillShot -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    // Fire the weapon
                    this.FireWeapon();

                    // Let others know that the weapon was fired
                    this.firedOnUpdate = true;

                    // Prepare for the next round if we're still firing
                    // and have the ammunition needed
                    if (this.firing && (this.ammoCount > 0 || this.infiniteAmmo))
                    {
                        // Prepare weapon
                        this.timeTillShot += 1 / this.shotsPerSecond;
                    }
                    else
                    {
                        // Weapon can no longer be fired and must therefore be stopped
                        this.Stop();
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
            Vector2 origin = this.player.Origin + new Vector2(this.player.CollisionBox.X, this.player.CollisionBox.Y);

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

            Animation weaponAnim = null;
            
            if (this.timeTillWeaponStart > 0)
                weaponAnim = this.warmupAnimation;
            else
                weaponAnim = this.fireAnimation;

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

        #endregion
    }
}
