using System.Xml;
using XNALibrary.Sprites;

namespace XNALibrary.ParticleEngine.Particles;

public class SpriteParticleDefinition : ParticleDefinition
{
    private Sprite _sprite;

    public Sprite Sprite
    {
        get => _sprite;
        set => _sprite = value;
    }

    public SpriteParticleDefinition(int id)
        : base(id)
    {
    }

    public SpriteParticleDefinition(int id, Sprite sprite)
        : this(id)
    {
        _sprite = sprite;
    }

    public override Particle Create(float angle)
    {
        SpriteParticle particle = new SpriteParticle();
        SetupParticle(particle, angle);
        return particle;
    }

    public override void SetupParticle(Particle particle, float angle)
    {
        base.SetupParticle(particle, angle);

        SpriteParticle spriteParticle = (SpriteParticle)particle;

        spriteParticle.Sprite = _sprite;
        spriteParticle.Rotation = angle;
    }

    protected override void OnLoadXml(XmlReader reader)
    {
    }
}