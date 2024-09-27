using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Animation;
using XNALibrary.Collision;
using XNALibrary.Graphics;
using XNALibrary.TileEngine;

namespace PhysicaliaRemastered.Actors;

public enum ActorState
{
    Standing,
    Walking,
    Jumping,
    Falling,
    Dying,
    Dead,
    Celebrating
}

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

    public bool IsFlippedVertically { get; protected set; }
    public bool IsFlippedHorizontally { get; protected set; }

    public SpriteEffects SpriteFlip => (IsFlippedVertically ? SpriteEffects.FlipVertically : SpriteEffects.None)
                                       | (IsFlippedHorizontally ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

    private float _health;

    public float Health
    {
        get => _health;
        set => _health = Math.Max(value, 0);
    }

    private ActorState _currentState = ActorState.Standing;

    private Rectangle _collisionBox;

    public Rectangle CollisionBox
    {
        get => _collisionBox;
        set => _collisionBox = value;
    }

    /*
     * The actor's collision box in world coordinates and adjusted for horizontal and/or vertical flipping
     */
    public Rectangle AbsoluteCollisionBox =>
        new(
            (int)Position.X + (IsFlippedHorizontally ? -CollisionBox.Width - CollisionBox.X : CollisionBox.X),
            (int)Position.Y + (IsFlippedVertically ? -CollisionBox.Height - CollisionBox.Y : CollisionBox.Y),
            CollisionBox.Width,
            CollisionBox.Height
        );

    public bool CanCollide { get; set; } = true;

    public bool CanTakeDamage { get; set; } = true;

    // TODO: The origin should not be dependent on the collision box.
    public virtual Vector2 Origin =>
        _collisionBox.Location.ToVector2() + new Vector2(_collisionBox.Width / 2f, _collisionBox.Height / 2f);

    public Vector2 Position { get; set; } = Vector2.Zero;

    public Vector2 Velocity
    {
        get => _velocity;
        set
        {
            _velocity = value;
            if (_velocity.X == 0)
            {
                return;
            }

            IsFlippedHorizontally = _velocity.X < 0;
        }
    }

    public Vector2 Acceleration
    {
        get => _acceleration;
        set
        {
            _acceleration = value;
            IsFlippedVertically = _acceleration.Y < 0;
        }
    }

    public abstract void TakeDamage(float damageLevel);

    public virtual void OnCollision(ICollidable collidedObject, List<BoxSide> collidedSides,
        Vector2 suggestedNewPosition,
        Vector2 suggestedNewVelocity)
    {
        if (collidedObject is not Tile)
        {
            return;
        }

        Position = suggestedNewPosition;
        _velocity = suggestedNewVelocity;
    }

    private readonly Dictionary<ActorState, Animation> _animations = new();

    public ActorState CurrentState
    {
        get => _currentState;
        set
        {
            if (_currentState == value)
            {
                return;
            }

            Debug.WriteLine($"{GetType().Name} changing state {_currentState} -> {value}");
            Animations.GetValueOrDefault(_currentState)?.Stop();
            _currentState = value;
            Animations.GetValueOrDefault(_currentState)?.Play();
        }
    }

    public Animation CurrentAnimation => Animations[CurrentState];

    public ReadOnlyDictionary<ActorState, Animation> Animations => _animations.AsReadOnly();

    public virtual void UpdateActorState()
    {
        if (_currentState is ActorState.Dead)
        {
            return;
        }

        if (_currentState == ActorState.Dying && !CurrentAnimation.IsActive)
        {
            CurrentState = ActorState.Dead;
        }
        else if (_health <= 0)
        {
            CurrentState = ActorState.Dying;
        }
        else if (_velocity.Y / _acceleration.Y < 0)
        {
            CurrentState = ActorState.Jumping;
        }
        else if (_velocity.Y / _acceleration.Y > 0)
        {
            CurrentState = ActorState.Falling;
        }
        else if (_velocity.X != 0)
        {
            CurrentState = ActorState.Walking;
        }
        else if (_velocity.X == 0)
        {
            CurrentState = ActorState.Standing;
        }
    }

    public void AddAnimation(ActorState animType, Animation animation)
    {
        _animations.Add(animType, animation);
    }

    public void ApplyStartValues()
    {
        Position = _startValues.Position;
        Velocity = _startValues.Velocity;
        Acceleration = _startValues.Acceleration;
    }

    public bool IsWithin(Rectangle rectangle)
    {
        return rectangle.Contains(AbsoluteCollisionBox);
    }

    public virtual void Update(GameTime gameTime)
    {
        if (_currentState is not ActorState.Dying)
        {
            _velocity += _acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Cap off velocity in Y
            float quota = _velocity.Y / MaxVerticalVelocity;
            if (quota is >= 1 or <= -1)
            {
                _velocity.Y = MaxVerticalVelocity * Math.Sign(_velocity.Y);
            }

            Position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        CurrentAnimation.Update(gameTime);
    }

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 viewportPosition)
    {
        if (!_animations.ContainsKey(_currentState))
        {
            return;
        }

        var frameOrigin = CurrentAnimation.CurrentFrame.Origin.ToVector2();
        if (IsFlippedHorizontally)
        {
            frameOrigin.X = CurrentAnimation.CurrentFrame.SourceRectangle.Width - frameOrigin.X;
        }

        if (IsFlippedVertically)
        {
            frameOrigin.Y = CurrentAnimation.CurrentFrame.SourceRectangle.Height - frameOrigin.Y;
        }

        spriteBatch.Draw(
            CurrentAnimation.CurrentFrame.Texture,
            Position - viewportPosition,
            CurrentAnimation.CurrentFrame.SourceRectangle,
            Color.White,
            0F,
            frameOrigin,
            1.0F,
            SpriteFlip,
            1F
        );

#if DEBUG
        spriteBatch.DrawRectangle(AbsoluteCollisionBox, Color.Red, viewportPosition);
#endif
    }
}