using Microsoft.Xna.Framework;
using XNALibrary.Graphics.Sprites;
using XNALibrary.Graphics.TileEngine;
using XNALibrary.Interfaces;

namespace XNALibrary.Graphics.ParticleEngine.Particles;

public class Projectile : SpriteParticle
{
    private bool spawnOnImpact;
    private int collisionProjectileID;
    private ObjectType damageObjects;
    private float damageAmount;

    public ObjectType DamageObjects
    {
        get { return damageObjects; }
        set { damageObjects = value; }
    }

    public float DamageAmount
    {
        get { return damageAmount; }
        set { damageAmount = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a new particle should be
    /// spawned when the current one collides with an object.
    /// </summary>
    public bool SpawnOnImpact
    {
        get { return spawnOnImpact; }
        set { spawnOnImpact = value; }
    }

    /// <summary>
    /// Gets or sets the id of the type of which a projectile should be
    /// created if Particle.SpawnOnImpact is set to true.
    /// </summary>
    public int CollisionProjectileID
    {
        get { return collisionProjectileID; }
        set { collisionProjectileID = value; }
    }

    public Projectile(Sprite sprite)
    {
        Sprite = sprite;
        damageObjects = 0;
        spawnOnImpact = false;
        collisionProjectileID = -1;
    }

    public override void OnCollision(ICollisionObject collidedObject, BoxSide collisionSides, Vector2 position, Vector2 velocity)
    {
        // Damage the object if possible
        if ((damageObjects & collidedObject.Type) != 0)
        {
            if (collidedObject.CanTakeDamage)
                collidedObject.TakeDamage(damageAmount);

            // Go inactive if we collided with the object
            if (collidedObject.CanCollide)
            {
                this.IsActive = false;

                // See if a new particle should be fired on collision
                if (spawnOnImpact)
                    this.ParticleEngine.Add(collisionProjectileID, 1, this.position);
            }
        }
    }
}