using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Collision;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.ActiveObjects;

public class MovingObject : ActiveObject
{
    private readonly Curve _curveX;
    private readonly Curve _curveY;
    private Vector2 _curveScale;
    private Vector2 _curvePosition;
    private Vector2 _positionOffset;

    public Vector2 Scale
    {
        get => _curveScale;
        set => _curveScale = value;
    }

    public float ScaleX
    {
        get => _curveScale.X;
        set => _curveScale.X = value;
    }

    public float ScaleY
    {
        get => _curveScale.Y;
        set => _curveScale.Y = value;
    }

    public override Vector2 Position
    {
        get => base.Position - _positionOffset;
        set => base.Position = value;
    }

    public MovingObject(SpriteLibrary spriteLibrary, int spriteKey)
        : this(spriteLibrary, spriteKey, new Curve())
    {
    }

    public MovingObject(SpriteLibrary spriteLibrary, int spriteKey, Curve curve)
        : this(spriteLibrary, spriteKey, curve, curve)
    {
    }

    public MovingObject(SpriteLibrary spriteLibrary, int spriteKey, Curve curveX, Curve curveY)
        : base(spriteLibrary, spriteKey)
    {
        _curveX = curveX;
        _curveY = curveY;
        _curvePosition = Vector2.Zero;
        _curveScale = new Vector2(1F);
    }

    public override void CheckCollision(ICollisionObject collObject)
    {
    }

    public override void Reset()
    {
        _curvePosition = Vector2.Zero;
        _positionOffset.X = _curveX.Evaluate(_curvePosition.X);
        _positionOffset.Y = _curveY.Evaluate(_curvePosition.Y);
    }

    public override void Update(GameTime gametime)
    {
        if (!IsActive)
        {
            return;
        }

        // Update the curve position
        _curvePosition += Velocity * (float)gametime.ElapsedGameTime.TotalSeconds;

        // Update position when we're sure our curve position is valid
        _positionOffset = new Vector2(_curveX.Evaluate(_curvePosition.X) * _curveScale.X,
            _curveY.Evaluate(_curvePosition.Y) * _curveScale.Y);
    }

    public override void Draw(SpriteBatch? spriteBatch, Vector2 offsetPosition)
    {
        base.Draw(spriteBatch, offsetPosition + _positionOffset);
    }
}