using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhysicaliaRemastered.ActiveObjects;

/// <summary>
/// Base class for active objects such as coins, doors, platforms, etc.
/// </summary>
public abstract class ActiveObject : ICollisionObject
{
    private static int activeObjectCount = 0;
    private int uniqueID;

    private Vector2 position;
    private Vector2 velocity;

    private ISpriteLibrary spriteLibrary;
    private Sprite sprite;

    // Width and Height must not be the same dimensions as the Sprite.
    private int width;
    private int height;

    private Rectangle collisionBox;

    private bool canTakeDamage;
    private bool canCollide;
    private bool enabled;
    private bool visible;

    private List<ActiveObject> childObjects;

    public int UniqueID => uniqueID;

    public bool IsActive
    {
        get => enabled ||visible;
        set => enabled = visible = value;
    }

    public bool Enabled
    {
        get => enabled;
        set => enabled = value;
    }

    public bool Visible
    {
        get => visible;
        set => visible = value;
    }

    public List<ActiveObject> Children => childObjects;

    public virtual ObjectType Type => ObjectType.ActiveObject;

    public virtual Rectangle SourceRectangle => sprite.SourceRectangle;

    public virtual Texture2D Texture => sprite.Texture;

    public Rectangle CollisionBox
    {
        get => collisionBox;
        set => collisionBox = value;
    }

    public bool CanCollide
    {
        get => canCollide;
        set => canCollide = value;
    }

    public bool CanTakeDamage
    {
        get => canTakeDamage;
        set => canTakeDamage = value;
    }

    public int Width
    {
        get => width;
        set => width = value;
    }

    public int Height
    {
        get => height;
        set => height = value;
    }

    public Vector2 Origin =>
        new(collisionBox.Width / 2,
            collisionBox.Height / 2);

    public virtual Vector2 Position
    {
        get => position;
        set => position = value;
    }

    public Vector2 Velocity
    {
        get => velocity;
        set => velocity = value;
    }

    public virtual void TakeDamage(float damageLevel) { }
    public virtual void OnCollision(ICollisionObject collidedObject, BoxSide collisionSides, Vector2 position, Vector2 velocity) { }

    public ActiveObject()
    {
            uniqueID = activeObjectCount++;

            childObjects = new List<ActiveObject>();
            width = sprite.SourceRectangle.Width;
            height = sprite.SourceRectangle.Height;

            velocity = position = Vector2.Zero;

            IsActive = false;
            canCollide = true;
            canTakeDamage = false;
        }

    public ActiveObject(ISpriteLibrary spriteLibrary, int spriteKey)
        : this()
    {
            this.spriteLibrary = spriteLibrary;
            sprite = this.spriteLibrary.GetSprite(spriteKey);

        }

    public void CheckCollisions(ICollisionObject[] collObjects)
    {
            if (canCollide)
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
    public virtual void Reset() { }

    public abstract void Update(GameTime gametime);

    public virtual void Draw(SpriteBatch spriteBatch)
    {
            Draw(spriteBatch, Vector2.Zero);
        }

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
            if (visible && spriteLibrary != null)
                spriteBatch.Draw(sprite.Texture,
                                 position - offsetPosition,
                                 sprite.SourceRectangle,
                                 Color.White,
                                 0F,
                                 Origin,
                                 1.0F,
                                 SpriteEffects.None,
                                 0.8F);
        }
}