using System.Xml;
using XNALibrary.Interfaces;

namespace XNALibrary.Graphics.ParticleEngine.Particles;

public class AnimationParticleDefinition(int id, Animation.Animation animation, IAnimationManager animationManager)
    : ParticleDefinition(id)
{
    private readonly List<Animation.Animation> _createdAnimations = new();

    public ObjectType DamageObjects { get; set; }

    public float DamageAmount { get; set; }

    public override Particle Create(float angle)
    {
        Animation.Animation particleAnimation = null;

        // See if a reusable animations has already been created
        for (int i = 0; i < _createdAnimations.Count; i++)
        {
            if (_createdAnimations[i].IsActive == false)
            {
                _createdAnimations[i].FrameIndex = 0;
                particleAnimation = _createdAnimations[i];
            }
        }

        // Create a new animation if none could be reused
        if (particleAnimation == null)
        {
            particleAnimation = animation.Copy();
            animationManager.AddPlaybackAnimation(particleAnimation);
            _createdAnimations.Add(particleAnimation);
        }

        AnimationParticle particle = new AnimationParticle(particleAnimation);
        SetupParticle(particle, angle);

        return particle;
    }

    public override void SetupParticle(Particle particle, float angle)
    {
        base.SetupParticle(particle, angle);

        AnimationParticle animParticle = (AnimationParticle)particle;
        animParticle.DamageAmount = DamageAmount;
        animParticle.DamageObjects = DamageObjects;
        animParticle.CanCollide = true;
        animParticle.IsActive = true;
        animParticle.Animation.Play();
    }

    protected override void OnLoadXml(XmlReader reader)
    {
        if (reader.NodeType == XmlNodeType.Element &&
            reader.LocalName == "Damage")
        {
            DamageAmount = int.Parse(reader.GetAttribute("amount"));
        }

        if (reader.NodeType == XmlNodeType.Element &&
            reader.LocalName == "DamageObjects")
        {
            String[] objects = reader.ReadElementContentAsString().Split(' ');

            if (objects.Length > 0 && objects[0] != "")
                for (int i = 0; i < objects.Length; i++)
                {
                    ObjectType objectType = (ObjectType)Enum.Parse(typeof(ObjectType), objects[i]);
                    DamageObjects |= objectType;
                }
        }
    }
}