using System.Xml;
using XNALibrary.Interfaces;

namespace XNALibrary.Graphics.ParticleEngine.Particles;

public class AnimationParticleDefinition : ParticleDefinition
{
    private Animation.Animation animation;
    private IAnimationManager animationManager;
    private ObjectType damageObjects;
    private float damageAmount;

    private List<Animation.Animation> createdAnimations;

    public Animation.Animation Animation
    {
        get { return animation; }
        set { animation = value; }
    }

    public ObjectType DamageObjects
    {
        get { return damageObjects; }
        set { damageObjects = value; }
    }

    public float DamageAmount
    {
        get { return damageAmount; }
        set { damageAmount = value; }
    }

    public AnimationParticleDefinition(int id, Animation.Animation animation, IAnimationManager animationManager)
        : base(id)
    {
        this.animation = animation;
        this.animationManager = animationManager;

        createdAnimations = new List<Animation.Animation>();
    }

    public override Particle Create(float angle)
    {
        Animation.Animation particleAnimation = null;

        // See if a reusable animations has already been created
        for (int i = 0; i < createdAnimations.Count; i++)
        {
            if (createdAnimations[i].IsActive == false)
            {
                createdAnimations[i].FrameIndex = 0;
                particleAnimation = createdAnimations[i];
            }
        }

        // Create a new animation if none could be reused
        if (particleAnimation == null)
        {
            particleAnimation = animation.Copy();
            animationManager.AddPlaybackAnimation(particleAnimation);
            createdAnimations.Add(particleAnimation);
        }

        AnimationParticle particle = new AnimationParticle(particleAnimation);
        this.SetupParticle(particle, angle);

        return particle;
    }

    public override void SetupParticle(Particle particle, float angle)
    {
        base.SetupParticle(particle, angle);

        AnimationParticle animParticle = (AnimationParticle)particle;
        animParticle.DamageAmount = damageAmount;
        animParticle.DamageObjects = damageObjects;
        animParticle.CanCollide = true;
        animParticle.IsActive = true;
        animParticle.Animation.Play();
    }

    protected override void OnLoadXml(XmlReader reader)
    {
        if (reader.NodeType == XmlNodeType.Element &&
            reader.LocalName == "Damage")
        {
            damageAmount = int.Parse(reader.GetAttribute("amount"));
        }

        if (reader.NodeType == XmlNodeType.Element &&
            reader.LocalName == "DamageObjects")
        {
            String[] objects = reader.ReadElementContentAsString().Split(' ');

            if (objects.Length > 0 && objects[0] != "")
                for (int i = 0; i < objects.Length; i++)
                {
                    ObjectType objectType = (ObjectType)Enum.Parse(typeof(ObjectType), objects[i]);
                    damageObjects |= objectType;
                }
        }
    }
}