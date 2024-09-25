using Microsoft.Xna.Framework;
using XNALibrary.TileEngine;

namespace XNALibrary.Collision;

public interface ICollidable
{
    public Vector2 Position { get; }
    public Vector2 Velocity { get; }

    public Rectangle CollisionBox { get; }
    public Rectangle AbsoluteCollisionBox => new(Position.ToPoint() + CollisionBox.Location, CollisionBox.Size);

    public bool CanCollide { get; }
    public bool CanTakeDamage { get; }

    /// <summary>
    /// Called when a collision occured.
    /// </summary>
    /// <param name="collidedObject">Denotes what kind of object the ICollisionObject collided with.</param>
    /// <param name="collidedSides">Flagged enum containing which of its sides the object collided with.</param>
    /// <param name="position">Suggested new position of the object.</param>
    /// <param name="velocity">Suggested new velocity of the object.</param>
    public void OnCollision(ICollidable collidedObject, List<BoxSide> collidedSides, Vector2 position, Vector2 velocity);

    /// <summary>
    /// Called when the object takes damage.
    /// </summary>
    /// <param name="damageLevel">The level of damage taken. Can range from 0.0 to 1.0</param>
    public void TakeDamage(float damageLevel);

    public static bool AreColliding(ICollidable c1, ICollidable c2)
    {
        Rectangle r1 = c1.CollisionBox;
        Rectangle r2 = c2.CollisionBox;

        r1.Location += c1.Position.ToPoint();
        r2.Location += c2.Position.ToPoint();
        
        return r1.Intersects(r2);
    }
}