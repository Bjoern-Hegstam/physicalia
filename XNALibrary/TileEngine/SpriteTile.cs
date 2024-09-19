using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Graphics;
using XNALibrary.Sprites;

namespace XNALibrary.TileEngine;

public class SpriteTile(Sprite sprite, Rectangle collisionBox, BoxSide collisionSides)
    : Tile(collisionBox, collisionSides)
{
    public override Rectangle SourceRectangle => sprite.SourceRectangle;

    public override Texture2D Texture => sprite.Texture;

    public SpriteTile(Sprite sprite)
        : this(sprite, Rectangle.Empty, 0)
    {
    }
}