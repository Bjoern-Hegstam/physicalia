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

public abstract class Particle(Vector2 position) : ICollidable
{
    private const float DefaultLife = 5F;

    public Vector2 Position { get; set; } = position;

    public Vector2 Velocity { get; set; } = Vector2.Zero;

    public Vector2 Acceleration { get; set; } = Vector2.Zero;

    public CollisionMode CollisionMode { get; set; }

    public float Radius { get; set; }

    public ParticleLifeMode LifeMode { get; set; } = ParticleLifeMode.Time;

    public float Life { get; set; } = DefaultLife;

    public bool IsActive { get; set; } = true;

    public ParticleDefinition? Definition { get; set; } = null;

    public ParticleEngine? ParticleEngine { get; set; }

    protected Particle()
        : this(Vector2.Zero)
    {
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

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
    }

    public abstract Vector2 Origin { get; }

    public abstract int Width { get; }
    public abstract int Height { get; }

    public abstract Rectangle CollisionBox { get; }

    public bool CanTakeDamage { get; set; }

    public bool CanCollide { get; set; }

    public virtual void OnCollision(ICollidable collidable, List<BoxSide> collidedSides, Vector2 suggestedNewPosition,
        Vector2 suggestedNewVelocity)
    {
        if (!CanCollide)
        {
            return;
        }

        Position = suggestedNewPosition;
        Velocity = suggestedNewVelocity;
    }

    public void TakeDamage(float damage)
    {
        if (LifeMode != ParticleLifeMode.Damage)
        {
            return;
        }

        Life -= damage;
        if (Life <= 0)
        {
            IsActive = false;
        }
    }
}