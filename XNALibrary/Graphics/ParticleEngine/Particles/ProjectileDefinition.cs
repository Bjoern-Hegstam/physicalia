using System;
using System.Collections.Generic;
using System.Text;
using XNALibrary.Graphics;
using System.Xml;

namespace XNALibrary.Graphics.ParticleEngine.Particles;

public class ProjectileDefinition : SpriteParticleDefinition
{
    private ObjectType damageObjects;
    private float damageAmount;

    public ObjectType DamageObjects
    {
        get { return this.damageObjects; }
        set { this.damageObjects = value; }
    }

    public float DamageAmount
    {
        get { return this.damageAmount; }
        set { this.damageAmount = value; }
    }

    public ProjectileDefinition(int id, Sprite sprite)
        : base(id, sprite) { }

    public override Particle Create(float angle)
    {
        Projectile projectile = new Projectile(this.Sprite);
        this.SetupParticle(projectile, angle);

        return projectile;
    }

    public override void SetupParticle(Particle particle, float angle)
    {
        base.SetupParticle(particle, angle);

        Projectile projectile = (Projectile)particle;
        projectile.DamageObjects = this.damageObjects;
        projectile.DamageAmount = this.damageAmount;
        projectile.CanCollide = true;
    }

    protected override void OnLoadXml(XmlReader reader)
    {
        if (reader.NodeType == XmlNodeType.Element &&
            reader.LocalName == "Damage")
        {
            this.damageAmount = int.Parse(reader.GetAttribute("amount"));
        }

        if (reader.NodeType == XmlNodeType.Element &&
            reader.LocalName == "DamageObjects")
        {
            String[] objects = reader.ReadElementContentAsString().Split(' ');

            if (objects.Length > 0 && objects[0] != "")
                for (int i = 0; i < objects.Length; i++)
                {
                    ObjectType objectType = (ObjectType)Enum.Parse(typeof(ObjectType), objects[i]);
                    this.damageObjects |= objectType;
                }
        }
    }
}