using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using XNALibrary.Services;

namespace XNALibrary.Graphics.ParticleEngine.Particles;

public class AnimationParticleDefinition : ParticleDefinition
{
    private Animation animation;
    private IAnimationManager animationManager;
    private ObjectType damageObjects;
    private float damageAmount;

    private List<Animation> createdAnimations;

    public Animation Animation
    {
        get { return this.animation; }
        set { this.animation = value; }
    }

    public ObjectType DamageObjects
    {
        get { return this.damageObjects; }
        set { this.damageObjects = value; }
    }

    public float DamageAmount
    {
        get { return this.damageAmount; }
        set { this.damageAmount = value; }
    }

    public AnimationParticleDefinition(int id, Animation animation, IAnimationManager animationManager)
        : base(id)
    {
        this.animation = animation;
        this.animationManager = animationManager;

        this.createdAnimations = new List<Animation>();
    }

    public override Particle Create(float angle)
    {
        Animation particleAnimation = null;

        // See if a reusable animations has already been created
        for (int i = 0; i < this.createdAnimations.Count; i++)
        {
            if (this.createdAnimations[i].IsActive == false)
            {
                this.createdAnimations[i].FrameIndex = 0;
                particleAnimation = this.createdAnimations[i];
            }
        }

        // Create a new animation if none could be reused
        if (particleAnimation == null)
        {
            particleAnimation = this.animation.Copy();
            this.animationManager.AddPlaybackAnimation(particleAnimation);
            this.createdAnimations.Add(particleAnimation);
        }

        AnimationParticle particle = new AnimationParticle(particleAnimation);
        this.SetupParticle(particle, angle);

        return particle;
    }

    public override void SetupParticle(Particle particle, float angle)
    {
        base.SetupParticle(particle, angle);

        AnimationParticle animParticle = (AnimationParticle)particle;
        animParticle.DamageAmount = this.damageAmount;
        animParticle.DamageObjects = this.damageObjects;
        animParticle.CanCollide = true;
        animParticle.IsActive = true;
        animParticle.Animation.Play();
    }

    protected override void OnLoadXml(XmlReader reader)
    {
        if (reader.NodeType == XmlNodeType.Element &&
            reader.LocalName == "Damage")
        {
            this.damageAmount = int.Parse(reader.GetAttribute("amount"));
        }

        if (reader.NodeType == XmlNodeType.Element &&
            reader.LocalName == "DamageObjects")
        {
            String[] objects = reader.ReadElementContentAsString().Split(' ');

            if (objects.Length > 0 && objects[0] != "")
                for (int i = 0; i < objects.Length; i++)
                {
                    ObjectType objectType = (ObjectType)Enum.Parse(typeof(ObjectType), objects[i]);
                    this.damageObjects |= objectType;
                }
        }
    }
}