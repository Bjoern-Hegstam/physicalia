using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Collision;
using XNALibrary.Graphics;

namespace XNALibrary.TileEngine;

public class Tile : ICollidable
{
    public required TileDefinition TileDefinition { get; init; }
    public required Vector2 Position { get; init; }

    public Rectangle CollisionBox => TileDefinition.CollisionBox;

    public Rectangle AbsoluteCollisionBox => new(
        Position.ToPoint() + CollisionBox.Location,
        CollisionBox.Size
    );

    public List<BoxSide> CollisionSides => TileDefinition.CollisionSides;

    public Vector2 Velocity { get; } = Vector2.Zero;

    public bool CanCollide => CollisionSides.Count > 0;

    public bool CanTakeDamage => false;

    public virtual void OnCollision(ICollidable collidedObject, List<BoxSide> collidedSides,
        Vector2 suggestedNewPosition,
        Vector2 suggestedNewVelocity)
    {
    }

    public virtual void TakeDamage(float damageLevel)
    {
    }
    
    public void Draw(SpriteBatch spriteBatch, Vector2 viewportPosition)
    {
        spriteBatch.Draw(
            TileDefinition.Sprite.Texture,
            Position - viewportPosition,
            TileDefinition.Sprite.SourceRectangle,
            Color.White
        );

#if DEBUG
        spriteBatch.DrawRectangle(AbsoluteCollisionBox, Color.Red);
#endif
    }
}