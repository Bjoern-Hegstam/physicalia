using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Services;
using XNALibrary.Graphics;

namespace XNALibrary.Graphics.Particles;

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
        get { return this.position; }
        set { this.position = value; }
    }

    public Vector2 Velocity
    {
        get { return this.velocity; }
        set { this.velocity = value; }
    }

    public Vector2 Acceleration
    {
        get { return this.acceleration; }
        set { this.acceleration = value; }
    }

    public CollisionMode CollisionMode
    {
        get { return this.collisionMode; }
        set { this.collisionMode = value; }
    }

    public float Radius
    {
        get { return this.radius; }
        set { this.radius = value; }
    }

    public ParticleLifeMode LifeMode
    {
        get { return this.lifeMode; }
        set { this.lifeMode = value; }
    }

    public float Life
    {
        get { return this.life; }
        set { this.life = value; }
    }

    public bool IsActive
    {
        get { return this.active; }
        set { this.active = value; }
    }

    public ParticleDefinition Definition
    {
        get { return this.definition; }
        set { this.definition = value; }
    }

    public IParticleEngine ParticleEngine
    {
        get { return this.particleEngine; }
        set { this.particleEngine = value; }
    }

    public Particle()
        : this(Vector2.Zero) { }

    public Particle(Vector2 position)
    {
        this.position = position;
        this.acceleration = this.velocity = Vector2.Zero;
        this.definition = null;
        this.life = DEFAULT_LIFE;
        this.lifeMode = ParticleLifeMode.Time;
        this.active = true;
    }

    public virtual void Update(GameTime gameTime)
    {
        // Update position
        this.position += this.velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        this.velocity += this.acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Update life
        if (this.lifeMode == ParticleLifeMode.Time)
            this.life -= (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Dead?
        if (this.life <= 0)
            this.active = false;
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
        get { return this.canTakeDamage; }
        set { this.canTakeDamage = value; }
    }

    private bool canCollide;
    public bool CanCollide
    {
        get { return this.canCollide; }
        set { this.canCollide = value; }
    }

    public virtual void OnCollision(ICollisionObject collisionObject, BoxSide collisionSides, Vector2 position, Vector2 velocity)
    {
        if (this.canCollide)
        {
            this.position = position;
            this.velocity = velocity;
        }
    }

    public void TakeDamage(float damage)
    {
        if (this.lifeMode == ParticleLifeMode.Damage)
        {
            this.life -= damage;
            if (this.life <= 0)
                this.active = false;
        }
    }
}