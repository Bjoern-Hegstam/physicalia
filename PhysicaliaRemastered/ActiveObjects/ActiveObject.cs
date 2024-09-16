using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhysicaliaRemastered.ActiveObjects
{
    /// <summary>
    /// Base class for active objects such as coins, doors, platforms, etc.
    /// </summary>
    public abstract class ActiveObject : ICollisionObject
    {
        #region Fields

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

        #endregion

        #region Properties

        public int UniqueID
        {
            get { return this.uniqueID; }
        }

        public bool IsActive
        {
            get { return this.enabled ||this.visible; }
            set { this.enabled = this.visible = value; }
        }

        public bool Enabled
        {
            get { return this.enabled; }
            set { this.enabled = value; }
        }

        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        public List<ActiveObject> Children
        {
            get { return this.childObjects; }
        }

        #endregion

        #region ICollisionObject Members

        public virtual ObjectType Type
        {
            get { return ObjectType.ActiveObject; }
        }

        public virtual Rectangle SourceRectangle
        {
            get { return this.sprite.SourceRectangle; }
        }

        public virtual Texture2D Texture
        {
            get { return this.sprite.Texture; }
        }

        public Rectangle CollisionBox
        {
            get { return this.collisionBox; }
            set { this.collisionBox = value; }
        }

        public bool CanCollide
        {
            get { return this.canCollide; }
            set { this.canCollide = value; }
        }

        public bool CanTakeDamage
        {
            get { return this.canTakeDamage; }
            set { this.canTakeDamage = value; }
        }

        public int Width
        {
            get { return this.width; }
            set { this.width = value; }
        }

        public int Height
        {
            get { return this.height; }
            set { this.height = value; }
        }

        public Vector2 Origin
        {
            get
            {
                return new Vector2(this.collisionBox.Width / 2,
                                   this.collisionBox.Height / 2);
            }
        }

        public virtual Vector2 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        public Vector2 Velocity
        {
            get { return this.velocity; }
            set { this.velocity = value; }
        }

        public virtual void TakeDamage(float damageLevel) { }
        public virtual void OnCollision(ICollisionObject collidedObject, BoxSide collisionSides, Vector2 position, Vector2 velocity) { }

        #endregion

        public ActiveObject()
        {
            this.uniqueID = ActiveObject.activeObjectCount++;

            this.childObjects = new List<ActiveObject>();
            this.width = this.sprite.SourceRectangle.Width;
            this.height = this.sprite.SourceRectangle.Height;

            this.velocity = this.position = Vector2.Zero;

            this.IsActive = false;
            this.canCollide = true;
            this.canTakeDamage = false;
        }

        public ActiveObject(ISpriteLibrary spriteLibrary, int spriteKey)
            : this()
        {
            this.spriteLibrary = spriteLibrary;
            this.sprite = this.spriteLibrary.GetSprite(spriteKey);

        }

        public void CheckCollisions(ICollisionObject[] collObjects)
        {
            if (this.canCollide)
            {
                for (int i = 0; i < collObjects.Length; i++)
                {
                    this.CheckCollision(collObjects[i]);
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
            this.Draw(spriteBatch, Vector2.Zero);
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
        {
            if (this.visible && this.spriteLibrary != null)
                spriteBatch.Draw(sprite.Texture,
                                 this.position - offsetPosition,
                                 this.sprite.SourceRectangle,
                                 Color.White,
                                 0F,
                                 this.Origin,
                                 1.0F,
                                 SpriteEffects.None,
                                 0.8F);
        }
    }
}
