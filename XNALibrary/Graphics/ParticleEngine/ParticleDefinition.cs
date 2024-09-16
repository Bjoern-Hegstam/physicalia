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
    private const float DEFAULT_LIFE_TIME = 5F;
    private const float DEFAULT_VELOCITY_SCALE = 1F;
    private const float DEFAULT_RADIUS = 10F;

    // Movement
    private float velocityScale;
    private Vector2 acceleration;
    private float startAngle;

    // Collision
    private CollisionMode collisionMode;
    private float radius;

    // Life
    private float lifeTime;
    private ParticleLifeMode lifeMode;

    // Id
    private int id;

    /// <summary>
    /// Scale of the Particle's initial velocity.
    /// </summary>
    public float VelocityScale
    {
        get { return velocityScale; }
        set { velocityScale = value; }
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

    /// <summary>
    /// The angle at which the Particle is ejected, measured in radians.
    /// </summary>
    public float StartAngle
    {
        get { return startAngle; }
        set { startAngle = value; }
    }

    /// <summary>
    /// Total length of the particle's life, measured in seconds.
    /// </summary>
    public float LifeTime
    {
        get { return lifeTime; }
        set { lifeTime = value; }
    }

    public ParticleLifeMode LifeMode
    {
        get { return lifeMode; }
        set { lifeMode = value; }
    }

    public int Id
    {
        get { return id; }
    }

    public ParticleDefinition(int id)
    {
            this.id = id;
            velocityScale = DEFAULT_VELOCITY_SCALE;
            acceleration = Vector2.Zero;
            lifeTime = DEFAULT_LIFE_TIME;
            lifeMode = ParticleLifeMode.Time;

            // By default a particle acts as a circle
            collisionMode = CollisionMode.Circle;
            radius = DEFAULT_RADIUS;
        }

    /// <summary>
    /// Creates a new Particle according to the definition.
    /// </summary>
    /// <returns>A Particle with its basic values set to those in
    /// the definition.</returns>
    public Particle Create()
    {
            return Create(startAngle);
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
            particle.Acceleration = acceleration;

            float velocityX = angle != 0 ? (float)Math.Cos(angle) : 1F;
            float velocityY = angle != 0 ? -(float)Math.Sin(angle) : 0F;

            particle.Velocity = new Vector2(velocityX, velocityY) * velocityScale;

            // Life
            particle.Life = lifeTime;
            particle.LifeMode = lifeMode;

            // Collision settings
            particle.CollisionMode = collisionMode;
            particle.Radius = radius;
        }

    public void LoadXml(XmlReader reader)
    {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Life")
                {
                    ParticleLifeMode mode = (ParticleLifeMode)Enum.Parse(typeof(ParticleLifeMode), reader.GetAttribute("mode"));
                    float value = float.Parse(reader.GetAttribute("value"));

                    lifeMode = mode;
                    lifeTime = value;
                }

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Movement")
                {
                    reader.ReadToFollowing("VelocityScale");
                    velocityScale = int.Parse(reader.ReadElementContentAsString());

                    int x = int.Parse(reader.GetAttribute("x")); int y = int.Parse(reader.GetAttribute("y"));
                    acceleration = new Vector2(x, y);
                }

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Collision")
                {
                    CollisionMode mode = (CollisionMode)Enum.Parse(typeof(CollisionMode), reader.GetAttribute("mode"));

                    collisionMode = mode;
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