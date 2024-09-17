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
    private const float DefaultLife = 5F;

    // Movement
    protected Vector2 position;
    protected Vector2 velocity;
    protected Vector2 acceleration;

    // Collision
    private CollisionMode _collisionMode;
    private float _radius;

    // Life
    private ParticleLifeMode _lifeMode;
    private float _life;
    private bool _active;

    // Definition id
    private ParticleDefinition _definition;

    // Particle engine
    private IParticleEngine _particleEngine;

    public Vector2 Position
    {
        get => position;
        set => position = value;
    }

    public Vector2 Velocity
    {
        get => velocity;
        set => velocity = value;
    }

    public Vector2 Acceleration
    {
        get => acceleration;
        set => acceleration = value;
    }

    public CollisionMode CollisionMode
    {
        get => _collisionMode;
        set => _collisionMode = value;
    }

    public float Radius
    {
        get => _radius;
        set => _radius = value;
    }

    public ParticleLifeMode LifeMode
    {
        get => _lifeMode;
        set => _lifeMode = value;
    }

    public float Life
    {
        get => _life;
        set => _life = value;
    }

    public bool IsActive
    {
        get => _active;
        set => _active = value;
    }

    public ParticleDefinition Definition
    {
        get => _definition;
        set => _definition = value;
    }

    public IParticleEngine ParticleEngine
    {
        get => _particleEngine;
        set => _particleEngine = value;
    }

    public Particle()
        : this(Vector2.Zero)
    {
    }

    public Particle(Vector2 position)
    {
        this.position = position;
        acceleration = velocity = Vector2.Zero;
        _definition = null;
        _life = DefaultLife;
        _lifeMode = ParticleLifeMode.Time;
        _active = true;
    }

    public virtual void Update(GameTime gameTime)
    {
        // Update position
        position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        velocity += acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Update life
        if (_lifeMode == ParticleLifeMode.Time)
            _life -= (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Dead?
        if (_life <= 0)
            _active = false;
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

    private bool _canTakeDamage;

    public bool CanTakeDamage
    {
        get => _canTakeDamage;
        set => _canTakeDamage = value;
    }

    private bool _canCollide;

    public bool CanCollide
    {
        get => _canCollide;
        set => _canCollide = value;
    }

    public virtual void OnCollision(ICollisionObject collisionObject, BoxSide collisionSides, Vector2 position,
        Vector2 velocity)
    {
        if (_canCollide)
        {
            this.position = position;
            this.velocity = velocity;
        }
    }

    public void TakeDamage(float damage)
    {
        if (_lifeMode == ParticleLifeMode.Damage)
        {
            _life -= damage;
            if (_life <= 0)
                _active = false;
        }
    }
}