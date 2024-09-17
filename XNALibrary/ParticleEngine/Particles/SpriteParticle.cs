using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Sprites;

namespace XNALibrary.ParticleEngine.Particles;

public class SpriteParticle : Particle
{
    private Sprite _sprite;

    public Sprite Sprite
    {
        get => _sprite;
        set => _sprite = value;
    }

    public float Rotation { get; set; }

    public override Vector2 Origin => new(_sprite.SourceRectangle.Width / 2f, _sprite.SourceRectangle.Height / 2f);

    public override int Width
    {
        get => _sprite.SourceRectangle.Width;
        set => throw new Exception("The method or operation is not implemented.");
    }

    public override int Height
    {
        get => _sprite.SourceRectangle.Height;
        set => throw new Exception("The method or operation is not implemented.");
    }

    public override Rectangle SourceRectangle => _sprite.SourceRectangle;

    public override Texture2D Texture => _sprite.Texture;

    public override Rectangle CollisionBox => new(0, 0, _sprite.SourceRectangle.Width, _sprite.SourceRectangle.Height);

    public override void Draw(SpriteBatch? spriteBatch)
    {
        Draw(spriteBatch, Vector2.Zero);
    }

    public override void Draw(SpriteBatch? spriteBatch, Vector2 offsetPosition)
    {
        spriteBatch.Draw(_sprite.Texture,
            Position - offsetPosition,
            _sprite.SourceRectangle,
            Color.White,
            -Rotation,
            Origin,
            1.0F,
            SpriteEffects.None,
            1.0F);
    }
}