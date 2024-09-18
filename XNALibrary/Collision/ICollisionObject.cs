using Microsoft.Xna.Framework;
using XNALibrary.TileEngine;

namespace XNALibrary.Collision;

public interface ICollisionObject
{
    ObjectType Type { get; }

    Vector2 Position { get; set; }

    Vector2 Origin { get; }

    Vector2 Velocity { get; set; }

    int Width { get; }
    int Height { get; }    
    
    bool CanCollide { get; }

    Rectangle CollisionBox { get; }

    bool CanTakeDamage { get; }

    /// <summary>
    /// Called when a collision occured.
    /// </summary>
    /// <param name="collidedObject">Denotes what kind of object the
    /// ICollisionObject collided with.</param>
    /// <param name="collisionSides">Flagged enum containing which of its
    /// sides the object collided with.</param>
    /// <param name="position">Suggested new position of the object.</param>
    /// <param name="velocity">Suggested new velocity of the object.</param>
    void OnCollision(ICollisionObject collidedObject, BoxSide collisionSides, Vector2 position, Vector2 velocity);

    /// <summary>
    /// Called when the object takes damage.
    /// </summary>
    /// <param name="damageLevel">The level of damage taken. Can range from
    /// 0.0 to 1.0</param>
    void TakeDamage(float damageLevel);
}