using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Collision;
using XNALibrary.TileEngine;

namespace XNALibrary.ParticleEngine.Particles;

public class AnimationParticle(Animation.Animation animation) : Particle
{
    public Animation.Animation Animation { get; set; } = animation;

    public float DamageAmount { get; set; } = 0F;

    public virtual Vector2 Origin => new(Width / 2f, Height / 2f);

    public virtual int Width => Animation.CurrentFrame.SourceRectangle.Width;

    public virtual int Height => Animation.CurrentFrame.SourceRectangle.Height;

    public override Rectangle CollisionBoxDefinition => new(0, 0, Width, Height);

    public override void OnCollision(ICollidable collidable, List<BoxSide> collidedSides, Vector2 suggestedNewPosition,
        Vector2 suggestedNewVelocity)
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