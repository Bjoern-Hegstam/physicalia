using System;
using System.Collections.Generic;
using System.Text;
using XNALibrary;
using XNALibrary.Graphics.Particles;
using Microsoft.Xna.Framework;
using XNALibrary.Graphics;

namespace XNALibrary.Graphics.ParticleEngine.Particles;

public class Projectile : SpriteParticle
{
    private bool spawnOnImpact;
    private int collisionProjectileID;
    private ObjectType damageObjects;
    private float damageAmount;

    public ObjectType DamageObjects
    {
        get { return this.damageObjects; }
        set { this.damageObjects = value; }
    }

    public float DamageAmount
    {
        get { return this.damageAmount; }
        set { this.damageAmount = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a new particle should be
    /// spawned when the current one collides with an object.
    /// </summary>
    public bool SpawnOnImpact
    {
        get { return this.spawnOnImpact; }
        set { this.spawnOnImpact = value; }
    }

    /// <summary>
    /// Gets or sets the id of the type of which a projectile should be
    /// created if Particle.SpawnOnImpact is set to true.
    /// </summary>
    public int CollisionProjectileID
    {
        get { return this.collisionProjectileID; }
        set { this.collisionProjectileID = value; }
    }

    public Projectile(Sprite sprite)
    {
        this.Sprite = sprite;
        this.damageObjects = 0;
        this.spawnOnImpact = false;
        this.collisionProjectileID = -1;
    }

    public override void OnCollision(ICollisionObject collidedObject, BoxSide collisionSides, Vector2 position, Vector2 velocity)
    {
        // Damage the object if possible
        if ((this.damageObjects & collidedObject.Type) != 0)
        {
            if (collidedObject.CanTakeDamage)
                collidedObject.TakeDamage(this.damageAmount);

            // Go inactive if we collided with the object
            if (collidedObject.CanCollide)
            {
                this.IsActive = false;

                // See if a new particle should be fired on collision
                if (this.spawnOnImpact)
                    this.ParticleEngine.Add(this.collisionProjectileID, 1, this.position);
            }
        }
    }
}