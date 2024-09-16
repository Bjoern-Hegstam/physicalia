using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Graphics.ParticleEngine.Particles;

public class SpriteParticle : Particle
{
    private float rotation;
    private Sprite sprite;

    public Sprite Sprite
    {
        get { return this.sprite; }
        set { this.sprite = value; }
    }

    public float Rotation
    {
        get { return this.rotation; }
        set { this.rotation = value; }
    }

    public SpriteParticle() { }

    public override Vector2 Origin
    {
        get { return new Vector2(this.sprite.SourceRectangle.Width / 2, this.sprite.SourceRectangle.Height / 2); }
    }

    public override int Width
    {
        get { return this.sprite.SourceRectangle.Width; }
        set { throw new Exception("The method or operation is not implemented."); }
    }

    public override int Height
    {
        get { return this.sprite.SourceRectangle.Height; }
        set { throw new Exception("The method or operation is not implemented."); }
    }

    public override Rectangle SourceRectangle
    {
        get { return this.sprite.SourceRectangle; }
    }

    public override Texture2D Texture
    {
        get { return this.sprite.Texture; }
    }

    public override Rectangle CollisionBox
    {
        get { return new Rectangle(0, 0, this.sprite.SourceRectangle.Width, this.sprite.SourceRectangle.Height); }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        this.Draw(spriteBatch, Vector2.Zero);
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
        spriteBatch.Draw(this.sprite.Texture,
            this.position - offsetPosition,
            this.sprite.SourceRectangle,
            Color.White,
            -this.rotation,
            this.Origin,
            1.0F,
            SpriteEffects.None,
            1.0F);
    }
}