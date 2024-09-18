using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Collision;
using XNALibrary.TileEngine;

namespace XNALibrary.ParticleEngine.Particles;

public class AnimationParticle(Animation.Animation animation) : Particle
{
    public Animation.Animation Animation { get; set; } = animation;

    /// <summary>
    /// Gets or sets a bitflagged enum denoting which objects the particle
    /// can damage.
    /// </summary>
    public ObjectType DamageObjects { get; set; } = 0;

    public float DamageAmount { get; set; } = 0F;

    public override Vector2 Origin => new(Animation.SourceRectangle.Width / 2f, Animation.SourceRectangle.Height / 2f);

    public override int Width => Animation.SourceRectangle.Width;

    public override int Height => Animation.SourceRectangle.Height;

    public override Rectangle CollisionBox => new(0, 0, Width, Height);

    public override void OnCollision(ICollisionObject collisionObject, BoxSide collisionSides, Vector2 position,
        Vector2 velocity)
    {
        // Damage the object if possible
        if ((DamageObjects & collisionObject.Type) != 0)
        {
            if (collisionObject.CanTakeDamage)
            {
                collisionObject.TakeDamage(DamageAmount);
            }
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
            spriteBatch.Draw(Animation.Texture,
                Position - offsetPosition,
                Animation.SourceRectangle,
                Color.White,
                0F,
                Origin,
                1.0F,
                SpriteEffects.None,
                1.0F);
        }
    }
}