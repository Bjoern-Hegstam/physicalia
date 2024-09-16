using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Graphics;

namespace XNALibrary.Graphics.TileEngine;

public abstract class Tile : ICollisionObject
{
    /// <summary>
    /// The collision of the Tile.
    /// </summary>
    private BoxSide collisionSides;

    /// <summary>
    /// Collision box of the Tile.
    /// </summary>
    private Rectangle collisionBox;

    /// <summary>
    /// Indicates whether the Tile can give damage.
    /// </summary>
    private bool givesDamage;
        
    /// <summary>
    /// The damage level of the Tile in procetual damage in decimal form.
    /// </summary>
    private float damageLevel;

    /// <summary>
    /// The sides of the Tile that can give damage.
    /// </summary>
    private BoxSide damageSides;

    /// <summary>
    /// Gets and Sets the collision box of the Tile.
    /// </summary>
    public Rectangle CollisionBox
    {
        get { return this.collisionBox; }
        set { this.collisionBox = value; }
    }

    /// <summary>
    /// Gets and Sets the collision sides of the tile.
    /// </summary>
    public BoxSide CollisionSides
    {
        get { return this.collisionSides; }
        set { this.collisionSides = value; }
    }

    /// <summary>
    /// Gets and Sets a bool denoting whether the tile can give damage.
    /// </summary>
    public bool GivesDamage
    {
        get { return this.givesDamage; }
        set { this.givesDamage = value; }
    }

    /// <summary>
    /// Gets and Sets the damage level of the Tile. The value can be between
    /// 0.0 and 1.0
    /// </summary>
    public float DamageLevel
    {
        get { return this.damageLevel; }
        set { this.damageLevel = (float)MathHelper.Clamp(value, 0, 1); }
    }

    /// <summary>
    /// Gets and Sets the damage sides of the Tile.
    /// </summary>
    public BoxSide DamageSides
    {
        get { return this.damageSides; }
        set { this.damageSides = value; }
    }

    /// <summary>
    /// Creates a new Tile.
    /// </summary>
    public Tile() : this(Rectangle.Empty, 0) { }

    /// <summary>
    /// Creates a new tile.
    /// </summary>
    /// <param name="collisionSides">Collision sides of the tile.</param>
    public Tile(Rectangle collisionBox, BoxSide collisionSides)
    {
        this.collisionBox = collisionBox;
        this.collisionSides = collisionSides;
        this.givesDamage = false;
        this.damageLevel = 0;
        this.damageSides = 0;
    }

    // ICollisionObject is only implemented for consistency and only provides
    // support for the Type propery. In the future other parts may be
    // implemented.

    public ObjectType Type
    {
        get { return ObjectType.Tile; }
    }

    public Vector2 Position
    {
        get
        {
            throw new Exception("The method or operation is not implemented.");
        }
        set
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public Vector2 Origin
    {
        get { throw new Exception("The method or operation is not implemented."); }
    }

    public Vector2 Velocity
    {
        get
        {
            throw new Exception("The method or operation is not implemented.");
        }
        set
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public int Width
    {
        get
        {
            return this.collisionBox.Width;
        }
        set
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public int Height
    {
        get
        {
            return this.collisionBox.Height;
        }
        set
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public abstract Rectangle SourceRectangle { get; }
    public abstract Texture2D Texture { get; }

    public bool CanCollide
    {
        get { return this.collisionSides != 0; }
    }

    public bool CanTakeDamage
    {
        get { return false; }
    }

    public virtual void OnCollision(ICollisionObject collidedObject, BoxSide collisionSides, Vector2 position, Vector2 velocity) { }

    public virtual void TakeDamage(float damageLevel)
    {
        // Tiles don't take damage by default
        // In the future this class could be extended to allow the player
        // to destroy parts of the level.
    }
}