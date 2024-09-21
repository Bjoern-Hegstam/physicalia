using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Collision;
using XNALibrary.Graphics;
using XNALibrary.Sprites;

namespace XNALibrary.TileEngine;

public class Tile(Sprite sprite, Rectangle collisionBox, BoxSide collisionSides) : ICollidable
{
    public ObjectType Type => ObjectType.Tile;
    public Rectangle CollisionBox => collisionBox;
    public BoxSide CollisionSides => collisionSides;

    public Vector2 Position => throw new NotSupportedException("The method or operation is not implemented.");
    public Vector2 Origin => throw new NotSupportedException("The method or operation is not implemented.");
    public Vector2 Velocity => throw new NotSupportedException("The method or operation is not implemented.");

    public Rectangle SourceRectangle => sprite.SourceRectangle;
    public Texture2D Texture => sprite.Texture;

    public bool CanCollide => CollisionSides != 0;

    public bool CanTakeDamage => false;

    public virtual void OnCollision(ICollidable collidedObject, BoxSide collisionSides, Vector2 position,
        Vector2 velocity)
    {
    }

    public virtual void TakeDamage(float damageLevel)
    {
    }
}