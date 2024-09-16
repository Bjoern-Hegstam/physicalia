using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Graphics.ParticleEngine.Particles;

public class AnimationParticle : Particle
{
    private Animation animation;
    private ObjectType damageObjects;
    private float damageAmount;

    /// <summary>
    /// Gets or sets the animation used by the particle
    /// </summary>
    public Animation Animation
    {
        get { return this.animation; }
        set { this.animation = value; }
    }

    /// <summary>
    /// Gets or sets a bitflagged enum denoting which objects the particle
    /// can damage.
    /// </summary>
    public ObjectType DamageObjects
    {
        get { return this.damageObjects; }
        set { this.damageObjects = value; }
    }

    /// <summary>
    /// Gets or sets the amount of damage caused by the particle
    /// </summary>
    public float DamageAmount
    {
        get { return this.damageAmount; }
        set { this.damageAmount = value; }
    }

    public AnimationParticle(Animation animation)
    {
        this.animation = animation;
        this.damageObjects = 0;
        this.damageAmount = 0F;
    }

    public override Microsoft.Xna.Framework.Vector2 Origin
    {
        get { return new Vector2(this.animation.SourceRectangle.Width / 2, this.animation.SourceRectangle.Height / 2); }
    }

    public override int Width
    {
        get { return this.animation.SourceRectangle.Width; }
        set
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public override int Height
    {
        get { return this.animation.SourceRectangle.Height; }
        set
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public override Rectangle SourceRectangle
    {
        get { return this.animation.SourceRectangle; }
    }

    public override Texture2D Texture
    {
        get { return this.animation.Texture; }
    }

    public override Rectangle CollisionBox
    {
        get { return new Rectangle(0, 0, this.Width, this.Height); }
    }

    public override void OnCollision(ICollisionObject collisionObject, BoxSide collisionSides, Vector2 position, Vector2 velocity)
    {
        // Damage the object if possible
        if ((this.damageObjects & collisionObject.Type) != 0)
        {
            if (collisionObject.CanTakeDamage)
                collisionObject.TakeDamage(this.damageAmount);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (!this.animation.IsActive)
        {
            this.animation.Stop();
            this.IsActive = false;
        }
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
        if (this.animation.IsActive)
            spriteBatch.Draw(this.animation.Texture,
                this.position - offsetPosition,
                this.animation.SourceRectangle,
                Color.White,
                0F,
                this.Origin,
                1.0F,
                SpriteEffects.None,
                1.0F);
    }
}