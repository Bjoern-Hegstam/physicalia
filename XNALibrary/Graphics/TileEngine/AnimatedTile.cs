using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Graphics.TileEngine;

public class AnimatedTile : Tile
{
    private Animation animation;

    public AnimatedTile(Animation animation)
        : this(animation, Rectangle.Empty, 0){}

    public AnimatedTile(Animation animation, Rectangle collisionBox, BoxSide collisionSides)
        : base(collisionBox, collisionSides)
    {
        this.animation = animation;
        this.animation.Play();
    }

    public override Rectangle SourceRectangle
    {
        get { return this.animation.SourceRectangle; }
    }

    public override Texture2D Texture
    {
        get { return this.animation.Texture; }
    }
}