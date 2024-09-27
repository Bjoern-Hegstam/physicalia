using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Collision;

namespace XNALibrary.TileEngine;

public class Tile : ICollidable
{
    public required TileDefinition TileDefinition { get; init; }
    public required Vector2 Position { get; init; }

    public Rectangle CollisionBoxDefinition => TileDefinition.CollisionBox;

    public Rectangle WorldCollisionBox => new(
        Position.ToPoint() + CollisionBoxDefinition.Location,
        CollisionBoxDefinition.Size
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
        // Explicit, consistent conversion to int to prevent flickering
        var drawPosition = (Position - viewportPosition).ToPoint().ToVector2();

        spriteBatch.Draw(
            TileDefinition.Sprite.Texture,
            drawPosition,
            TileDefinition.Sprite.SourceRectangle,
            Color.White
        );

#if DEBUG
        spriteBatch.DrawRectangle(WorldCollisionBox, CanCollide ? Color.Red : Color.Gray, viewportPosition);
#endif
    }
}