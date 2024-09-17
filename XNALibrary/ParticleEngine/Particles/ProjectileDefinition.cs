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
        Projectile projectile = new Projectile(Sprite);
        SetupParticle(projectile, angle);

        return projectile;
    }

    public override void SetupParticle(Particle particle, float angle)
    {
        base.SetupParticle(particle, angle);

        Projectile projectile = (Projectile)particle;
        projectile.DamageObjects = DamageObjects;
        projectile.DamageAmount = DamageAmount;
        projectile.CanCollide = true;
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
            {
                for (int i = 0; i < objects.Length; i++)
                {
                    ObjectType objectType = (ObjectType)Enum.Parse(typeof(ObjectType), objects[i]);
                    DamageObjects |= objectType;
                }
            }
        }
    }
}