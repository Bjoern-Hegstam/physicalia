using System.Xml;
using XNALibrary.Animation;

namespace XNALibrary.ParticleEngine.Particles;

public class AnimationParticleDefinition(int id, Animation.Animation animation, AnimationManager animationManager)
    : ParticleDefinition(id)
{
    private readonly List<Animation.Animation> _createdAnimations = [];

    public ObjectType DamageObjects { get; set; }

    public float DamageAmount { get; set; }

    public override Particle Create(float angle)
    {
        // See if a reusable animations has already been created
        Animation.Animation? particleAnimation = _createdAnimations.FirstOrDefault(anim => !anim.IsActive);

        // Create a new animation if none could be reused
        if (particleAnimation == null)
        {
            particleAnimation = animationManager.AddPlaybackAnimation(animation.AnimationDefinition.Id);
            _createdAnimations.Add(particleAnimation);
        }

        var particle = new AnimationParticle(particleAnimation);
        SetupParticle(particle, angle);

        return particle;
    }

    public override void SetupParticle(Particle particle, float angle)
    {
        base.SetupParticle(particle, angle);

        var animParticle = (AnimationParticle)particle;
        animParticle.DamageAmount = DamageAmount;
        animParticle.DamageObjects = DamageObjects;
        animParticle.CanCollide = true;
        animParticle.IsActive = true;
        animParticle.Animation.Play();
    }

    protected override void OnLoadXml(XmlReader reader)
    {
        if (reader is { NodeType: XmlNodeType.Element, LocalName: "Damage" })
        {
            DamageAmount = int.Parse(reader.GetAttribute("amount") ?? throw new ResourceLoadException());
        }

        if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "DamageObjects")
        {
            return;
        }

        string[] objects = reader.ReadElementContentAsString().Split(' ');

        if (objects.Length <= 0 || objects[0] == "")
        {
            return;
        }

        foreach (string obj in objects)
        {
            var objectType = Enum.Parse<ObjectType>(obj);
            DamageObjects |= objectType;
        }
    }
}