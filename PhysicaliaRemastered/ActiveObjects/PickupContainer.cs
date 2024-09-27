using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Actors;
using PhysicaliaRemastered.Pickups;
using XNALibrary.Collision;
using XNALibrary.Graphics;

namespace PhysicaliaRemastered.ActiveObjects;

/// <summary>
/// Represents a container within a Level that hold a Pickup. The container is
/// responsible for checking if the contained object should be picked up, as well
/// as drawing the sprite representation of the pickup.
/// </summary>
public class PickupContainer(Pickup pickup) : ActiveObject
{
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

    public override void CheckCollision(ICollidable collObject)
    {
        if (!IsActive)
        {
            return;
        }

        if (collObject is not Player)
        {
            return;
        }

        if (WorldCollisionBox.Intersects(collObject.WorldCollisionBox))
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

    public override void Draw(SpriteBatch spriteBatch, Vector2 viewportPosition)
    {
        if (!Visible)
        {
            return;
        }

        PickupObject.Draw(spriteBatch, Position - viewportPosition);

#if DEBUG
        spriteBatch.DrawRectangle(WorldCollisionBox, Color.Red, viewportPosition);
#endif
    }
}