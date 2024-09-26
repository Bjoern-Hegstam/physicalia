using Microsoft.Xna.Framework;
using XNALibrary.TileEngine;

namespace XNALibrary.Collision;

public interface ICollidable
{
    /*
     * The object's world position
     */
    public Vector2 Position { get; }
    public Vector2 Velocity { get; }

    /*
     * The object's collision box relative to its position
     */
    public Rectangle CollisionBox { get; }

    /*
    * The object's collision box in world coordinates
    */
    public Rectangle AbsoluteCollisionBox { get; }

    public bool CanCollide { get; }
    public bool CanTakeDamage { get; }

    public void OnCollision(ICollidable collidedObject, List<BoxSide> collidedSides, Vector2 suggestedNewPosition,
        Vector2 suggestedNewVelocity);

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
