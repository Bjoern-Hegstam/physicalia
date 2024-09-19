using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Collision;
using XNALibrary.Graphics;

namespace XNALibrary.TileEngine;

public abstract class Tile : ICollisionObject
{
    /// <summary>
    /// Collision box of the Tile.
    /// </summary>
    private Rectangle _collisionBox;

    /// <summary>
    /// The damage level of the Tile in range [0, 1].
    /// </summary>
    private float _damageLevel;

    /// <summary>
    /// Gets and Sets the collision box of the Tile.
    /// </summary>
    public Rectangle CollisionBox
    {
        get => _collisionBox;
        set => _collisionBox = value;
    }

    /// <summary>
    /// Gets and Sets the collision sides of the tile.
    /// </summary>
    public BoxSide CollisionSides { get; set; }

    /// <summary>
    /// Gets and Sets a bool denoting whether the tile can give damage.
    /// </summary>
    public bool GivesDamage { get; set; }

    /// <summary>
    /// Gets and Sets the damage level of the Tile. The value can be between
    /// 0.0 and 1.0
    /// </summary>
    public float DamageLevel
    {
        get => _damageLevel;
        set => _damageLevel = MathHelper.Clamp(value, 0, 1);
    }

    public BoxSide DamageSides { get; set; }

    // ICollisionObject is only implemented for consistency and only provides
    // support for the Type property. In the future other parts may be
    // implemented.

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
        get => _collisionBox.Width;
        set => throw new Exception("The method or operation is not implemented.");
    }

    public int Height
    {
        get => _collisionBox.Height;
        set => throw new Exception("The method or operation is not implemented.");
    }

    public abstract Rectangle SourceRectangle { get; }
    public abstract Texture2D Texture { get; }

    public bool CanCollide => CollisionSides != 0;

    public bool CanTakeDamage => false;
    
    public Tile() : this(Rectangle.Empty, 0)
    {
    }

    public Tile(Rectangle collisionBox, BoxSide collisionSides)
    {
        _collisionBox = collisionBox;
        CollisionSides = collisionSides;
        GivesDamage = false;
        _damageLevel = 0;
        DamageSides = 0;
    }
    
    public virtual void OnCollision(ICollisionObject collidedObject, BoxSide collisionSides, Vector2 position,
        Vector2 velocity)
    {
    }

    public virtual void TakeDamage(float damageLevel)
    {
        // Tiles don't take damage by default
        // In the future this class could be extended to allow the player
        // to destroy parts of the level.
    }
}