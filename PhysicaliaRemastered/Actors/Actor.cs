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

    public int Width => CurrentAnimation.Frame.Width;
    public int Height => CurrentAnimation.Frame.Height;

    public virtual Vector2 Origin
    {
        // TODO: The origin should not be dependent on the collision box.
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
        // Collision box
        spriteBatch.DrawRectangle(
            Position - viewportPosition + _collisionBox.Location.ToVector2(),
            _collisionBox,
            Color.Red,
            Origin,
            SpriteFlip
        );

        // Position
        var solidColorTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
        solidColorTexture.SetData([Color.Green]);
        spriteBatch.Draw(
            solidColorTexture,
            Position - viewportPosition - Origin,
            new Rectangle(-4, -4, 9, 9),
            Color.White
        );
        
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
#endif
    }
}