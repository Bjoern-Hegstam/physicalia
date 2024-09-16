using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhysicaliaRemastered.Actors;

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
    public const float MAX_VERTICAL_VELOCITY = 600;

    private ActorStartValues startValues;

    public ActorStartValues StartValues
    {
        get => startValues;
        set => startValues = value;
    }

    private Vector2 position;
    private Vector2 velocity;
    private Vector2 acceleration;

    private SpriteEffects verticalFlip;
    private SpriteEffects horizontalFlip;

    public SpriteEffects SpriteFlip => (horizontalFlip | verticalFlip);

    private float health;

    public virtual float Health
    {
        get => health;
        set => health = value;
    }

    private Dictionary<int, Animation> animations;
    private int currentAnimType;

    private Rectangle collisionBox;

    private bool canCollide;
    private bool canTakeDamage;

    public abstract ObjectType Type
    {
        get;
    }

    public Rectangle SourceRectangle => CurrentAnimation.SourceRectangle;

    public Texture2D Texture => CurrentAnimation.Texture;

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

    public bool CanTakeDamage
    {
        get => canTakeDamage;
        set => canTakeDamage = value;
    }

    public int Width
    {
        get => CurrentAnimation.SourceRectangle.Width;
        set { }
    }

    public int Height
    {
        get => CurrentAnimation.SourceRectangle.Height;
        set { }
    }

    public virtual Vector2 Origin
    {
        get
        {
                Vector2 origin = new Vector2(collisionBox.X, collisionBox.Y);
                origin += new Vector2(collisionBox.Width / 2, collisionBox.Height / 2);

                return origin;
            }
    }

    public Vector2 Position
    {
        get => position;
        set => position = value;
    }

    public Vector2 Velocity
    {
        get => velocity;
        set
        {
                // Do checks if the actor will be moving somewhere
                if (value.X != 0)
                {
                    SpriteEffects flip;
                    flip = value.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                    if (flip != horizontalFlip)
                    {
                        // Same calculation no mather how we flip
                        collisionBox.X = Width - collisionBox.Width - collisionBox.X;

                        // Only update flip if a change of direction has been made
                        horizontalFlip = flip;
                    }
                }

                // Update velocity
                velocity = value;
            }
    }

    public Vector2 Acceleration
    {
        get => acceleration;
        set
        {
                verticalFlip = value.Y < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;

                if ((acceleration.Y < 0 && value.Y > 0) ||
                    (acceleration.Y > 0 && value.Y < 0))
                {
                    // Same calculation no mather how we flip
                    collisionBox.Y = Height - collisionBox.Height - collisionBox.Y;
                }

                acceleration = value;
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

    /// <summary>
    /// Gets the currently used Animation.
    /// </summary>
    public Animation CurrentAnimation => animations.ContainsKey(currentAnimType) ? animations[currentAnimType] : null;

    /// <summary>
    /// The key to the currently used animation key.
    /// </summary>
    public int CurrentAnimationType
    {
        get => currentAnimType;
        set => SetAnimation(value);
    }

    public Dictionary<int, Animation> Animations => animations;

    public Actor()
    {
            animations = new Dictionary<int, Animation>();
            currentAnimType = -1;

            velocity = position = Vector2.Zero;
            acceleration = Vector2.Zero;

            canCollide = true;
            canTakeDamage = true;
        }

    /// <summary>
    /// Updates the player's animation according to its current state. Base
    /// implementation provides support for the basic animation types.
    /// </summary>
    public virtual void UpdateAnimation()
    {
            if (animations.ContainsKey((int)ActorAnimation.Jump) &&
                velocity.Y / acceleration.Y < 0)
            {
                if (currentAnimType != (int)ActorAnimation.Jump)
                    CurrentAnimationType = (int)ActorAnimation.Jump;

                return;
            }

            if (animations.ContainsKey((int)ActorAnimation.Fall) &&
                velocity.Y / acceleration.Y > 0)
            {
                if (currentAnimType != (int)ActorAnimation.Fall)
                    CurrentAnimationType = (int)ActorAnimation.Fall;

                return;
            }

            if (animations.ContainsKey((int)ActorAnimation.Walk) &&
                velocity.X != 0)
            {
                if (currentAnimType != (int)ActorAnimation.Walk)
                    CurrentAnimationType = (int)ActorAnimation.Walk;

                return;
            }

            if (animations.ContainsKey((int)ActorAnimation.Die) &&
                health <= 0)
            {
                if (currentAnimType != (int)ActorAnimation.Die)
                    CurrentAnimationType = (int)ActorAnimation.Die;

                return;
            }

            if (animations.ContainsKey((int)ActorAnimation.Rest) &&
                velocity.X == 0)
            {
                if (currentAnimType != (int)ActorAnimation.Rest)
                    CurrentAnimationType = (int)ActorAnimation.Rest;

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
            if (animations.ContainsKey(currentAnimType))
                CurrentAnimation.Stop();

            currentAnimType = animationKey;

            if (animations.ContainsKey(currentAnimType))
                CurrentAnimation.Play();
        }

    /// <summary>
    /// Adds a key to a playback animation and associates it with the
    /// specified ActorAnimation value.
    /// </summary>
    /// <param name="animType">The type of animation the key goes to.</param>
    /// <param name="animKey">Key to the playback animation.</param>
    public void AddAnimation(ActorAnimation animType, Animation animation)
    {
            animations.Add((int)animType, animation);
        }

    /// <summary>
    /// Adds a key to a playback animation and associates it with the
    /// specified integer value.
    /// </summary>
    /// <param name="animType">The type of animation the key goes to.</param>
    /// <param name="animKey">Key to the playback animation.</param>
    public void AddAnimation(int animType, Animation animation)
    {
            animations.Add(animType, animation);
        }

    /// <summary>
    /// Removes the specified animation key from the Actor's collection
    /// of playback animation keys.
    /// </summary>
    /// <param name="animType">The type of animation to remove.</param>
    public void RemoveAnimation(ActorAnimation animType)
    {
            animations.Remove((int)animType);
        }

    /// <summary>
    /// Removes the specified animation key from the Actor's collection
    /// of playback animation keys.
    /// </summary>
    /// <param name="animType">The type of animation to remove.</param>
    public void RemoveAnimation(int animType)
    {
            animations.Remove(animType);
        }

    /// <summary>
    /// Applies the values specified in Actor.StartValues.
    /// </summary>
    public void ApplyStartValues()
    {
            Position = startValues.Position;
            Velocity = startValues.Velocity;
            Acceleration = startValues.Acceleration;
        }

    public virtual void Update(GameTime gameTime)
    {
            velocity += acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Cap off velocity in Y
            float quota = velocity.Y / MAX_VERTICAL_VELOCITY;
            if (quota >= 1 || quota <= -1)
                velocity.Y = MAX_VERTICAL_VELOCITY * Math.Sign(velocity.Y);

            position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
            Draw(spriteBatch, Vector2.Zero);
        }

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
            // Only draw if a valid animation is set
            if (animations.ContainsKey(currentAnimType))
            {
                SpriteEffects test = SpriteFlip;
                spriteBatch.Draw(CurrentAnimation.Texture,
                                 position - offsetPosition,
                                 CurrentAnimation.SourceRectangle,
                                 Color.White,
                                 0F,
                                 Origin,
                                 1.0F,
                                 SpriteFlip,
                                 0.8F);
            }
        }
}