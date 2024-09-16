using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Graphics.Sprites;

namespace XNALibrary.Graphics.ParticleEngine.Particles;

public class SpriteParticle : Particle
{
    private float rotation;
    private Sprite sprite;

    public Sprite Sprite
    {
        get { return sprite; }
        set { sprite = value; }
    }

    public float Rotation
    {
        get { return rotation; }
        set { rotation = value; }
    }

    public SpriteParticle() { }

    public override Vector2 Origin
    {
        get { return new Vector2(sprite.SourceRectangle.Width / 2, sprite.SourceRectangle.Height / 2); }
    }

    public override int Width
    {
        get { return sprite.SourceRectangle.Width; }
        set { throw new Exception("The method or operation is not implemented."); }
    }

    public override int Height
    {
        get { return sprite.SourceRectangle.Height; }
        set { throw new Exception("The method or operation is not implemented."); }
    }

    public override Rectangle SourceRectangle
    {
        get { return sprite.SourceRectangle; }
    }

    public override Texture2D Texture
    {
        get { return sprite.Texture; }
    }

    public override Rectangle CollisionBox
    {
        get { return new Rectangle(0, 0, sprite.SourceRectangle.Width, sprite.SourceRectangle.Height); }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Draw(spriteBatch, Vector2.Zero);
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
        spriteBatch.Draw(sprite.Texture,
            this.position - offsetPosition,
            sprite.SourceRectangle,
            Color.White,
            -rotation,
            Origin,
            1.0F,
            SpriteEffects.None,
            1.0F);
    }
}