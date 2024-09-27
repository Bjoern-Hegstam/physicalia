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
     * The object's collision box relative to its default position
     */
    public Rectangle CollisionBoxDefinition { get; }

    /*
     * The object's collision box in world coordinates
     */
    public Rectangle WorldCollisionBox { get; }

    public bool CanCollide { get; }
    public bool CanTakeDamage { get; }

    public void OnCollision(ICollidable collidedObject, List<BoxSide> collidedSides, Vector2 suggestedNewPosition,
        Vector2 suggestedNewVelocity);

    /// <param name="damageLevel">The level of damage taken. Can range from 0.0 to 1.0</param>
    public void TakeDamage(float damageLevel);
}