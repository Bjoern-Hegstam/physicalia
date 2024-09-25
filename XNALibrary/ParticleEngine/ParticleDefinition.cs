using System.Xml;
using Microsoft.Xna.Framework;

namespace XNALibrary.ParticleEngine;

public enum CollisionMode
{
    Rectangle,
    Circle
}

public abstract class ParticleDefinition(int id)
{
    private const float DefaultLifeTime = 5F;
    private const float DefaultVelocityScale = 1F;
    private const float DefaultRadius = 10F;

    /// <summary>
    /// Scale of the Particle's initial velocity.
    /// </summary>
    public float VelocityScale { get; set; } = DefaultVelocityScale;

    public Vector2 Acceleration { get; set; } = Vector2.Zero;

    public CollisionMode CollisionMode { get; set; } = CollisionMode.Circle;

    public float Radius { get; set; } = DefaultRadius;

    /// <summary>
    /// The angle at which the Particle is ejected, measured in radians.
    /// </summary>
    public float StartAngle { get; set; }

    /// <summary>
    /// Total length of the particle's life, measured in seconds.
    /// </summary>
    public float LifeTime { get; set; } = DefaultLifeTime;

    public ParticleLifeMode LifeMode { get; set; } = ParticleLifeMode.Time;

    public int Id { get; } = id;

    // By default a particle acts as a circle

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
        particle.Acceleration = Acceleration;

        float velocityX = angle != 0 ? (float)Math.Cos(angle) : 1F;
        float velocityY = angle != 0 ? -(float)Math.Sin(angle) : 0F;

        particle.Velocity = new Vector2(velocityX, velocityY) * VelocityScale;

        // Life
        particle.Life = LifeTime;
        particle.LifeMode = LifeMode;

        // Collision settings
        particle.CollisionMode = CollisionMode;
        particle.Radius = Radius;
    }

    public void LoadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Life" })
            {
                var mode =
                    Enum.Parse<ParticleLifeMode>(reader.GetAttribute("mode") ?? throw new ResourceLoadException());
                float value = float.Parse(reader.GetAttribute("value") ?? throw new ResourceLoadException());

                LifeMode = mode;
                LifeTime = value;
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Movement" })
            {
                reader.ReadToFollowing("VelocityScale");
                VelocityScale = int.Parse(reader.ReadElementContentAsString());

                int x = int.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
                int y = int.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());
                Acceleration = new Vector2(x, y);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Collision" })
            {
                var mode = Enum.Parse<CollisionMode>(reader.GetAttribute("mode") ?? throw new ResourceLoadException());

                CollisionMode = mode;
            }

            // Let derived classes process the input as well
            OnLoadXml(reader);

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "ParticleDefinition" })
            {
                return;
            }
        }
    }

    protected abstract void OnLoadXml(XmlReader reader);
}