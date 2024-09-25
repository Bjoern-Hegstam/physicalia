using Microsoft.Xna.Framework;
using XNALibrary.Collision;
using XNALibrary.Sprites;
using XNALibrary.TileEngine;

namespace XNALibrary.ParticleEngine.Particles;

public class Projectile(Sprite sprite) : SpriteParticle(sprite)
{
    public float DamageAmount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a new particle should be
    /// spawned when the current one collides with an object.
    /// </summary>
    public bool SpawnOnImpact { get; set; } = false;

    /// <summary>
    /// Gets or sets the id of the type of which a projectile should be
    /// created if Particle.SpawnOnImpact is set to true.
    /// </summary>
    public int CollisionProjectileId { get; set; } = -1;

    public override void OnCollision(ICollidable collidedObject, BoxSide collidedSides, Vector2 position,
        Vector2 velocity)
    {
        if (collidedObject.CanTakeDamage)
        {
            collidedObject.TakeDamage(DamageAmount);
        }

        // Go inactive if we collided with the object
        if (!collidedObject.CanCollide)
        {
            return;
        }

        IsActive = false;

        // See if a new particle should be fired on collision
        if (SpawnOnImpact)
        {
            ParticleEngine.Add(CollisionProjectileId, 1, Position);
        }
    }
}