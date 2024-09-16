using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Graphics.TileEngine;
using XNALibrary.Interfaces;

namespace XNALibrary.Graphics.ParticleEngine;

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
    Damage,
    /// <summary>
    /// Particle is unaffected by both time and damage.
    /// </summary>
    None
}

public abstract class Particle : ICollisionObject
{
    private const float DEFAULT_LIFE = 5F;

    // Movement
    protected Vector2 position;
    protected Vector2 velocity;
    protected Vector2 acceleration;

    // Collision
    private CollisionMode collisionMode;
    private float radius;

    // Life
    private ParticleLifeMode lifeMode;
    private float life;
    private bool active;

    // Definition id
    private ParticleDefinition definition;

    // Particle engine
    private IParticleEngine particleEngine;

    public Vector2 Position
    {
        get { return position; }
        set { position = value; }
    }

    public Vector2 Velocity
    {
        get { return velocity; }
        set { velocity = value; }
    }

    public Vector2 Acceleration
    {
        get { return acceleration; }
        set { acceleration = value; }
    }

    public CollisionMode CollisionMode
    {
        get { return collisionMode; }
        set { collisionMode = value; }
    }

    public float Radius
    {
        get { return radius; }
        set { radius = value; }
    }

    public ParticleLifeMode LifeMode
    {
        get { return lifeMode; }
        set { lifeMode = value; }
    }

    public float Life
    {
        get { return life; }
        set { life = value; }
    }

    public bool IsActive
    {
        get { return active; }
        set { active = value; }
    }

    public ParticleDefinition Definition
    {
        get { return definition; }
        set { definition = value; }
    }

    public IParticleEngine ParticleEngine
    {
        get { return particleEngine; }
        set { particleEngine = value; }
    }

    public Particle()
        : this(Vector2.Zero) { }

    public Particle(Vector2 position)
    {
        this.position = position;
        acceleration = velocity = Vector2.Zero;
        definition = null;
        life = DEFAULT_LIFE;
        lifeMode = ParticleLifeMode.Time;
        active = true;
    }

    public virtual void Update(GameTime gameTime)
    {
        // Update position
        position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        velocity += acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Update life
        if (lifeMode == ParticleLifeMode.Time)
            life -= (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Dead?
        if (life <= 0)
            active = false;
    }

    public virtual void Draw(SpriteBatch spriteBatch) { }
    public virtual void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition) { }

    public abstract Vector2 Origin { get; }

    public abstract int Width { get; set; }
    public abstract int Height { get; set; }

    public abstract Rectangle SourceRectangle { get; }
    public abstract Texture2D Texture { get; }

    public abstract Rectangle CollisionBox { get; }

    public ObjectType Type
    {
        get { return ObjectType.Particle; }
    }

    private bool canTakeDamage;
    public bool CanTakeDamage
    {
        get { return canTakeDamage; }
        set { canTakeDamage = value; }
    }

    private bool canCollide;
    public bool CanCollide
    {
        get { return canCollide; }
        set { canCollide = value; }
    }

    public virtual void OnCollision(ICollisionObject collisionObject, BoxSide collisionSides, Vector2 position, Vector2 velocity)
    {
        if (canCollide)
        {
            this.position = position;
            this.velocity = velocity;
        }
    }

    public void TakeDamage(float damage)
    {
        if (lifeMode == ParticleLifeMode.Damage)
        {
            life -= damage;
            if (life <= 0)
                active = false;
        }
    }
}