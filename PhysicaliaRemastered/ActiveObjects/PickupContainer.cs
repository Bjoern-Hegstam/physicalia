using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Pickups;

namespace PhysicaliaRemastered.ActiveObjects
{
    /// <summary>
    /// Represents a container within a Level that hold a Pickup. The container is
    /// responsible for checking if the contained object should be picked up, as well
    /// as drawing the sprite representation of the pickup.
    /// </summary>
    public class PickupContainer : ActiveObject
    {
        public override ObjectType Type
        {
            get { return ObjectType.Pickup; }
        }
        public PickupContainer(Pickup pickup)
        {
            this.pickup = pickup;
        }

        #region Pickup

        private Pickup pickup;

        public Pickup PickupObject
        {
            get { return this.pickup; }
            set { this.pickup = value; }
        }

        private void Pickup()
        {
            // Deactivate the pickup
            this.IsActive = false;
            this.CanCollide = false;
            this.CanTakeDamage = false;

            // Call the pickup to tell it to do its thing
            this.pickup.DoPickup();
        }

        #endregion

        #region ActiveObject members

        public override void CheckCollision(ICollisionObject collObject)
        {
            // Only check for collision if we're still active
            if (!this.IsActive)
                return;

            // Can only be picked up by player
            if (collObject.Type != ObjectType.Player)
                return;

            // Check to see if the Container is colliding with the Player
            if (CollisionHelper.IsColliding(this, collObject))
                this.Pickup();
        }

        public override void Reset()
        {
            this.pickup.Reset();
        }

        public override void Update(GameTime gametime)
        {
            if (this.Enabled)
                this.pickup.Update(gametime);
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
        {
            // Only draw if we're still active
            if (this.Visible)
                // The pickup is drawn at the upper-left corner of the container
                this.pickup.Draw(spriteBatch, this.Position - this.Origin - offsetPosition);
        }

        #endregion
    }
}
