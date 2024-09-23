using System.Xml;
using XNALibrary.Sprites;

namespace XNALibrary.ParticleEngine.Particles;

public class SpriteParticleDefinition(int id, Sprite sprite) : ParticleDefinition(id)
{

    public override Particle Create(float angle)
    {
        var particle = new SpriteParticle(sprite);
        SetupParticle(particle, angle);
        return particle;
    }

    public override void SetupParticle(Particle particle, float angle)
    {
        base.SetupParticle(particle, angle);

        var spriteParticle = (SpriteParticle)particle;

        spriteParticle.Rotation = angle;
    }

    protected override void OnLoadXml(XmlReader reader)
    {
    }
}