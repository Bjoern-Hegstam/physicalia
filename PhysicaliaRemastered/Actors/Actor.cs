using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Physicalia;

namespace PhysicaliaRemastered.Actors
{
    public enum ActorAnimation
    {
        Rest = 0,
        Walk = 1,
        Jump = 2,
        Fall = 3,
        Die = 4,
        Win = 5
    }

    /// <summary>
    /// Represents an active Actor that has a position and velocity as well as
    /// an animated Sprite.
    /// </summary>
    public abstract class Actor : ICollisionObject
    {
        #region Constants

        public const float MAX_VERTICAL_VELOCITY = 600;

        #endregion

        #region Fields and Properties

        private ActorStartValues startValues;

        public ActorStartValues StartValues
        {
            get { return this.startValues; }
            set { this.startValues = value; }
        }

        private Vector2 position;
        private Vector2 velocity;
        private Vector2 acceleration;

        private SpriteEffects verticalFlip;
        private SpriteEffects horizontalFlip;

        public SpriteEffects SpriteFlip
        {
            get { return (this.horizontalFlip | this.verticalFlip); }
        }

        private float health;

        public virtual float Health
        {
            get { return this.health; }
            set { this.health = value; }
        }

        private Dictionary<int, Animation> animations;
        private int currentAnimType;

        private Rectangle collisionBox;

        private bool canCollide;
        private bool canTakeDamage;

        #endregion

        #region ICollisionObject Members

        public abstract ObjectType Type
        {
            get;
        }

        public Rectangle SourceRectangle
        {
            get { return this.CurrentAnimation.SourceRectangle; }
        }

        public Texture2D Texture
        {
            get { return this.CurrentAnimation.Texture; }
        }

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

        public bool CanTakeDamage
        {
            get { return this.canTakeDamage; }
            set { this.canTakeDamage = value; }
        }

        public int Width
        {
            get { return this.CurrentAnimation.SourceRectangle.Width; }
            set { }
        }

        public int Height
        {
            get { return this.CurrentAnimation.SourceRectangle.Height; }
            set { }
        }

        public virtual Vector2 Origin
        {
            get
            {
                Vector2 origin = new Vector2(this.collisionBox.X, this.collisionBox.Y);
                origin += new Vector2(this.collisionBox.Width / 2, this.collisionBox.Height / 2);

                return origin;
            }
        }

        public Vector2 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        public Vector2 Velocity
        {
            get { return this.velocity; }
            set
            {
                // Do checks if the actor will be moving somewhere
                if (value.X != 0)
                {
                    SpriteEffects flip;
                    flip = value.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                    if (flip != this.horizontalFlip)
                    {
                        // Same calculation no mather how we flip
                        this.collisionBox.X = this.Width - this.collisionBox.Width - this.collisionBox.X;

                        // Only update flip if a change of direction has been made
                        this.horizontalFlip = flip;
                    }
                }

                // Update velocity
                this.velocity = value;
            }
        }

        public Vector2 Acceleration
        {
            get { return this.acceleration; }
            set
            {
                this.verticalFlip = value.Y < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;

                if ((this.acceleration.Y < 0 && value.Y > 0) ||
                    (this.acceleration.Y > 0 && value.Y < 0))
                {
                    // Same calculation no mather how we flip
                    this.collisionBox.Y = this.Height - this.collisionBox.Height - this.collisionBox.Y;
                }

                this.acceleration = value;
            }
        }

        public abstract void TakeDamage(float damageLevel);
        public virtual void OnCollision(ICollisionObject collidedObject, BoxSide collisionSides, Vector2 position, Vector2 velocity)
        {
            if (collidedObject.Type == ObjectType.Tile)
            {
                this.position = position;
                this.velocity = velocity;
            }
        }

        #endregion

        /// <summary>
        /// Gets the currently used Animation.
        /// </summary>
        public Animation CurrentAnimation
        {
            get { return this.animations.ContainsKey(this.currentAnimType) ? this.animations[this.currentAnimType] : null; }
        }

        /// <summary>
        /// The key to the currently used animation key.
        /// </summary>
        public int CurrentAnimationType
        {
            get { return this.currentAnimType; }
            set { this.SetAnimation(value); }
        }

        public Dictionary<int, Animation> Animations
        {
            get { return this.animations; }
        }

        public Actor()
        {
            this.animations = new Dictionary<int, Animation>();
            this.currentAnimType = -1;

            this.velocity = this.position = Vector2.Zero;
            this.acceleration = Vector2.Zero;

            this.canCollide = true;
            this.canTakeDamage = true;
        }

        #region Animation methods

        /// <summary>
        /// Updates the player's animation according to its current state. Base
        /// implementation provides support for the basic animation types.
        /// </summary>
        public virtual void UpdateAnimation()
        {
            if (this.animations.ContainsKey((int)ActorAnimation.Jump) &&
                this.velocity.Y / this.acceleration.Y < 0)
            {
                if (this.currentAnimType != (int)ActorAnimation.Jump)
                    this.CurrentAnimationType = (int)ActorAnimation.Jump;

                return;
            }

            if (this.animations.ContainsKey((int)ActorAnimation.Fall) &&
                this.velocity.Y / this.acceleration.Y > 0)
            {
                if (this.currentAnimType != (int)ActorAnimation.Fall)
                    this.CurrentAnimationType = (int)ActorAnimation.Fall;

                return;
            }

            if (this.animations.ContainsKey((int)ActorAnimation.Walk) &&
                this.velocity.X != 0)
            {
                if (this.currentAnimType != (int)ActorAnimation.Walk)
                    this.CurrentAnimationType = (int)ActorAnimation.Walk;

                return;
            }

            if (this.animations.ContainsKey((int)ActorAnimation.Die) &&
                this.health <= 0)
            {
                if (this.currentAnimType != (int)ActorAnimation.Die)
                    this.CurrentAnimationType = (int)ActorAnimation.Die;

                return;
            }

            if (this.animations.ContainsKey((int)ActorAnimation.Rest) &&
                this.velocity.X == 0)
            {
                if (this.currentAnimType != (int)ActorAnimation.Rest)
                    this.CurrentAnimationType = (int)ActorAnimation.Rest;

                return;
            }
        }

        /// <summary>
        /// Stops the currently playing animation and starts playing the
        /// new one.
        /// </summary>
        /// <param name="animationKey">Key to the new animation to play.</param>
        private void SetAnimation(int animationKey)
        {
            if (this.animations.ContainsKey(this.currentAnimType))
                this.CurrentAnimation.Stop();

            this.currentAnimType = animationKey;

            if (this.animations.ContainsKey(this.currentAnimType))
                this.CurrentAnimation.Play();
        }

        /// <summary>
        /// Adds a key to a playback animation and associates it with the
        /// specified ActorAnimation value.
        /// </summary>
        /// <param name="animType">The type of animation the key goes to.</param>
        /// <param name="animKey">Key to the playback animation.</param>
        public void AddAnimation(ActorAnimation animType, Animation animation)
        {
            this.animations.Add((int)animType, animation);
        }

        /// <summary>
        /// Adds a key to a playback animation and associates it with the
        /// specified integer value.
        /// </summary>
        /// <param name="animType">The type of animation the key goes to.</param>
        /// <param name="animKey">Key to the playback animation.</param>
        public void AddAnimation(int animType, Animation animation)
        {
            this.animations.Add(animType, animation);
        }

        /// <summary>
        /// Removes the specified animation key from the Actor's collection
        /// of playback animation keys.
        /// </summary>
        /// <param name="animType">The type of animation to remove.</param>
        public void RemoveAnimation(ActorAnimation animType)
        {
            this.animations.Remove((int)animType);
        }

        /// <summary>
        /// Removes the specified animation key from the Actor's collection
        /// of playback animation keys.
        /// </summary>
        /// <param name="animType">The type of animation to remove.</param>
        public void RemoveAnimation(int animType)
        {
            this.animations.Remove(animType);
        }

        #endregion

        /// <summary>
        /// Applies the values specified in Actor.StartValues.
        /// </summary>
        public void ApplyStartValues()
        {
            this.Position = this.startValues.Position;
            this.Velocity = this.startValues.Velocity;
            this.Acceleration = this.startValues.Acceleration;
        }

        public virtual void Update(GameTime gameTime)
        {
            this.velocity += this.acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Cap off velocity in Y
            float quota = this.velocity.Y / MAX_VERTICAL_VELOCITY;
            if (quota >= 1 || quota <= -1)
                this.velocity.Y = MAX_VERTICAL_VELOCITY * Math.Sign(this.velocity.Y);

            this.position += this.velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            this.Draw(spriteBatch, Vector2.Zero);
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
        {
            // Only draw if a valid animation is set
            if (this.animations.ContainsKey(this.currentAnimType))
            {
                SpriteEffects test = this.SpriteFlip;
                spriteBatch.Draw(this.CurrentAnimation.Texture,
                                 this.position - offsetPosition,
                                 this.CurrentAnimation.SourceRectangle,
                                 Color.White,
                                 0F,
                                 this.Origin,
                                 1.0F,
                                 this.SpriteFlip,
                                 0.8F);
            }
        }
    }
}
