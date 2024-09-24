using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary;
using XNALibrary.Animation;
using XNALibrary.Collision;
using XNALibrary.Graphics;
using XNALibrary.TileEngine;

namespace PhysicaliaRemastered.Actors;

public enum ActorAnimationType
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

    private Vector2 _velocity = Vector2.Zero;
    private Vector2 _acceleration = Vector2.Zero;

    private SpriteEffects _verticalFlip;
    private SpriteEffects _horizontalFlip;

    public SpriteEffects SpriteFlip => _horizontalFlip | _verticalFlip;

    private float _health;

    public virtual float Health
    {
        get => _health;
        set => _health = value;
    }

    private ActorAnimationType _currentAnimationType = ActorAnimationType.Rest;

    private Rectangle _collisionBox;

    public abstract ObjectType Type { get; }

    public Rectangle CollisionBox
    {
        get => _collisionBox;
        set => _collisionBox = value;
    }

    public bool CanCollide { get; set; } = true;

    public bool CanTakeDamage { get; set; } = true;

    public int Width => CurrentAnimation.Frame.Width;
    public int Height => CurrentAnimation.Frame.Height;

    public virtual Vector2 Origin
    {
        get
        {
            var origin = new Vector2(_collisionBox.X, _collisionBox.Y);
            origin += new Vector2(_collisionBox.Width / 2f, _collisionBox.Height / 2f);

            return origin;
        }
    }

    public Vector2 Position { get; set; } = Vector2.Zero;

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

    public Dictionary<ActorAnimationType, Animation> Animations = new();

    public ActorAnimationType CurrentAnimationType
    {
        get => _currentAnimationType;
        set
        {
            Animations.GetValueOrDefault(_currentAnimationType)?.Stop();
            _currentAnimationType = value;
            Animations.GetValueOrDefault(_currentAnimationType)?.Play();
        }
    }

    public Animation CurrentAnimation => Animations[CurrentAnimationType];

    public virtual void UpdateAnimation()
    {
        if (Animations.ContainsKey(ActorAnimationType.Jump) && _velocity.Y / _acceleration.Y < 0)
        {
            if (_currentAnimationType != ActorAnimationType.Jump)
            {
                CurrentAnimationType = ActorAnimationType.Jump;
            }

            return;
        }

        if (Animations.ContainsKey(ActorAnimationType.Fall) && _velocity.Y / _acceleration.Y > 0)
        {
            if (_currentAnimationType != ActorAnimationType.Fall)
            {
                CurrentAnimationType = ActorAnimationType.Fall;
            }

            return;
        }

        if (Animations.ContainsKey(ActorAnimationType.Walk) && _velocity.X != 0)
        {
            if (_currentAnimationType != ActorAnimationType.Walk)
            {
                CurrentAnimationType = ActorAnimationType.Walk;
            }

            return;
        }

        if (Animations.ContainsKey(ActorAnimationType.Die) && _health <= 0)
        {
            if (_currentAnimationType != ActorAnimationType.Die)
            {
                CurrentAnimationType = ActorAnimationType.Die;
            }

            return;
        }

        if (Animations.ContainsKey(ActorAnimationType.Rest) && _velocity.X == 0)
        {
            if (_currentAnimationType != ActorAnimationType.Rest)
            {
                CurrentAnimationType = ActorAnimationType.Rest;
            }
        }
    }

    public void AddAnimation(ActorAnimationType animType, Animation animation)
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
        if (!Animations.ContainsKey(_currentAnimationType))
        {
            return;
        }

        spriteBatch.Draw(
            CurrentAnimation.AnimationDefinition.Texture,
            Position - viewportPosition,
            CurrentAnimation.Frame,
            Color.White,
            0F,
            Origin,
            1.0F,
            SpriteFlip,
            0.8F
        );

#if DEBUG
        // Origin
        spriteBatch.DrawRectangle(
            Position - viewportPosition + Origin - new Vector2 { X = 2 },
            new Rectangle(0, 0, 5, 1),
            Color.Purple,
            Origin,
            SpriteFlip
        );

        spriteBatch.DrawRectangle(
            Position - viewportPosition + Origin - new Vector2 { Y = 2 },
            new Rectangle(0, 0, 1, 5),
            Color.Purple,
            Origin,
            SpriteFlip
        );

        // Collision box
        spriteBatch.DrawRectangle(
            Position - viewportPosition + _collisionBox.Location.ToVector2(),
            _collisionBox,
            Color.Red,
            Origin,
            SpriteFlip
        );
#endif
    }
}