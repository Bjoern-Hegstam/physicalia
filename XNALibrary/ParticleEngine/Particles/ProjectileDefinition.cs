using System.Xml;
using XNALibrary.Sprites;

namespace XNALibrary.ParticleEngine.Particles;

public class ProjectileDefinition(int id, Sprite sprite) : SpriteParticleDefinition(id, sprite)
{
    public float DamageAmount { get; set; }

    public override Particle Create(float angle)
    {
        var projectile = new Projectile(sprite);
        SetupParticle(projectile, angle);

        return projectile;
    }

    public override void SetupParticle(Particle particle, float angle)
    {
        base.SetupParticle(particle, angle);

        var projectile = (Projectile)particle;
        projectile.DamageAmount = DamageAmount;
        projectile.CanCollide = true;
    }

    protected override void OnLoadXml(XmlReader reader)
    {
        if (reader is { NodeType: XmlNodeType.Element, LocalName: "Damage" })
        {
            DamageAmount = int.Parse(reader.GetAttribute("amount") ?? throw new ResourceLoadException());
        }
    }
}