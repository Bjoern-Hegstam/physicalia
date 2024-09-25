using System.Diagnostics;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Animation;
using XNALibrary.Collision;
using XNALibrary.ParticleEngine.Particles;
using XNALibrary.Sprites;
using XNALibrary.TileEngine;

namespace XNALibrary.ParticleEngine;

public class ParticleEngine
{
    private readonly Dictionary<int, ParticleDefinition> _definitions = new();
    private readonly List<Particle> _particleBuffer = [];
    private readonly List<Particle> _activeParticles = [];

    public void LoadXml(string path, SpriteLibrary spriteLibrary, AnimationRunner animationRunner)
    {
        var readerSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true
        };

        using var reader = XmlReader.Create(path, readerSettings);
        LoadXml(reader, spriteLibrary, animationRunner);
    }

    public void LoadXml(XmlReader reader, SpriteLibrary spriteLibrary, AnimationRunner animationRunner)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "ParticleDefinition" })
            {
                int id = int.Parse(reader.GetAttribute("id") ?? throw new ResourceLoadException());
                string particleType = reader.GetAttribute("type") ?? throw new ResourceLoadException();
                ParticleDefinition particleDef;

                switch (particleType)
                {
                    case "SpriteParticle":
                    {
                        reader.ReadToFollowing("Sprite");
                        var particleSpriteId =
                            new SpriteId(reader.GetAttribute("id") ?? throw new ResourceLoadException());
                        Sprite sprite = spriteLibrary.GetSprite(particleSpriteId);

                        particleDef = new SpriteParticleDefinition(id, sprite);
                        break;
                    }
                    case "Projectile":
                    {
                        reader.ReadToFollowing("Sprite");
                        var projectileSpriteId =
                            new SpriteId(reader.GetAttribute("id") ?? throw new ResourceLoadException());
                        Sprite sprite = spriteLibrary.GetSprite(projectileSpriteId);

                        particleDef = new ProjectileDefinition(id, sprite);
                        break;
                    }
                    case "AnimationParticle":
                    {
                        reader.ReadToFollowing("Animation");
                        AnimationDefinitionId animationId =
                            new AnimationDefinitionId(reader.GetAttribute("key") ?? throw new ResourceLoadException());
                        Animation.Animation animation = animationRunner.AddPlaybackAnimation(animationId);

                        particleDef = new AnimationParticleDefinition(id, animation, animationRunner);
                        break;
                    }
                    default:
                        throw new ArgumentException("Invalid Particle type: " + particleType);
                }

                particleDef.LoadXml(reader);

                AddDefinition(particleDef);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "ParticleDefinitions" })
            {
                return;
            }
        }
    }

    public Particle[] Particles => _activeParticles.ToArray();

    public void AddDefinition(ParticleDefinition definition)
    {
        _definitions.Add(definition.Id, definition);
    }

    public void Add(int typeId, int count)
    {
        Add(typeId, count, Vector2.Zero, 0);
    }

    public void Add(int typeId, int count, Vector2 position)
    {
        Add(typeId, count, position, 0F);
    }

    public void Add(int typeId, int count, Vector2 position, float angle)
    {
        Debug.WriteLine(_activeParticles.Count + " : " + _particleBuffer.Count);

        // Is the key valid?
        if (!_definitions.ContainsKey(typeId))
        {
            throw new ArgumentException("Invalid type id: " + typeId);
        }

        Particle particle;

        for (int i = _particleBuffer.Count - 1; i >= 0 && count > 0; i--)
        {
            if (_particleBuffer[i].Definition.Id != typeId)
            {
                continue;
            }

            // Get particle from the reserve
            particle = _particleBuffer[i];
            _particleBuffer.RemoveAt(i);

            // Setup the particle
            _definitions[typeId].SetupParticle(particle, angle);

            particle.Position = position;
            particle.IsActive = true;

            // Add engine reference
            particle.ParticleEngine = this;

            // Add the particle to the active particles
            _activeParticles.Add(particle);

            // Decrease the count
            count--;
        }

        // Add more particles if needed
        while (count > 0)
        {
            particle = _definitions[typeId].Create(angle);
            particle.Position = position;

            // Add engine reference
            particle.ParticleEngine = this;

            _activeParticles.Add(particle);
            count--;
        }
    }

    public void CheckCollisions(IEnumerable<ICollidable> collObjects)
    {
        foreach (ICollidable collObj in collObjects)
        {
            if (collObj.CanCollide || collObj.CanTakeDamage)
            {
                CheckCollisions(collObj);
            }
        }
    }

    /// <summary>
    /// Checks for collisions between the ICollisionObject and the particles
    /// managed by the engine.
    /// </summary>
    /// <param name="collObject">Object to check for collisions against.</param>
    public void CheckCollisions(ICollidable collObject)
    {
        Rectangle objectCollisionBox = collObject.AbsoluteCollisionBox;

        IEnumerable<Particle> collidingParticles = from particle in _activeParticles
            where particle.CanCollide
            where objectCollisionBox.Intersects((particle as ICollidable).AbsoluteCollisionBox)
            select particle;

        foreach (Particle particle in collidingParticles)
        {
            particle.OnCollision(
                collObject,
                [BoxSide.Bottom, BoxSide.Left, BoxSide.Right, BoxSide.Top],
                Vector2.Zero,
                Vector2.Zero
            );
        }
    }

    /// <summary>
    /// Moves all active particles to the engines buffer of particles.
    /// </summary>
    public void ClearActive()
    {
        Particle[] particles = new Particle[_activeParticles.Count];
        _activeParticles.CopyTo(particles);
        _activeParticles.Clear();
        _particleBuffer.AddRange(particles);
    }

    /// <summary>
    /// Prepares the engine by having it use parts of its system to force a JIT of its code.
    /// </summary>
    public void Prepare()
    {
        // Fire a single particle to make the CLR JIT the ParticleEngine
        if (_definitions.Count > 0)
        {
            foreach (int key in _definitions.Keys)
            {
                Add(key, 1);
                break;
            }
        }

        // Clear out the created particle
        ClearActive();
    }

    public void Update(GameTime gameTime)
    {
        for (int i = _activeParticles.Count - 1; i >= 0; i--)
        {
            // Update the particle
            _activeParticles[i].Update(gameTime);

            // Has the particle gone inactive?
            if (_activeParticles[i].IsActive)
            {
                continue;
            }

            _particleBuffer.Add(_activeParticles[i]);
            _activeParticles.RemoveAt(i);
        }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
        foreach (Particle particle in _activeParticles)
        {
            particle.Draw(spriteBatch, offsetPosition);
        }
    }
}