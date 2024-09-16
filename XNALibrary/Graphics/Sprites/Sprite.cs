using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Graphics.Sprites;

/// <summary>
/// Struct representing a single Sprite. The sprite contains the source texture
/// to use and a Rectangle marking of the texture to use.
/// </summary>
public struct Sprite
{
    /// <summary>
    /// Texture used by the sprite
    /// </summary>
    private Texture2D texture;

    /// <summary>
    /// The location and size of the sprite on the texture.
    /// </summary>
    private Rectangle sourceRectangle;

    public Texture2D Texture
    {
        get { return this.texture; }
        set { this.texture = value; }
    }

    /// <summary>
    /// Gets and Sets the location and size of the sprite on the texture.
    /// </summary>
    public Rectangle SourceRectangle
    {
        get { return this.sourceRectangle; }
        set { this.sourceRectangle = value; }
    }

    /// <summary>
    /// Creates a new Sprite.
    /// </summary>
    /// <param name="texture">Texture used by the sprite.</param>
    /// <param name="sourceRect">The part of the texture that is the Sprite.</param>
    public Sprite(Texture2D texture, Rectangle sourceRect)
    {
        this.texture = texture;
        this.sourceRectangle = sourceRect;
    }

    /// <summary>
    /// Creates a new Sprite
    /// </summary>
    /// <param name="texture">Texture used by the sprite.</param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public Sprite(Texture2D texture, int x, int y, int width, int height)
        : this(texture, new Rectangle(x, y, width, height)) { }
}