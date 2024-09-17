using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary;
using XNALibrary.Animation;
using XNALibrary.Collision;
using XNALibrary.TileEngine;

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
    public const float MaxVerticalVelocity = 600;

    private ActorStartValues _startValues;

    public ActorStartValues StartValues
    {
        get => _startValues;
        set => _startValues = value;
    }

    private Vector2 _velocity;
    private Vector2 _acceleration;

    private SpriteEffects _verticalFlip;
    private SpriteEffects _horizontalFlip;

    public SpriteEffects SpriteFlip => (_horizontalFlip | _verticalFlip);

    private float _health;

    public virtual float Health
    {
        get => _health;
        set => _health = value;
    }

    private int _currentAnimType;

    private Rectangle _collisionBox;

    public abstract ObjectType Type { get; }

    public Rectangle SourceRectangle => CurrentAnimation.SourceRectangle;

    public Texture2D Texture => CurrentAnimation.Texture;

    public Rectangle CollisionBox
    {
        get => _collisionBox;
        set => _collisionBox = value;
    }

    public bool CanCollide { get; set; }

    public bool CanTakeDamage { get; set; }

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
            var origin = new Vector2(_collisionBox.X, _collisionBox.Y);
            origin += new Vector2(_collisionBox.Width / 2, _collisionBox.Height / 2);

            return origin;
        }
    }

    public Vector2 Position { get; set; }

    public Vector2 Velocity
    {
        get => _velocity;
        set
        {
            // Do checks if the actor will be moving somewhere
            if (value.X != 0)
            {
                SpriteEffects flip;
                flip = value.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                if (flip != _horizontalFlip)
                {
                    // Same calculation no mather how we flip
                    _collisionBox.X = Width - _collisionBox.Width - _collisionBox.X;

                    // Only update flip if a change of direction has been made
                    _horizontalFlip = flip;
                }
            }

            // Update velocity
            _velocity = value;
        }
    }

    public Vector2 Acceleration
    {
        get => _acceleration;
        set
        {
            _verticalFlip = value.Y < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            if ((_acceleration.Y < 0 && value.Y > 0) ||
                (_acceleration.Y > 0 && value.Y < 0))
            {
                // Same calculation no mather how we flip
                _collisionBox.Y = Height - _collisionBox.Height - _collisionBox.Y;
            }

            _acceleration = value;
        }
    }

    public abstract void TakeDamage(float damageLevel);

    public virtual void OnCollision(ICollisionObject collidedObject, BoxSide collisionSides, Vector2 position,
        Vector2 velocity)
    {
        if (collidedObject.Type == ObjectType.Tile)
        {
            Position = position;
            _velocity = velocity;
        }
    }

    /// <summary>
    /// Gets the currently used Animation.
    /// </summary>
    public Animation CurrentAnimation => Animations.ContainsKey(_currentAnimType) ? Animations[_currentAnimType] : null;

    /// <summary>
    /// The key to the currently used animation key.
    /// </summary>
    public int CurrentAnimationType
    {
        get => _currentAnimType;
        set => SetAnimation(value);
    }

    public Dictionary<int, Animation> Animations { get; }

    public Actor()
    {
        Animations = new Dictionary<int, Animation>();
        _currentAnimType = -1;

        _velocity = Position = Vector2.Zero;
        _acceleration = Vector2.Zero;

        CanCollide = true;
        CanTakeDamage = true;
    }

    /// <summary>
    /// Updates the player's animation according to its current state. Base
    /// implementation provides support for the basic animation types.
    /// </summary>
    public virtual void UpdateAnimation()
    {
        if (Animations.ContainsKey((int)ActorAnimation.Jump) &&
            _velocity.Y / _acceleration.Y < 0)
        {
            if (_currentAnimType != (int)ActorAnimation.Jump)
            {
                CurrentAnimationType = (int)ActorAnimation.Jump;
            }

            return;
        }

        if (Animations.ContainsKey((int)ActorAnimation.Fall) &&
            _velocity.Y / _acceleration.Y > 0)
        {
            if (_currentAnimType != (int)ActorAnimation.Fall)
            {
                CurrentAnimationType = (int)ActorAnimation.Fall;
            }

            return;
        }

        if (Animations.ContainsKey((int)ActorAnimation.Walk) &&
            _velocity.X != 0)
        {
            if (_currentAnimType != (int)ActorAnimation.Walk)
            {
                CurrentAnimationType = (int)ActorAnimation.Walk;
            }

            return;
        }

        if (Animations.ContainsKey((int)ActorAnimation.Die) &&
            _health <= 0)
        {
            if (_currentAnimType != (int)ActorAnimation.Die)
            {
                CurrentAnimationType = (int)ActorAnimation.Die;
            }

            return;
        }

        if (Animations.ContainsKey((int)ActorAnimation.Rest) &&
            _velocity.X == 0)
        {
            if (_currentAnimType != (int)ActorAnimation.Rest)
            {
                CurrentAnimationType = (int)ActorAnimation.Rest;
            }
        }
    }

    /// <summary>
    /// Stops the currently playing animation and starts playing the
    /// new one.
    /// </summary>
    /// <param name="animationKey">Key to the new animation to play.</param>
    private void SetAnimation(int animationKey)
    {
        if (Animations.ContainsKey(_currentAnimType))
        {
            CurrentAnimation.Stop();
        }

        _currentAnimType = animationKey;

        if (Animations.ContainsKey(_currentAnimType))
        {
            CurrentAnimation.Play();
        }
    }

    /// <summary>
    /// Adds a key to a playback animation and associates it with the
    /// specified ActorAnimation value.
    /// </summary>
    /// <param name="animType">The type of animation the key goes to.</param>
    /// <param name="animKey">Key to the playback animation.</param>
    public void AddAnimation(ActorAnimation animType, Animation animation)
    {
        Animations.Add((int)animType, animation);
    }

    /// <summary>
    /// Adds a key to a playback animation and associates it with the
    /// specified integer value.
    /// </summary>
    /// <param name="animType">The type of animation the key goes to.</param>
    /// <param name="animKey">Key to the playback animation.</param>
    public void AddAnimation(int animType, Animation animation)
    {
        Animations.Add(animType, animation);
    }

    /// <summary>
    /// Removes the specified animation key from the Actor's collection
    /// of playback animation keys.
    /// </summary>
    /// <param name="animType">The type of animation to remove.</param>
    public void RemoveAnimation(ActorAnimation animType)
    {
        Animations.Remove((int)animType);
    }

    /// <summary>
    /// Removes the specified animation key from the Actor's collection
    /// of playback animation keys.
    /// </summary>
    /// <param name="animType">The type of animation to remove.</param>
    public void RemoveAnimation(int animType)
    {
        Animations.Remove(animType);
    }

    /// <summary>
    /// Applies the values specified in Actor.StartValues.
    /// </summary>
    public void ApplyStartValues()
    {
        Position = _startValues.Position;
        Velocity = _startValues.Velocity;
        Acceleration = _startValues.Acceleration;
    }

    public virtual void Update(GameTime gameTime)
    {
        _velocity += _acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
        // Cap off velocity in Y
        float quota = _velocity.Y / MaxVerticalVelocity;
        if (quota >= 1 || quota <= -1)
        {
            _velocity.Y = MaxVerticalVelocity * Math.Sign(_velocity.Y);
        }

        Position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        Draw(spriteBatch, Vector2.Zero);
    }

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
        // Only draw if a valid animation is set
        if (Animations.ContainsKey(_currentAnimType))
        {
            SpriteEffects test = SpriteFlip;
            spriteBatch.Draw(CurrentAnimation.Texture,
                Position - offsetPosition,
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