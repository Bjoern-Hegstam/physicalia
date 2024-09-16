using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.GameManagement;
using XNALibrary.Graphics.Sprites;

namespace PhysicaliaRemastered.Pickups;

public abstract class Pickup
{
    private bool pickedUp;

    public bool PickedUp
    {
        get => pickedUp;
        set => pickedUp = value;
    }

    private Level level;

    public Level Level
    {
        get => level;
        set => level = value;
    }

    private Sprite sprite;

    public Sprite Sprite
    {
        get => sprite;
        set => sprite = value;
    }

    private int id;

    public int ID
    {
        get => id;
        set => id = value;
    }

    public Pickup(Level level)
    {
            this.level = level;
            pickedUp = false;
        }

    public Pickup Copy()
    {
            Pickup pickup = MemberwiseClone() as Pickup;

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
            pickedUp = false;
        }

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 positionOffset)
    {
            spriteBatch.Draw(Sprite.Texture, positionOffset, Sprite.SourceRectangle, Color.White);
        }
}