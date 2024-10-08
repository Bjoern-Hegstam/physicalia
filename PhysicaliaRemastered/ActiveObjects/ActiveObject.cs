using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Collision;
using XNALibrary.TileEngine;

namespace PhysicaliaRemastered.ActiveObjects;

/// <summary>
/// Base class for active objects such as coins, doors, platforms, etc.
/// </summary>
public abstract class ActiveObject : ICollidable
{
    private static int _activeObjectCount;

    private Vector2 _position;

    public int UniqueId { get; }

    public bool IsActive
    {
        get => Enabled || Visible;
        set => Enabled = Visible = value;
    }

    public bool Enabled { get; set; }

    public bool Visible { get; set; }

    public List<ActiveObject> Children { get; }

    public Rectangle CollisionBoxDefinition { get; set; }

    public Rectangle WorldCollisionBox => new(
        Position.ToPoint() + CollisionBoxDefinition.Location,
        CollisionBoxDefinition.Size
    );

    public bool CanCollide { get; set; }

    public bool CanTakeDamage { get; set; }

    public virtual Vector2 Position
    {
        get => _position;
        set => _position = value;
    }

    public Vector2 Velocity { get; set; }

    public virtual void TakeDamage(float damageLevel)
    {
    }

    public virtual void OnCollision(ICollidable collidedObject, List<BoxSide> collidedSides,
        Vector2 suggestedNewPosition, Vector2 suggestedNewVelocity)
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

    public void CheckCollisions(IEnumerable<ICollidable> collObjects)
    {
        if (!CanCollide)
        {
            return;
        }

        foreach (ICollidable collObject in collObjects)
        {
            CheckCollision(collObject);
        }
    }

    public abstract void CheckCollision(ICollidable collObject);

    /// <summary>
    /// When overriden in a derived class, resets the state of the ActiveObject.
    /// </summary>
    public virtual void Reset()
    {
    }

    public abstract void Update(GameTime gameTime);

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 viewportPosition)
    {
    }
}