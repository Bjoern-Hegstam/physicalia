using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.GameManagement;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.Pickups;

public abstract class Pickup
{
    public bool PickedUp { get; set; }

    public Level Level { get; set; }

    public Sprite Sprite { get; set; }

    public int Id { get; set; }

    public Pickup(Level level)
    {
        Level = level;
        PickedUp = false;
    }

    public Pickup Copy()
    {
        Pickup pickup = MemberwiseClone() as Pickup;

        return pickup;
    }

    public virtual void Update(GameTime gameTime)
    {
    }

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
        PickedUp = false;
    }

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 positionOffset)
    {
        spriteBatch.Draw(Sprite.Texture, positionOffset, Sprite.SourceRectangle, Color.White);
    }
}