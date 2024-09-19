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

    public void LoadXml(string path, SpriteLibrary spriteLibrary, AnimationManager animationManager)
    {
        var readerSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true
        };

        using var reader = XmlReader.Create(path, readerSettings);
        LoadXml(reader, spriteLibrary, animationManager);
    }

    public void LoadXml(XmlReader reader, SpriteLibrary spriteLibrary, AnimationManager animationManager)
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
                        SpriteId particleSpriteId =
                            new SpriteId(int.Parse(reader.GetAttribute("key") ?? throw new ResourceLoadException()));
                        Sprite sprite = spriteLibrary.GetSprite(particleSpriteId);

                        particleDef = new SpriteParticleDefinition(id, sprite);
                        break;
                    }
                    case "Projectile":
                    {
                        reader.ReadToFollowing("Sprite");
                        SpriteId projectileSpriteId =
                            new SpriteId(int.Parse(reader.GetAttribute("key") ?? throw new ResourceLoadException()));
                        Sprite sprite = spriteLibrary.GetSprite(projectileSpriteId);

                        particleDef = new ProjectileDefinition(id, sprite);
                        break;
                    }
                    case "AnimationParticle":
                    {
                        reader.ReadToFollowing("Animation");
                        int animationId = int.Parse(reader.GetAttribute("key") ?? throw new ResourceLoadException());
                        Animation.Animation animation = animationManager.AddPlaybackAnimation(animationId);

                        particleDef = new AnimationParticleDefinition(id, animation, animationManager);
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
            if (_particleBuffer[i].Definition.Id == typeId)
            {
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

    public void CheckCollisions(IEnumerable<ICollisionObject> collObjects)
    {
        foreach (ICollisionObject collObj in collObjects)
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
    public void CheckCollisions(ICollisionObject collObject)
    {
        Rectangle particleRect, collObjRect = collObject.CollisionBox;
        Vector2 boxPos = collObject.Position - collObject.Origin;
        collObjRect.X += (int)boxPos.X;
        collObjRect.Y += (int)boxPos.Y;

        for (var i = 0; i < _activeParticles.Count; i++)
        {
            Particle particle = _activeParticles[i];

            // Don't bother if the particle can't collide with anything
            if (!particle.CanCollide)
            {
                continue;
            }

            particleRect = particle.CollisionBox;
            boxPos = particle.Position - particle.Origin;
            particleRect.X += (int)boxPos.X;
            particleRect.Y += (int)boxPos.Y;

            if (collObjRect.Intersects(particleRect))
            {
                particle.OnCollision(collObject, BoxSide.Bottom | BoxSide.Left | BoxSide.Right | BoxSide.Top,
                    Vector2.Zero, Vector2.Zero);
            }
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