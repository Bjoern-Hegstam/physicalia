using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Pickups;
using XNALibrary;
using XNALibrary.Collision;

namespace PhysicaliaRemastered.ActiveObjects;

/// <summary>
/// Represents a container within a Level that hold a Pickup. The container is
/// responsible for checking if the contained object should be picked up, as well
/// as drawing the sprite representation of the pickup.
/// </summary>
public class PickupContainer(Pickup pickup) : ActiveObject
{
    public override ObjectType Type => ObjectType.Pickup;

    public Pickup PickupObject { get; set; } = pickup;

    private void Pickup()
    {
        // Deactivate the pickup
        IsActive = false;
        CanCollide = false;
        CanTakeDamage = false;

        // Call the pickup to tell it to do its thing
        PickupObject.OnPickedUp();
    }

    public override void CheckCollision(ICollisionObject collObject)
    {
        // Only check for collision if we're still active
        if (!IsActive)
        {
            return;
        }

        // Can only be picked up by player
        if (collObject.Type != ObjectType.Player)
        {
            return;
        }

        // Check to see if the Container is colliding with the Player
        if (CollisionHelper.IsColliding(this, collObject))
        {
            Pickup();
        }
    }

    public override void Reset()
    {
        PickupObject.Reset();
    }

    public override void Update(GameTime gameTime)
    {
        if (Enabled)
        {
            PickupObject.Update(gameTime);
        }
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
        if (Visible)
        {
            // The pickup is drawn in the upper-left corner of the container
            PickupObject.Draw(spriteBatch, Position - Origin - offsetPosition);
        }
    }
}