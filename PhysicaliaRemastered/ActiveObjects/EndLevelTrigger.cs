using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Pickups;

namespace PhysicaliaRemastered.ActiveObjects
{
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

        #region Pickup members

        public override void DoPickup()
        {
            this.Level.NextState = LevelState.Finished;
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 positionOffset)
        {
            spriteBatch.Draw(this.sprite.Texture,
                            positionOffset,
                            this.sprite.SourceRectangle,
                            Color.White);
        }

        #endregion
    }
}
