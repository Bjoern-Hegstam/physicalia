using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Physicalia;

namespace PhysicaliaRemastered.Actors.Bosses
{
    public abstract class Boss
    {
        #region Fields

        private float health;

        #endregion

        #region Properties

        public float Health
        {
            get { return this.health; }
            set { this.health = value; }
        }

        #endregion

        #region Constructor

        public Boss()
        {

        }

        #endregion

        public abstract void LoadContent(ISpriteLibrary spriteLibrary);

        public abstract void CheckCollisions(Player player, Particle[] particles);

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch, Vector2 positionOffset);
    }
}
