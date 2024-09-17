using System.Xml;
using XNALibrary.Sprites;

namespace XNALibrary.ParticleEngine.Particles;

public class ProjectileDefinition : SpriteParticleDefinition
{
    public ObjectType DamageObjects { get; set; }

    public float DamageAmount { get; set; }

    public ProjectileDefinition(int id, Sprite sprite)
        : base(id, sprite)
    {
    }

    public override Particle Create(float angle)
    {
        var projectile = new Projectile(Sprite);
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
            DamageAmount = int.Parse(reader.GetAttribute("amount"));
        }

        if (reader is { NodeType: XmlNodeType.Element, LocalName: "DamageObjects" })
        {
            string[] objects = reader.ReadElementContentAsString().Split(' ');

            if (objects.Length > 0 && objects[0] != "")
            {
                for (var i = 0; i < objects.Length; i++)
                {
                    var objectType = (ObjectType)Enum.Parse(typeof(ObjectType), objects[i]);
                    DamageObjects |= objectType;
                }
            }
        }
    }
}