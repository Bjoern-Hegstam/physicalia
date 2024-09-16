using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhysicaliaRemastered.ActiveObjects
{
    public class MovingObject : ActiveObject
    {
        private Curve curveX, curveY;
        private Vector2 curveScale;
        private Vector2 curvePosition;
        private Vector2 positionOffset;

        public Vector2 Scale
        {
            get { return this.curveScale; }
            set { this.curveScale = value; }
        }

        public float ScaleX
        {
            get { return this.curveScale.X; }
            set { this.curveScale.X = value; }
        }

        public float ScaleY
        {
            get { return this.curveScale.Y; }
            set { this.curveScale.Y = value; }
        }

        #region ActiveObjects members

        public override Vector2 Position
        {
            get { return base.Position - this.positionOffset; }
            set { base.Position = value; }
        }

        #endregion

        public MovingObject(ISpriteLibrary spriteLibrary, int spriteKey)
            : this( spriteLibrary, spriteKey, new Curve()) { }

        public MovingObject(ISpriteLibrary spriteLibrary, int spriteKey, Curve curve)
            : this( spriteLibrary, spriteKey, curve, curve) { }

        public MovingObject(ISpriteLibrary spriteLibrary, int spriteKey, Curve curveX, Curve curveY)
            : base(spriteLibrary, spriteKey)
        {
            this.curveX = curveX;
            this.curveY = curveY;
            this.curvePosition = Vector2.Zero;
            this.curveScale = new Vector2(1F);
        }

        public override void CheckCollision(ICollisionObject collObject) { }

        public override void Reset()
        {
            this.curvePosition = Vector2.Zero;
            this.positionOffset.X = this.curveX.Evaluate(this.curvePosition.X);
            this.positionOffset.Y = this.curveY.Evaluate(this.curvePosition.Y);
        }

        public override void Update(GameTime gametime)
        {
            if (!this.IsActive)
                return;

            // Update the curve position
            this.curvePosition += this.Velocity * (float)gametime.ElapsedGameTime.TotalSeconds;

            // Update position when we're sure our curve position is valid
            this.positionOffset = new Vector2(this.curveX.Evaluate(this.curvePosition.X) * this.curveScale.X,
                                             this.curveY.Evaluate(this.curvePosition.Y) * this.curveScale.Y);
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
        {
            base.Draw(spriteBatch, offsetPosition + this.positionOffset);
        }
    }
}
