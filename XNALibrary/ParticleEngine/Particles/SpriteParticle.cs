using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Sprites;

namespace XNALibrary.ParticleEngine.Particles;

public class SpriteParticle(Sprite sprite) : Particle
{
    public Sprite Sprite { get; } = sprite;

    public float Rotation { get; set; }

    public override Rectangle CollisionBoxDefinition =>
        new(0, 0, Sprite.SourceRectangle.Width, Sprite.SourceRectangle.Height);

    public override void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
        spriteBatch.Draw(
            Sprite.Texture,
            Position - offsetPosition,
            Sprite.SourceRectangle,
            Color.White,
            -Rotation,
            new Vector2(
                Sprite.SourceRectangle.Width / 2f,
                Sprite.SourceRectangle.Height / 2f
            ),
            1.0F,
            SpriteEffects.None,
            1.0F
        );
    }
}