using System.Xml;
using XNALibrary.Graphics.Sprites;
using XNALibrary.Interfaces;

namespace XNALibrary.Graphics.ParticleEngine.Particles;

public class ProjectileDefinition : SpriteParticleDefinition
{
    private ObjectType _damageObjects;
    private float _damageAmount;

    public ObjectType DamageObjects
    {
        get => _damageObjects;
        set => _damageObjects = value;
    }

    public float DamageAmount
    {
        get => _damageAmount;
        set => _damageAmount = value;
    }

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
        projectile.DamageObjects = _damageObjects;
        projectile.DamageAmount = _damageAmount;
        projectile.CanCollide = true;
    }

    protected override void OnLoadXml(XmlReader reader)
    {
        if (reader.NodeType == XmlNodeType.Element &&
            reader.LocalName == "Damage")
        {
            _damageAmount = int.Parse(reader.GetAttribute("amount"));
        }

        if (reader.NodeType == XmlNodeType.Element &&
            reader.LocalName == "DamageObjects")
        {
            String[] objects = reader.ReadElementContentAsString().Split(' ');

            if (objects.Length > 0 && objects[0] != "")
                for (int i = 0; i < objects.Length; i++)
                {
                    ObjectType objectType = (ObjectType)Enum.Parse(typeof(ObjectType), objects[i]);
                    _damageObjects |= objectType;
                }
        }
    }
}