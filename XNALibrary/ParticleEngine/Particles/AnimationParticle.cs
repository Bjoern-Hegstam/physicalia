using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Collision;
using XNALibrary.TileEngine;

namespace XNALibrary.ParticleEngine.Particles;

public class AnimationParticle(Animation.Animation animation) : Particle
{
    /// <summary>
    /// Gets or sets the animation used by the particle
    /// </summary>
    public Animation.Animation Animation { get; set; } = animation;

    /// <summary>
    /// Gets or sets a bitflagged enum denoting which objects the particle
    /// can damage.
    /// </summary>
    public ObjectType DamageObjects { get; set; } = 0;

    /// <summary>
    /// Gets or sets the amount of damage caused by the particle
    /// </summary>
    public float DamageAmount { get; set; } = 0F;

    public override Vector2 Origin => new(Animation.SourceRectangle.Width / 2f, Animation.SourceRectangle.Height / 2f);

    public override int Width
    {
        get => Animation.SourceRectangle.Width;
        set => throw new Exception("The method or operation is not implemented.");
    }

    public override int Height
    {
        get => Animation.SourceRectangle.Height;
        set => throw new Exception("The method or operation is not implemented.");
    }

    public override Rectangle SourceRectangle => Animation.SourceRectangle;

    public override Texture2D Texture => Animation.Texture;

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