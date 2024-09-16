using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Pickups;

namespace PhysicaliaRemastered.ActiveObjects;

/// <summary>
/// When an EndLevelTrigger is touched by the player it sets the state of the
/// associated Level to LevelState.Finished.
/// </summary>
public class EndLevelTrigger : Pickup
{
    private Sprite sprite;

    public EndLevelTrigger(Level level, Sprite sprite)
        : base(level)
    {
            this.sprite = sprite;
        }

    public override void Update(GameTime gametime) { }

    public override void DoPickup()
    {
            Level.NextState = LevelState.Finished;
        }

    public override void Draw(SpriteBatch spriteBatch, Vector2 positionOffset)
    {
            spriteBatch.Draw(sprite.Texture,
                            positionOffset,
                            sprite.SourceRectangle,
                            Color.White);
        }
}