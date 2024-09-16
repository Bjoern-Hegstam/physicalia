using Microsoft.Xna.Framework;

namespace PhysicaliaRemastered.ActiveObjects;

public class Platform : MovingObject
{
    public Platform(ISpriteLibrary spriteLibrary, int spriteKey)
        : base(spriteLibrary, spriteKey){}

    public Platform(ISpriteLibrary spriteLibrary, int spriteKey, Curve curve)
        : this(spriteLibrary, spriteKey, curve, curve) { }

    public Platform(ISpriteLibrary spriteLibrary, int spriteKey, Curve curveX, Curve curveY)
        : base(spriteLibrary, spriteKey, curveX, curveY)
    {
            this.CanCollide = true;
        }

    public override void CheckCollision(ICollisionObject collObject)
    {
            
        }
}