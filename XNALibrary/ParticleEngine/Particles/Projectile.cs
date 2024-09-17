using Microsoft.Xna.Framework;
using XNALibrary.Collision;
using XNALibrary.Sprites;
using XNALibrary.TileEngine;

namespace XNALibrary.ParticleEngine.Particles;

public class Projectile : SpriteParticle
{
    private bool _spawnOnImpact;
    private int _collisionProjectileId;
    private ObjectType _damageObjects;
    private float _damageAmount;

    public ObjectType DamageObjects
    {
        get => _damageObjects;
        set => _damageObjects = value;
    }

    public float DamageAmount
    {
        get => _damageAmount;
        set => _damageAmount = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether a new particle should be
    /// spawned when the current one collides with an object.
    /// </summary>
    public bool SpawnOnImpact
    {
        get => _spawnOnImpact;
        set => _spawnOnImpact = value;
    }

    /// <summary>
    /// Gets or sets the id of the type of which a projectile should be
    /// created if Particle.SpawnOnImpact is set to true.
    /// </summary>
    public int CollisionProjectileId
    {
        get => _collisionProjectileId;
        set => _collisionProjectileId = value;
    }

    public Projectile(Sprite sprite)
    {
        Sprite = sprite;
        _damageObjects = 0;
        _spawnOnImpact = false;
        _collisionProjectileId = -1;
    }

    public override void OnCollision(ICollisionObject collidedObject, BoxSide collisionSides, Vector2 position,
        Vector2 velocity)
    {
        // Damage the object if possible
        if ((_damageObjects & collidedObject.Type) != 0)
        {
            if (collidedObject.CanTakeDamage)
            {
                collidedObject.TakeDamage(_damageAmount);
            }

            // Go inactive if we collided with the object
            if (collidedObject.CanCollide)
            {
                IsActive = false;

                // See if a new particle should be fired on collision
                if (_spawnOnImpact)
                {
                    ParticleEngine.Add(_collisionProjectileId, 1, this.position);
                }
            }
        }
    }
}