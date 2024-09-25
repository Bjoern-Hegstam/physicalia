using Microsoft.Xna.Framework;
using XNALibrary.TileEngine;

namespace XNALibrary.Collision;

public interface ICollidable
{
    Vector2 Position { get; }
    Vector2 Origin { get; }
    Vector2 Velocity { get; }

    bool CanCollide { get; }
    Rectangle CollisionBox { get; }
    bool CanTakeDamage { get; }

    /// <summary>
    /// Called when a collision occured.
    /// </summary>
    /// <param name="collidedObject">Denotes what kind of object the ICollisionObject collided with.</param>
    /// <param name="collidedSides">Flagged enum containing which of its sides the object collided with.</param>
    /// <param name="position">Suggested new position of the object.</param>
    /// <param name="velocity">Suggested new velocity of the object.</param>
    void OnCollision(ICollidable collidedObject, BoxSide collidedSides, Vector2 position, Vector2 velocity);

    /// <summary>
    /// Called when the object takes damage.
    /// </summary>
    /// <param name="damageLevel">The level of damage taken. Can range from 0.0 to 1.0</param>
    void TakeDamage(float damageLevel);

    public static bool AreColliding(ICollidable c1, ICollidable c2)
    {
        Rectangle r1 = c1.CollisionBox;
        Rectangle r2 = c2.CollisionBox;

        r1.Location += (c1.Position - c1.Origin).ToPoint();
        r2.Location += (c2.Position - c2.Origin).ToPoint();
        
        return r1.Intersects(r2);
    }
}