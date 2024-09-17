using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Interfaces;

namespace XNALibrary.Graphics.TileEngine;

public abstract class Tile : ICollisionObject
{
    /// <summary>
    /// The collision of the Tile.
    /// </summary>
    private BoxSide _collisionSides;

    /// <summary>
    /// Collision box of the Tile.
    /// </summary>
    private Rectangle _collisionBox;

    /// <summary>
    /// Indicates whether the Tile can give damage.
    /// </summary>
    private bool _givesDamage;

    /// <summary>
    /// The damage level of the Tile in procetual damage in decimal form.
    /// </summary>
    private float _damageLevel;

    /// <summary>
    /// The sides of the Tile that can give damage.
    /// </summary>
    private BoxSide _damageSides;

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
    public BoxSide CollisionSides
    {
        get => _collisionSides;
        set => _collisionSides = value;
    }

    /// <summary>
    /// Gets and Sets a bool denoting whether the tile can give damage.
    /// </summary>
    public bool GivesDamage
    {
        get => _givesDamage;
        set => _givesDamage = value;
    }

    /// <summary>
    /// Gets and Sets the damage level of the Tile. The value can be between
    /// 0.0 and 1.0
    /// </summary>
    public float DamageLevel
    {
        get => _damageLevel;
        set => _damageLevel = (float)MathHelper.Clamp(value, 0, 1);
    }

    /// <summary>
    /// Gets and Sets the damage sides of the Tile.
    /// </summary>
    public BoxSide DamageSides
    {
        get => _damageSides;
        set => _damageSides = value;
    }

    /// <summary>
    /// Creates a new Tile.
    /// </summary>
    public Tile() : this(Rectangle.Empty, 0)
    {
    }

    /// <summary>
    /// Creates a new tile.
    /// </summary>
    /// <param name="collisionSides">Collision sides of the tile.</param>
    public Tile(Rectangle collisionBox, BoxSide collisionSides)
    {
        _collisionBox = collisionBox;
        _collisionSides = collisionSides;
        _givesDamage = false;
        _damageLevel = 0;
        _damageSides = 0;
    }

    // ICollisionObject is only implemented for consistency and only provides
    // support for the Type propery. In the future other parts may be
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

    public bool CanCollide => _collisionSides != 0;

    public bool CanTakeDamage => false;

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