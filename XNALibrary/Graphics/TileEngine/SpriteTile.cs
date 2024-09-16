using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Graphics.TileEngine;

public class SpriteTile : Tile
{
    /// <summary>
    /// Sprite used by the tile.
    /// </summary>
    private Sprite sprite;

    public SpriteTile(Sprite sprite)
        : this(sprite, Rectangle.Empty, 0){}

    public SpriteTile(Sprite sprite, Rectangle collisionBox, BoxSide collisionSides)
        : base(collisionBox, collisionSides)
    {
            this.sprite = sprite;
        }

    public override Rectangle SourceRectangle
    {
        get { return this.sprite.SourceRectangle; }
    }

    public override Texture2D Texture
    {
        get { return this.sprite.Texture; }
    }
}