using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary;
using XNALibrary.Collision;
using XNALibrary.TileEngine;

namespace PhysicaliaRemastered.ActiveObjects;

/// <summary>
/// Base class for active objects such as coins, doors, platforms, etc.
/// </summary>
public abstract class ActiveObject : ICollisionObject
{
    private static int _activeObjectCount;

    private Vector2 _position;

    private Rectangle _collisionBox;

    public int UniqueId { get; }

    public bool IsActive
    {
        get => Enabled || Visible;
        set => Enabled = Visible = value;
    }

    public bool Enabled { get; set; }

    public bool Visible { get; set; }

    public List<ActiveObject> Children { get; }

    public virtual ObjectType Type => ObjectType.ActiveObject;

    public Rectangle CollisionBox
    {
        get => _collisionBox;
        set => _collisionBox = value;
    }

    public bool CanCollide { get; set; }

    public bool CanTakeDamage { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public Vector2 Origin =>
        new(_collisionBox.Width / 2f,
            _collisionBox.Height / 2f);

    public virtual Vector2 Position
    {
        get => _position;
        set => _position = value;
    }

    public Vector2 Velocity { get; set; }

    public virtual void TakeDamage(float damageLevel)
    {
    }

    public virtual void OnCollision(ICollisionObject collidedObject, BoxSide collisionSides, Vector2 position,
        Vector2 velocity)
    {
    }

    protected ActiveObject()
    {
        UniqueId = _activeObjectCount++;

        Children = [];

        Velocity = _position = Vector2.Zero;

        IsActive = false;
        CanCollide = true;
        CanTakeDamage = false;
    }

    public void CheckCollisions(IEnumerable<ICollisionObject> collObjects)
    {
        if (!CanCollide)
        {
            return;
        }

        foreach (ICollisionObject collObject in collObjects)
        {
            CheckCollision(collObject);
        }
    }

    public abstract void CheckCollision(ICollisionObject collObject);

    /// <summary>
    /// When overriden in a derived class, resets the state of the ActiveObject.
    /// </summary>
    public virtual void Reset()
    {
    }

    public abstract void Update(GameTime gameTime);

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
    }
}