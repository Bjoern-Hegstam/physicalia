using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.TileEngine;

namespace XNALibrary.Collision;

public interface ICollisionObject
{
    ObjectType Type { get; }

    Vector2 Position { get; set; }

    Vector2 Origin { get; }

    /// <summary>
    /// Gets & Sets the velocity of the object, measured in units.
    /// </summary>
    Vector2 Velocity { get; set; }

    /// <summary>
    /// Gets & Sets the width of the object in pixels.
    /// </summary>
    int Width { get; set; }

    /// <summary>
    /// Gets & Sets the height of the object in pixels.
    /// </summary>
    int Height { get; set; }

    Rectangle SourceRectangle { get; }

    Texture2D Texture { get; }

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

public static class CollisionHelper
{
    /// <summary>
    /// Checks if the collision boxes of the ICollisionObjects are intersecting.
    /// </summary>
    /// <param name="collA"></param>
    /// <param name="collB"></param>
    /// <returns>True if the collision boxes intersects; false otherwise.</returns>
    public static bool IsColliding(ICollisionObject collA, ICollisionObject collB)
    {
        // Get collision boxes
        Rectangle rectA = collA.CollisionBox;
        Rectangle rectB = collB.CollisionBox;

        // Setup positions
        rectA.X += (int)(collA.Position.X - collA.Origin.X);
        rectA.Y += (int)(collA.Position.Y - collA.Origin.Y);

        rectB.X += (int)(collB.Position.X - collB.Origin.X);
        rectB.Y += (int)(collB.Position.Y - collB.Origin.Y);

        return rectA.Intersects(rectB);
    }

    public static bool PerPixelCollisionCheck(ICollisionObject collA, ICollisionObject collB)
    {
        // Get correctly positioned Collision boxes
        Rectangle rectA = GetAbsoluteCollisionBox(collA);
        Rectangle rectB = GetAbsoluteCollisionBox(collB);

        Rectangle collisionArea = GetCollisionArea(rectA, rectB);

        return false;
    }

    private static Rectangle GetAbsoluteCollisionBox(ICollisionObject collObj)
    {
        Rectangle collRect = collObj.CollisionBox;
        collRect.X += (int)(collObj.Position.X - collObj.Origin.X);
        collRect.Y += (int)(collObj.Position.Y - collObj.Origin.Y);

        return collRect;
    }

    private static Rectangle GetCollisionArea(Rectangle rectA, Rectangle rectB)
    {
        var collisionArea = Rectangle.Empty;

        collisionArea.X = Math.Max(rectA.X, rectB.X);
        collisionArea.Y = Math.Max(rectA.Y, rectB.Y);
        collisionArea.Width = Math.Min(rectA.Right, rectB.Right) - collisionArea.X;
        collisionArea.Height = Math.Min(rectA.Height, rectB.Height) - collisionArea.Y;

        return collisionArea;
    }
}