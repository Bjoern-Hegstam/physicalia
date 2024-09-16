using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Services;
using XNALibrary.Graphics;
using System.Xml;

namespace XNALibrary.Graphic.Particles;

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
        this.definitions = new Dictionary<int, ParticleDefinition>();
        this.particleBuffer = new List<Particle>();
        this.activeParticles = new List<Particle>();
    }

    public void LoadXml(string path, ISpriteLibrary spriteLibrary, IAnimationManager animationManager)
    {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.IgnoreComments = true;
        readerSettings.IgnoreWhitespace = true;
        readerSettings.IgnoreProcessingInstructions = true;

        using (XmlReader reader = XmlReader.Create(path, readerSettings))
        {
            this.LoadXml(reader, spriteLibrary, animationManager);
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
                        Animation animation = animationManager.AddPlaybackAnimation(spriteKey);

                        particleDef = new AnimationParticleDefinition(id, animation, animationManager);
                        break;
                    default:
                        throw new ArgumentException("Invalid Particle type: " + particleType);
                }

                // Have the definition setup itself
                particleDef.LoadXml(reader);

                // Store the definition
                this.AddDefinition(particleDef);
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "ParticleDefinitions")
                return;
        }
    }

    public Particle[] Particles
    {
        get { return this.activeParticles.ToArray(); }
    }

    public bool HasDefinition(int definitionID)
    {
        return this.definitions.ContainsKey(definitionID);
    }

    public void AddDefinition(ParticleDefinition definition)
    {
        this.definitions.Add(definition.Id, definition);
    }

    public void RemoveDefinition(int definitionId, bool rinseBuffer)
    {
        // Remove definition
        this.definitions.Remove(definitionId);

        // Rinse the buffers?
        if (rinseBuffer)
        {
            for (int i = 0; i < this.activeParticles.Count; i++)
                if (this.activeParticles[i].Definition.Id == definitionId)
                    this.activeParticles.RemoveAt(i);

            for (int i = 0; i < this.particleBuffer.Count; i++)
                if (this.particleBuffer[i].Definition.Id == definitionId)
                    this.particleBuffer.RemoveAt(i);
        }
    }

    public void Add(int typeId, int count)
    {
        this.Add(typeId, count, Vector2.Zero, 0);
    }

    public void Add(int typeId, int count, Vector2 position)
    {
        this.Add(typeId, count, position, 0F);
    }

    public void Add(int typeId, int count, Vector2 position, float angle)
    {
        System.Diagnostics.Debug.WriteLine(this.activeParticles.Count.ToString() + " : " + this.particleBuffer.Count.ToString());

        // Is the key valid?
        if (!this.definitions.ContainsKey(typeId))
            throw new ArgumentException("Invalid type id: " + typeId);

        Particle particle = null;

        for (int i = this.particleBuffer.Count - 1; i >= 0  && count > 0; i--)
        {
            if (this.particleBuffer[i].Definition.Id == typeId)
            {
                // Get particle from the reserve
                particle = this.particleBuffer[i];
                this.particleBuffer.RemoveAt(i);

                // Setup the particle
                this.definitions[typeId].SetupParticle(particle, angle);

                particle.Position = position;
                particle.IsActive = true;

                // Add engine reference
                particle.ParticleEngine = this;

                // Add the particle to the active particles
                this.activeParticles.Add(particle);

                // Decrease the count
                count--;
            }
        }

        // Add more particles if needed
        while (count > 0)
        {
            particle = this.definitions[typeId].Create(angle);
            particle.Position = position;

            // Add engine reference
            particle.ParticleEngine = this;

            this.activeParticles.Add(particle);
            count--;
        }
    }

    public void CheckCollisions(ICollisionObject[] collObjects)
    {
        foreach (ICollisionObject collObj in collObjects)
        {
            if (collObj.CanCollide || collObj.CanTakeDamage)
                this.CheckCollisions(collObj);
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

        for (int i = 0; i < this.activeParticles.Count; i++)
        {
            Particle particle = this.activeParticles[i];

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
        Particle[] particles = new Particle[this.activeParticles.Count];
        this.activeParticles.CopyTo(particles);
        this.activeParticles.Clear();
        this.particleBuffer.AddRange(particles);
    }

    /// <summary>
    /// Clears the engines buffer of particles
    /// </summary>
    public void ClearBuffer()
    {
        this.particleBuffer.Clear();
    }

    /// <summary>
    /// Prepares the engine by having it use parts of its system of force
    /// a JIT of its code.
    /// </summary>
    public void Prepare()
    {
        // Fire a single particle to make the CLR JIT the ParticleEngine
        if (this.definitions.Count > 0)
        {
            foreach (int key in this.definitions.Keys)
            {
                this.Add(key, 1);
                break;
            }
        }

        // Clear out the created particle
        this.ClearActive();
    }

    public void Update(GameTime gameTime)
    {
        for (int i = this.activeParticles.Count - 1; i >= 0; i--)
        {
            // Update the particle
            this.activeParticles[i].Update(gameTime);

            // Has the particle gone inactive?
            if (!this.activeParticles[i].IsActive)
            {
                this.particleBuffer.Add(this.activeParticles[i]);
                this.activeParticles.RemoveAt(i);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        this.Draw(spriteBatch, Vector2.Zero);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
        for (int i = 0; i < this.activeParticles.Count; i++)
        {
            this.activeParticles[i].Draw(spriteBatch, offsetPosition);
        }
    }
}