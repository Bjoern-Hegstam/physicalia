using Microsoft.Xna.Framework;

namespace XNALibrary.Collision;

public static class CollisionHelper
{
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