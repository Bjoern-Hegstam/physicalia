using System.Xml;
using XNALibrary.Sprites;

namespace XNALibrary.ParticleEngine.Particles;

public class ProjectileDefinition(int id, Sprite sprite) : SpriteParticleDefinition(id, sprite)
{
    public ObjectType DamageObjects { get; set; }

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
        projectile.DamageObjects = DamageObjects;
        projectile.DamageAmount = DamageAmount;
        projectile.CanCollide = true;
    }

    protected override void OnLoadXml(XmlReader reader)
    {
        if (reader is { NodeType: XmlNodeType.Element, LocalName: "Damage" })
        {
            DamageAmount = int.Parse(reader.GetAttribute("amount") ?? throw new ResourceLoadException());
        }

        if (reader is { NodeType: XmlNodeType.Element, LocalName: "DamageObjects" })
        {
            string[] objects = reader.ReadElementContentAsString().Split(' ');

            if (objects.Length <= 0 || objects[0] == "")
            {
                return;
            }

            foreach (string objString in objects)
            {
                var objectType = Enum.Parse<ObjectType>(objString);
                DamageObjects |= objectType;
            }
        }
    }
}