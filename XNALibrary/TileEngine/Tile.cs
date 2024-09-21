using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Collision;
using XNALibrary.Graphics;
using XNALibrary.Sprites;

namespace XNALibrary.TileEngine;

public class Tile(Sprite sprite, Rectangle collisionBox, BoxSide collisionSides) : ICollisionObject
{
    /// <summary>
    /// The damage level of the Tile in range [0, 1].
    /// </summary>
    private float _damageLevel;

    public Rectangle CollisionBox
    {
        get => collisionBox;
        set => collisionBox = value;
    }

    public BoxSide CollisionSides { get; set; } = collisionSides;

    public bool GivesDamage { get; set; } = false;

    public float DamageLevel
    {
        get => _damageLevel;
        set => _damageLevel = MathHelper.Clamp(value, 0, 1);
    }

    public ObjectType Type => ObjectType.Tile;

    public Vector2 Position
    {
        get => throw new Exception("The method or operation is not implemented.");
        set => throw new Exception("The method or operation is not implemented.");
    }

    public Vector2 Origin => throw new Exception("The method or operation is not implemented.");

    public Vector2 Velocity
    {
        get => throw new Exception("The method or operation is not implemented.");
        set => throw new Exception("The method or operation is not implemented.");
    }

    public int Width
    {
        get => collisionBox.Width;
        set => throw new Exception("The method or operation is not implemented.");
    }

    public int Height
    {
        get => collisionBox.Height;
        set => throw new Exception("The method or operation is not implemented.");
    }

    public Rectangle SourceRectangle => sprite.SourceRectangle;
    public Texture2D Texture => sprite.Texture;

    public bool CanCollide => CollisionSides != 0;

    public bool CanTakeDamage => false;

    public virtual void OnCollision(ICollisionObject collidedObject, BoxSide collisionSides, Vector2 position,
        Vector2 velocity)
    {
    }

    public virtual void TakeDamage(float damageLevel)
    {
    }
}