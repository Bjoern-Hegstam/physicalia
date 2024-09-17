using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Collision;
using XNALibrary.TileEngine;

namespace XNALibrary.ParticleEngine;

/// <summary>
/// Determines what can affect the life of the particle.
/// </summary>
public enum ParticleLifeMode
{
    /// <summary>
    /// Life decreases each update.
    /// </summary>
    Time,

    /// <summary>
    /// Life decreases upon collision.
    /// </summary>
    Damage
}

public abstract class Particle : ICollisionObject
{
    private const float DefaultLife = 5F;

    public Vector2 Position { get; set; }

    public Vector2 Velocity { get; set; }

    public Vector2 Acceleration { get; set; }

    public CollisionMode CollisionMode { get; set; }

    public float Radius { get; set; }

    public ParticleLifeMode LifeMode { get; set; }

    public float Life { get; set; }

    public bool IsActive { get; set; }

    public ParticleDefinition? Definition { get; set; }

    public IParticleEngine ParticleEngine { get; set; }

    public Particle()
        : this(Vector2.Zero)
    {
    }

    public Particle(Vector2 position)
    {
        Position = position;
        Acceleration = Velocity = Vector2.Zero;
        Definition = null;
        Life = DefaultLife;
        LifeMode = ParticleLifeMode.Time;
        IsActive = true;
    }

    public virtual void Update(GameTime gameTime)
    {
        // Update position
        Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        Velocity += Acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Update life
        if (LifeMode == ParticleLifeMode.Time)
        {
            Life -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        // Dead?
        if (Life <= 0)
        {
            IsActive = false;
        }
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
    }

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
    }

    public abstract Vector2 Origin { get; }

    public abstract int Width { get; set; }
    public abstract int Height { get; set; }

    public abstract Rectangle SourceRectangle { get; }
    public abstract Texture2D Texture { get; }

    public abstract Rectangle CollisionBox { get; }

    public ObjectType Type => ObjectType.Particle;

    public bool CanTakeDamage { get; set; }

    public bool CanCollide { get; set; }

    public virtual void OnCollision(ICollisionObject collisionObject, BoxSide collisionSides, Vector2 position,
        Vector2 velocity)
    {
        if (CanCollide)
        {
            Position = position;
            Velocity = velocity;
        }
    }

    public void TakeDamage(float damage)
    {
        if (LifeMode == ParticleLifeMode.Damage)
        {
            Life -= damage;
            if (Life <= 0)
            {
                IsActive = false;
            }
        }
    }
}