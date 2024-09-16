using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhysicaliaRemastered.Pickups
{
    public abstract class Pickup
    {
        #region Fields and Properties

        private bool pickedUp;

        public bool PickedUp
        {
            get { return this.pickedUp; }
            set { this.pickedUp = value; }
        }

        private Level level;

        public Level Level
        {
            get { return this.level; }
            set { this.level = value; }
        }

        private Sprite sprite;

        public Sprite Sprite
        {
            get { return this.sprite; }
            set { this.sprite = value; }
        }

        private int id;

        public int ID
        {
            get { return this.id; }
            set { this.id = value; }
        }

        #endregion

        public Pickup(Level level)
        {
            this.level = level;
            this.pickedUp = false;
        }

        public Pickup Copy()
        {
            Pickup pickup = this.MemberwiseClone() as Pickup;

            return pickup;
        }

        public virtual void Update(GameTime gameTime) { }

        /// <summary>
        /// When overriden in a derived class, takes actions needed when being
        /// picked up.
        /// </summary>
        public abstract void DoPickup();

        /// <summary>
        /// When overriden in a derived class, resets the state of the Pickup.
        /// </summary>
        public virtual void Reset()
        {
            this.pickedUp = false;
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 positionOffset)
        {
            spriteBatch.Draw(this.Sprite.Texture, positionOffset, this.Sprite.SourceRectangle, Color.White);
        }
    }
}
