using System.Diagnostics;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Graphics.ParticleEngine.Particles;
using XNALibrary.Graphics.Sprites;
using XNALibrary.Graphics.TileEngine;
using XNALibrary.Interfaces;

namespace XNALibrary.Graphics.ParticleEngine;

public class ParticleEngine : IParticleEngine
{
    // Particle management
    private Dictionary<int, ParticleDefinition> definitions;
    private List<Particle> particleBuffer;
    private List<Particle> activeParticles;

    /// <summary>
    /// Creates a new ParticleEngine.
    /// </summary>
    public ParticleEngine()
    {
        definitions = new Dictionary<int, ParticleDefinition>();
        particleBuffer = new List<Particle>();
        activeParticles = new List<Particle>();
    }

    public void LoadXml(string path, ISpriteLibrary spriteLibrary, IAnimationManager animationManager)
    {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.IgnoreComments = true;
        readerSettings.IgnoreWhitespace = true;
        readerSettings.IgnoreProcessingInstructions = true;

        using (XmlReader reader = XmlReader.Create(path, readerSettings))
        {
            LoadXml(reader, spriteLibrary, animationManager);
        }
    }

    public void LoadXml(XmlReader reader, ISpriteLibrary spriteLibrary, IAnimationManager animationManager)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "ParticleDefinition")
            {
                int id = int.Parse(reader.GetAttribute("id"));
                string particleType = reader.GetAttribute("type");
                ParticleDefinition particleDef = null;

                int spriteKey;
                Sprite sprite;

                // Get the right ParticleDefinition
                switch (particleType)
                {
                    case "SpriteParticle":
                        reader.ReadToFollowing("Sprite");
                        spriteKey = int.Parse(reader.GetAttribute("key"));
                        sprite = spriteLibrary.GetSprite(spriteKey);

                        particleDef = new SpriteParticleDefinition(id, sprite);
                        break;
                    case "Projectile":
                        reader.ReadToFollowing("Sprite");
                        spriteKey = int.Parse(reader.GetAttribute("key"));
                        sprite = spriteLibrary.GetSprite(spriteKey);

                        particleDef = new ProjectileDefinition(id, sprite);
                        break;
                    case "AnimationParticle":
                        reader.ReadToFollowing("Animation");
                        spriteKey = int.Parse(reader.GetAttribute("key"));
                        Animation.Animation animation = animationManager.AddPlaybackAnimation(spriteKey);

                        particleDef = new AnimationParticleDefinition(id, animation, animationManager);
                        break;
                    default:
                        throw new ArgumentException("Invalid Particle type: " + particleType);
                }

                // Have the definition setup itself
                particleDef.LoadXml(reader);

                // Store the definition
                AddDefinition(particleDef);
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "ParticleDefinitions")
                return;
        }
    }

    public Particle[] Particles
    {
        get { return activeParticles.ToArray(); }
    }

    public bool HasDefinition(int definitionID)
    {
        return definitions.ContainsKey(definitionID);
    }

    public void AddDefinition(ParticleDefinition definition)
    {
        definitions.Add(definition.Id, definition);
    }

    public void RemoveDefinition(int definitionId, bool rinseBuffer)
    {
        // Remove definition
        definitions.Remove(definitionId);

        // Rinse the buffers?
        if (rinseBuffer)
        {
            for (int i = 0; i < activeParticles.Count; i++)
                if (activeParticles[i].Definition.Id == definitionId)
                    activeParticles.RemoveAt(i);

            for (int i = 0; i < particleBuffer.Count; i++)
                if (particleBuffer[i].Definition.Id == definitionId)
                    particleBuffer.RemoveAt(i);
        }
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
        Debug.WriteLine(activeParticles.Count.ToString() + " : " + particleBuffer.Count.ToString());

        // Is the key valid?
        if (!definitions.ContainsKey(typeId))
            throw new ArgumentException("Invalid type id: " + typeId);

        Particle particle = null;

        for (int i = particleBuffer.Count - 1; i >= 0  && count > 0; i--)
        {
            if (particleBuffer[i].Definition.Id == typeId)
            {
                // Get particle from the reserve
                particle = particleBuffer[i];
                particleBuffer.RemoveAt(i);

                // Setup the particle
                definitions[typeId].SetupParticle(particle, angle);

                particle.Position = position;
                particle.IsActive = true;

                // Add engine reference
                particle.ParticleEngine = this;

                // Add the particle to the active particles
                activeParticles.Add(particle);

                // Decrease the count
                count--;
            }
        }

        // Add more particles if needed
        while (count > 0)
        {
            particle = definitions[typeId].Create(angle);
            particle.Position = position;

            // Add engine reference
            particle.ParticleEngine = this;

            activeParticles.Add(particle);
            count--;
        }
    }

    public void CheckCollisions(ICollisionObject[] collObjects)
    {
        foreach (ICollisionObject collObj in collObjects)
        {
            if (collObj.CanCollide || collObj.CanTakeDamage)
                CheckCollisions(collObj);
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

        for (int i = 0; i < activeParticles.Count; i++)
        {
            Particle particle = activeParticles[i];

            // Don't bother if the particle can't collide with anything
            if (!particle.CanCollide)
                continue;

            particleRect = particle.CollisionBox;
            boxPos = particle.Position - particle.Origin;
            particleRect.X += (int)boxPos.X;
            particleRect.Y += (int)boxPos.Y;

            if (collObjRect.Intersects(particleRect))
                particle.OnCollision(collObject, BoxSide.Bottom | BoxSide.Left | BoxSide.Right | BoxSide.Top, Vector2.Zero, Vector2.Zero);
        }
    }

    /// <summary>
    /// Moves all active particles to the engines buffer of particles.
    /// </summary>
    public void ClearActive()
    {
        Particle[] particles = new Particle[activeParticles.Count];
        activeParticles.CopyTo(particles);
        activeParticles.Clear();
        particleBuffer.AddRange(particles);
    }

    /// <summary>
    /// Clears the engines buffer of particles
    /// </summary>
    public void ClearBuffer()
    {
        particleBuffer.Clear();
    }

    /// <summary>
    /// Prepares the engine by having it use parts of its system of force
    /// a JIT of its code.
    /// </summary>
    public void Prepare()
    {
        // Fire a single particle to make the CLR JIT the ParticleEngine
        if (definitions.Count > 0)
        {
            foreach (int key in definitions.Keys)
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
        for (int i = activeParticles.Count - 1; i >= 0; i--)
        {
            // Update the particle
            activeParticles[i].Update(gameTime);

            // Has the particle gone inactive?
            if (!activeParticles[i].IsActive)
            {
                particleBuffer.Add(activeParticles[i]);
                activeParticles.RemoveAt(i);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Draw(spriteBatch, Vector2.Zero);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
        for (int i = 0; i < activeParticles.Count; i++)
        {
            activeParticles[i].Draw(spriteBatch, offsetPosition);
        }
    }
}