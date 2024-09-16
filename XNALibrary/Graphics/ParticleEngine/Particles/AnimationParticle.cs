using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Graphics.TileEngine;
using XNALibrary.Interfaces;

namespace XNALibrary.Graphics.ParticleEngine.Particles;

public class AnimationParticle : Particle
{
    private Animation.Animation animation;
    private ObjectType damageObjects;
    private float damageAmount;

    /// <summary>
    /// Gets or sets the animation used by the particle
    /// </summary>
    public Animation.Animation Animation
    {
        get { return animation; }
        set { animation = value; }
    }

    /// <summary>
    /// Gets or sets a bitflagged enum denoting which objects the particle
    /// can damage.
    /// </summary>
    public ObjectType DamageObjects
    {
        get { return damageObjects; }
        set { damageObjects = value; }
    }

    /// <summary>
    /// Gets or sets the amount of damage caused by the particle
    /// </summary>
    public float DamageAmount
    {
        get { return damageAmount; }
        set { damageAmount = value; }
    }

    public AnimationParticle(Animation.Animation animation)
    {
        this.animation = animation;
        damageObjects = 0;
        damageAmount = 0F;
    }

    public override Vector2 Origin
    {
        get { return new Vector2(animation.SourceRectangle.Width / 2, animation.SourceRectangle.Height / 2); }
    }

    public override int Width
    {
        get { return animation.SourceRectangle.Width; }
        set
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public override int Height
    {
        get { return animation.SourceRectangle.Height; }
        set
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public override Rectangle SourceRectangle
    {
        get { return animation.SourceRectangle; }
    }

    public override Texture2D Texture
    {
        get { return animation.Texture; }
    }

    public override Rectangle CollisionBox
    {
        get { return new Rectangle(0, 0, Width, Height); }
    }

    public override void OnCollision(ICollisionObject collisionObject, BoxSide collisionSides, Vector2 position, Vector2 velocity)
    {
        // Damage the object if possible
        if ((damageObjects & collisionObject.Type) != 0)
        {
            if (collisionObject.CanTakeDamage)
                collisionObject.TakeDamage(damageAmount);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (!animation.IsActive)
        {
            animation.Stop();
            this.IsActive = false;
        }
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
        if (animation.IsActive)
            spriteBatch.Draw(animation.Texture,
                this.position - offsetPosition,
                animation.SourceRectangle,
                Color.White,
                0F,
                Origin,
                1.0F,
                SpriteEffects.None,
                1.0F);
    }
}