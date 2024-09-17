using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Sprites;

namespace XNALibrary.TileEngine;

public class SpriteTile : Tile
{
    /// <summary>
    /// Sprite used by the tile.
    /// </summary>
    private Sprite _sprite;

    public SpriteTile(Sprite sprite)
        : this(sprite, Rectangle.Empty, 0)
    {
    }

    public SpriteTile(Sprite sprite, Rectangle collisionBox, BoxSide collisionSides)
        : base(collisionBox, collisionSides)
    {
        _sprite = sprite;
    }

    public override Rectangle SourceRectangle => _sprite.SourceRectangle;

    public override Texture2D Texture => _sprite.Texture;
}