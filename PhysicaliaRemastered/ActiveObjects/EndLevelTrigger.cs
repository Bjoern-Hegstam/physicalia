using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.GameManagement;
using PhysicaliaRemastered.Pickups;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.ActiveObjects;

/// <summary>
/// When an EndLevelTrigger is touched by the player it sets the state of the
/// associated Level to LevelState.Finished.
/// </summary>
public class EndLevelTrigger(Level level, Sprite sprite) : Pickup(level)
{
    public override void Update(GameTime gameTime)
    {
    }

    public override void OnPickedUp()
    {
        Level.NextState = LevelState.Finished;
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 viewportPosition)
    {
        spriteBatch.Draw(sprite.Texture,
            viewportPosition,
            sprite.SourceRectangle,
            Color.White);
    }
}