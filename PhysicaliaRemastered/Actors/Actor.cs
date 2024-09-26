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

    private SpriteEffects _verticalFlip;
    private SpriteEffects _horizontalFlip;

    public SpriteEffects SpriteFlip => _horizontalFlip | _verticalFlip;

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

    public bool CanCollide { get; set; } = true;

    public bool CanTakeDamage { get; set; } = true;

    public int Width => CurrentAnimation.CurrentFrame.SourceRectangle.Width;
    public int Height => CurrentAnimation.CurrentFrame.SourceRectangle.Height;

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

            SpriteEffects flip = _velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            _horizontalFlip = flip;
        }
    }

    public Vector2 Acceleration
    {
        get => _acceleration;
        set
        {
            _acceleration = value;
            _verticalFlip = _acceleration.Y < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
        }
    }

    public abstract void TakeDamage(float damageLevel);

    public virtual void OnCollision(ICollidable collidedObject, List<BoxSide> collidedSides, Vector2 position,
        Vector2 velocity)
    {
        if (collidedObject is not Tile)
        {
            return;
        }

        Position = position;
        _velocity = velocity;
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
        var actorCollisionBox = new Rectangle(
            (Position - Origin).ToPoint(),
            CollisionBox.Size
        );
        return rectangle.Contains(actorCollisionBox);
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

#if DEBUG
        // Collision box
        var collisionBoxTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
        collisionBoxTexture.SetData([Color.Red]);

        spriteBatch.Draw(
            collisionBoxTexture,
            this.GetAbsoluteCollisionBox().Location.ToVector2(),
            new Rectangle(Point.Zero, _collisionBox.Size),
            Color.White
        );
#endif
        
        var frameCenter = new Vector2(
            14,
            24
        );
        spriteBatch.Draw(
            CurrentAnimation.CurrentFrame.Texture,
            Position - viewportPosition,
            CurrentAnimation.CurrentFrame.SourceRectangle,
            Color.White,
            0F,
            frameCenter, // TODO: Origin should be derived from the current animation frame
            1.0F,
            SpriteFlip,
            0.8F
        );
        
    }
}