using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Graphics.Sprites;

namespace XNALibrary.Graphics.ParticleEngine.Particles;

public class SpriteParticle : Particle
{
    private float _rotation;
    private Sprite _sprite;

    public Sprite Sprite
    {
        get => _sprite;
        set => _sprite = value;
    }

    public float Rotation
    {
        get => _rotation;
        set => _rotation = value;
    }

    public SpriteParticle()
    {
    }

    public override Vector2 Origin => new(_sprite.SourceRectangle.Width / 2, _sprite.SourceRectangle.Height / 2);

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

    public override void Draw(SpriteBatch spriteBatch)
    {
        Draw(spriteBatch, Vector2.Zero);
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
        spriteBatch.Draw(_sprite.Texture,
            position - offsetPosition,
            _sprite.SourceRectangle,
            Color.White,
            -_rotation,
            Origin,
            1.0F,
            SpriteEffects.None,
            1.0F);
    }
}