using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Collision;
using XNALibrary.TileEngine;

namespace XNALibrary.ParticleEngine.Particles;

public class AnimationParticle(Animation.Animation animation) : Particle
{
    public Animation.Animation Animation { get; set; } = animation;

    public float DamageAmount { get; set; } = 0F;

    public override Vector2 Origin => new(Width / 2f, Height / 2f);

    public override int Width => Animation.CurrentFrame.SourceRectangle.Width;

    public override int Height => Animation.CurrentFrame.SourceRectangle.Height;

    public override Rectangle CollisionBox => new(0, 0, Width, Height);

    public override void OnCollision(ICollidable collidable, List<BoxSide> collidedSides, Vector2 position,
        Vector2 velocity)
    {
        if (collidable.CanTakeDamage)
        {
            collidable.TakeDamage(DamageAmount);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (!Animation.IsActive)
        {
            Animation.Stop();
            IsActive = false;
        }
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
        if (Animation.IsActive)
        {
            spriteBatch.Draw(
                Animation.CurrentFrame.Texture,
                Position - offsetPosition,
                Animation.CurrentFrame.SourceRectangle,
                Color.White,
                0F,
                Origin,
                1.0F,
                SpriteEffects.None,
                1.0F
            );
        }
    }
}