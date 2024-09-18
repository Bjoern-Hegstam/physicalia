using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.GameManagement;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.Pickups;

public abstract class Pickup(Level level)
{
    public bool PickedUp { get; set; }

    public Level Level { get; set; } = level;

    public Sprite Sprite { get; set; }

    public PickupTemplateId TemplateId { get; set; }

    public Pickup Copy()
    {
        return MemberwiseClone() as Pickup;
    }

    public virtual void Update(GameTime gameTime)
    {
    }

    /// <summary>
    /// When overriden in a derived class, takes actions needed when being
    /// picked up.
    /// </summary>
    public abstract void OnPickedUp();

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