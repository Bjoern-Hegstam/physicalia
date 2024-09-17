using System.Xml;
using Microsoft.Xna.Framework;

namespace XNALibrary.Graphics.ParticleEngine;

public enum CollisionMode
{
    Rectangle,
    Circle
}

public abstract class ParticleDefinition
{
    private const float DefaultLifeTime = 5F;
    private const float DefaultVelocityScale = 1F;
    private const float DefaultRadius = 10F;

    // Movement
    private float _velocityScale;
    private Vector2 _acceleration;
    private float _startAngle;

    // Collision
    private CollisionMode _collisionMode;
    private float _radius;

    // Life
    private float _lifeTime;
    private ParticleLifeMode _lifeMode;

    // Id
    private readonly int _id;

    /// <summary>
    /// Scale of the Particle's initial velocity.
    /// </summary>
    public float VelocityScale
    {
        get => _velocityScale;
        set => _velocityScale = value;
    }

    public Vector2 Acceleration
    {
        get => _acceleration;
        set => _acceleration = value;
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

    /// <summary>
    /// The angle at which the Particle is ejected, measured in radians.
    /// </summary>
    public float StartAngle
    {
        get => _startAngle;
        set => _startAngle = value;
    }

    /// <summary>
    /// Total length of the particle's life, measured in seconds.
    /// </summary>
    public float LifeTime
    {
        get => _lifeTime;
        set => _lifeTime = value;
    }

    public ParticleLifeMode LifeMode
    {
        get => _lifeMode;
        set => _lifeMode = value;
    }

    public int Id => _id;

    public ParticleDefinition(int id)
    {
        _id = id;
        _velocityScale = DefaultVelocityScale;
        _acceleration = Vector2.Zero;
        _lifeTime = DefaultLifeTime;
        _lifeMode = ParticleLifeMode.Time;

        // By default a particle acts as a circle
        _collisionMode = CollisionMode.Circle;
        _radius = DefaultRadius;
    }

    /// <summary>
    /// Creates a new Particle according to the definition.
    /// </summary>
    /// <returns>A Particle with its basic values set to those in
    /// the definition.</returns>
    public Particle Create()
    {
        return Create(_startAngle);
    }

    /// <summary>
    /// Creates a new Particle according to the definition.
    /// </summary>
    /// <param name="angle">The angle at which the particle is ejected.</param>
    /// <returns>A Particle with its basic values set to those in
    /// the definition.</returns>
    public abstract Particle Create(float angle);

    public virtual void SetupParticle(Particle particle, float angle)
    {
        // Id of definition
        particle.Definition = this;

        // Movement
        particle.Acceleration = _acceleration;

        float velocityX = angle != 0 ? (float)Math.Cos(angle) : 1F;
        float velocityY = angle != 0 ? -(float)Math.Sin(angle) : 0F;

        particle.Velocity = new Vector2(velocityX, velocityY) * _velocityScale;

        // Life
        particle.Life = _lifeTime;
        particle.LifeMode = _lifeMode;

        // Collision settings
        particle.CollisionMode = _collisionMode;
        particle.Radius = _radius;
    }

    public void LoadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Life")
            {
                ParticleLifeMode mode =
                    (ParticleLifeMode)Enum.Parse(typeof(ParticleLifeMode), reader.GetAttribute("mode"));
                float value = float.Parse(reader.GetAttribute("value"));

                _lifeMode = mode;
                _lifeTime = value;
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Movement")
            {
                reader.ReadToFollowing("VelocityScale");
                _velocityScale = int.Parse(reader.ReadElementContentAsString());

                int x = int.Parse(reader.GetAttribute("x"));
                int y = int.Parse(reader.GetAttribute("y"));
                _acceleration = new Vector2(x, y);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Collision")
            {
                CollisionMode mode = (CollisionMode)Enum.Parse(typeof(CollisionMode), reader.GetAttribute("mode"));

                _collisionMode = mode;
            }

            // Let derived classes process the input as well
            OnLoadXml(reader);

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "ParticleDefinition")
                return;
        }
    }

    protected abstract void OnLoadXml(XmlReader reader);
}