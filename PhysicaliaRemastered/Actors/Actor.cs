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
public abstract class Actor : ICollidable
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

    public SpriteEffects SpriteFlip => _horizontalFlip | _verticalFlip;

    private float _health;

    public virtual float Health
    {
        get => _health;
        set => _health = value;
    }

    private int _currentAnimType;

    private Rectangle _collisionBox;

    public abstract ObjectType Type { get; }

    public Rectangle CollisionBox
    {
        get => _collisionBox;
        set => _collisionBox = value;
    }

    public bool CanCollide { get; set; }

    public bool CanTakeDamage { get; set; }

    public int Width => CurrentAnimation.SourceRectangle.Width;
    public int Height => CurrentAnimation.SourceRectangle.Height;

    public virtual Vector2 Origin
    {
        get
        {
            var origin = new Vector2(_collisionBox.X, _collisionBox.Y);
            origin += new Vector2(_collisionBox.Width / 2f, _collisionBox.Height / 2f);

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
                SpriteEffects flip = value.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

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

    public virtual void OnCollision(ICollidable collidedObject, BoxSide collisionSides, Vector2 position,
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

    protected Actor()
    {
        Animations = new Dictionary<int, Animation>();
        _currentAnimType = -1;

        _velocity = Position = Vector2.Zero;
        _acceleration = Vector2.Zero;

        CanCollide = true;
        CanTakeDamage = true;
    }

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

    public void AddAnimation(int animType, Animation animation)
    {
        Animations.Add(animType, animation);
    }

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

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 viewportPosition)
    {
        if (!Animations.ContainsKey(_currentAnimType))
        {
            return;
        }

        spriteBatch.Draw(
            CurrentAnimation.Texture,
            Position - viewportPosition,
            CurrentAnimation.SourceRectangle,
            Color.White,
            0F,
            Origin,
            1.0F,
            SpriteFlip,
            0.8F
        );

#if DEBUG
        DrawCollisionBox(spriteBatch, viewportPosition);
#endif
    }

    private void DrawCollisionBox(SpriteBatch spriteBatch, Vector2 viewportPosition)
    {
        var collisionBoxLineTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
        collisionBoxLineTexture.SetData([Color.Red]);

        var collisionBoxLocation = (Position - viewportPosition + _collisionBox.Location.ToVector2()).ToPoint();

        const int outlineThickness = 1;

        spriteBatch.Draw(
            collisionBoxLineTexture,
            collisionBoxLocation.ToVector2(),
            new Rectangle(
                collisionBoxLocation.X,
                collisionBoxLocation.Y,
                _collisionBox.Width,
                outlineThickness
            ),
            Color.White,
            0F,
            Origin,
            1.0F,
            SpriteFlip,
            0.7F
        );

        spriteBatch.Draw(
            collisionBoxLineTexture,
            collisionBoxLocation.ToVector2() + new Vector2 { X = _collisionBox.Width },
            new Rectangle(
                collisionBoxLocation.X,
                collisionBoxLocation.Y,
                outlineThickness,
                _collisionBox.Height
            ),
            Color.White,
            0F,
            Origin,
            1.0F,
            SpriteFlip,
            0.7F
        );

        spriteBatch.Draw(
            collisionBoxLineTexture,
            collisionBoxLocation.ToVector2() + new Vector2() { Y = _collisionBox.Height },
            new Rectangle(
                collisionBoxLocation.X,
                collisionBoxLocation.Y,
                _collisionBox.Width,
                outlineThickness
            ),
            Color.White,
            0F,
            Origin,
            1.0F,
            SpriteFlip,
            0.7F
        );

        spriteBatch.Draw(
            collisionBoxLineTexture,
            collisionBoxLocation.ToVector2(),
            new Rectangle(
                collisionBoxLocation.X,
                collisionBoxLocation.Y,
                outlineThickness,
                _collisionBox.Height
            ),
            Color.White,
            0F,
            Origin,
            1.0F,
            SpriteFlip,
            0.7F
        );
    }
}