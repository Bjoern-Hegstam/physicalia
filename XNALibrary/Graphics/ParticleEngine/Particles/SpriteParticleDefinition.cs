using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;

namespace XNALibrary.Graphics.ParticleEngine.Particles;

public class SpriteParticleDefinition : ParticleDefinition
{
    private Sprite sprite;

    public Sprite Sprite
    {
        get { return this.sprite; }
        set { this.sprite = value; }
    }

    public SpriteParticleDefinition(int id)
        : base(id) { }

    public SpriteParticleDefinition(int id, Sprite sprite)
        : this(id)
    {
            this.sprite = sprite;
        }

    public override Particle Create(float angle)
    {
            SpriteParticle particle = new SpriteParticle();
            this.SetupParticle(particle, angle);
            return particle;
        }

    public override void SetupParticle(Particle particle, float angle)
    {
            base.SetupParticle(particle, angle);

            SpriteParticle spriteParticle = (SpriteParticle)particle;
            
            spriteParticle.Sprite = this.sprite;
            spriteParticle.Rotation = angle;
        }

    protected override void OnLoadXml(XmlReader reader) { }
}