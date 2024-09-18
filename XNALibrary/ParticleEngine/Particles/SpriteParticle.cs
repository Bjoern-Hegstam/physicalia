using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Sprites;

namespace XNALibrary.ParticleEngine.Particles;

public class SpriteParticle : Particle
{
    public Sprite Sprite { get; set; }

    public float Rotation { get; set; }

    public override Vector2 Origin => new(Sprite.SourceRectangle.Width / 2f, Sprite.SourceRectangle.Height / 2f);

    public override int Width => Sprite.SourceRectangle.Width;

    public override int Height => Sprite.SourceRectangle.Height;

    public virtual Rectangle SourceRectangle => Sprite.SourceRectangle;

    public virtual Texture2D Texture => Sprite.Texture;

    public override Rectangle CollisionBox => new(0, 0, Sprite.SourceRectangle.Width, Sprite.SourceRectangle.Height);

    public override void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
        spriteBatch.Draw(Sprite.Texture,
            Position - offsetPosition,
            Sprite.SourceRectangle,
            Color.White,
            -Rotation,
            Origin,
            1.0F,
            SpriteEffects.None,
            1.0F);
    }
}