using Microsoft.Xna.Framework;
using XNALibrary.Collision;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.ActiveObjects;

public class Platform : MovingObject
{
    public Platform(SpriteLibrary spriteLibrary, int spriteKey)
        : base(spriteLibrary, spriteKey)
    {
    }

    public Platform(SpriteLibrary spriteLibrary, int spriteKey, Curve curve)
        : this(spriteLibrary, spriteKey, curve, curve)
    {
    }

    public Platform(SpriteLibrary spriteLibrary, int spriteKey, Curve curveX, Curve curveY)
        : base(spriteLibrary, spriteKey, curveX, curveY)
    {
        CanCollide = true;
    }

    public override void CheckCollision(ICollisionObject collObject)
    {
    }
}