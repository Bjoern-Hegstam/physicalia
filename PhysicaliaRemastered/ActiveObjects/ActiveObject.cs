using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary;
using XNALibrary.Collision;
using XNALibrary.Sprites;
using XNALibrary.TileEngine;

namespace PhysicaliaRemastered.ActiveObjects;

/// <summary>
/// Base class for active objects such as coins, doors, platforms, etc.
/// </summary>
public abstract class ActiveObject : ICollisionObject
{
    private static int _activeObjectCount;

    private Vector2 _position;

    private readonly ISpriteLibrary _spriteLibrary;
    private Sprite _sprite;

    // Width and Height must not be the same dimensions as the Sprite.

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

    public virtual Rectangle SourceRectangle => _sprite.SourceRectangle;

    public virtual Texture2D Texture => _sprite.Texture;

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

    public ActiveObject()
    {
        UniqueId = _activeObjectCount++;

        Children = new List<ActiveObject>();
        Width = _sprite.SourceRectangle.Width;
        Height = _sprite.SourceRectangle.Height;

        Velocity = _position = Vector2.Zero;

        IsActive = false;
        CanCollide = true;
        CanTakeDamage = false;
    }

    public ActiveObject(ISpriteLibrary spriteLibrary, int spriteKey)
        : this()
    {
        _spriteLibrary = spriteLibrary;
        _sprite = _spriteLibrary.GetSprite(spriteKey);
    }

    public void CheckCollisions(ICollisionObject[] collObjects)
    {
        if (CanCollide)
        {
            for (int i = 0; i < collObjects.Length; i++)
            {
                CheckCollision(collObjects[i]);
            }
        }
    }

    public abstract void CheckCollision(ICollisionObject collObject);

    /// <summary>
    /// When overriden in a derived class, resets the state of the ActiveObject.
    /// </summary>
    public virtual void Reset()
    {
    }

    public abstract void Update(GameTime gametime);

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        Draw(spriteBatch, Vector2.Zero);
    }

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
        if (Visible && _spriteLibrary != null)
            spriteBatch.Draw(_sprite.Texture,
                _position - offsetPosition,
                _sprite.SourceRectangle,
                Color.White,
                0F,
                Origin,
                1.0F,
                SpriteEffects.None,
                0.8F);
    }
}