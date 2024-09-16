using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhysicaliaRemastered.ActiveObjects;

public class MovingObject : ActiveObject
{
    private Curve curveX, curveY;
    private Vector2 curveScale;
    private Vector2 curvePosition;
    private Vector2 positionOffset;

    public Vector2 Scale
    {
        get => curveScale;
        set => curveScale = value;
    }

    public float ScaleX
    {
        get => curveScale.X;
        set => curveScale.X = value;
    }

    public float ScaleY
    {
        get => curveScale.Y;
        set => curveScale.Y = value;
    }

    public override Vector2 Position
    {
        get => base.Position - positionOffset;
        set => base.Position = value;
    }

    public MovingObject(ISpriteLibrary spriteLibrary, int spriteKey)
        : this( spriteLibrary, spriteKey, new Curve()) { }

    public MovingObject(ISpriteLibrary spriteLibrary, int spriteKey, Curve curve)
        : this( spriteLibrary, spriteKey, curve, curve) { }

    public MovingObject(ISpriteLibrary spriteLibrary, int spriteKey, Curve curveX, Curve curveY)
        : base(spriteLibrary, spriteKey)
    {
            this.curveX = curveX;
            this.curveY = curveY;
            curvePosition = Vector2.Zero;
            curveScale = new Vector2(1F);
        }

    public override void CheckCollision(ICollisionObject collObject) { }

    public override void Reset()
    {
            curvePosition = Vector2.Zero;
            positionOffset.X = curveX.Evaluate(curvePosition.X);
            positionOffset.Y = curveY.Evaluate(curvePosition.Y);
        }

    public override void Update(GameTime gametime)
    {
            if (!IsActive)
                return;

            // Update the curve position
            curvePosition += Velocity * (float)gametime.ElapsedGameTime.TotalSeconds;

            // Update position when we're sure our curve position is valid
            positionOffset = new Vector2(curveX.Evaluate(curvePosition.X) * curveScale.X,
                                             curveY.Evaluate(curvePosition.Y) * curveScale.Y);
        }

    public override void Draw(SpriteBatch spriteBatch, Vector2 offsetPosition)
    {
            base.Draw(spriteBatch, offsetPosition + positionOffset);
        }
}