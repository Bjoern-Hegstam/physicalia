using System.Xml;
using XNALibrary.Graphics.Sprites;
using XNALibrary.Interfaces;

namespace XNALibrary.Graphics.ParticleEngine.Particles;

public class ProjectileDefinition : SpriteParticleDefinition
{
    private ObjectType damageObjects;
    private float damageAmount;

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

    public ProjectileDefinition(int id, Sprite sprite)
        : base(id, sprite) { }

    public override Particle Create(float angle)
    {
        Projectile projectile = new Projectile(Sprite);
        this.SetupParticle(projectile, angle);

        return projectile;
    }

    public override void SetupParticle(Particle particle, float angle)
    {
        base.SetupParticle(particle, angle);

        Projectile projectile = (Projectile)particle;
        projectile.DamageObjects = damageObjects;
        projectile.DamageAmount = damageAmount;
        projectile.CanCollide = true;
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