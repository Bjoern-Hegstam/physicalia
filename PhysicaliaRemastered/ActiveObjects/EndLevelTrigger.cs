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
public class EndLevelTrigger : Pickup
{
    private Sprite _sprite;

    public EndLevelTrigger(Level level, Sprite sprite)
        : base(level)
    {
        _sprite = sprite;
    }

    public override void Update(GameTime gametime)
    {
    }

    public override void DoPickup()
    {
        Level.NextState = LevelState.Finished;
    }

    public override void Draw(SpriteBatch? spriteBatch, Vector2 positionOffset)
    {
        spriteBatch.Draw(_sprite.Texture,
            positionOffset,
            _sprite.SourceRectangle,
            Color.White);
    }
}