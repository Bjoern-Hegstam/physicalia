using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Graphics.TileEngine;

public class AnimatedTile : Tile
{
    private Animation.Animation animation;

    public AnimatedTile(Animation.Animation animation)
        : this(animation, Rectangle.Empty, 0){}

    public AnimatedTile(Animation.Animation animation, Rectangle collisionBox, BoxSide collisionSides)
        : base(collisionBox, collisionSides)
    {
        this.animation = animation;
        this.animation.Play();
    }

    public override Rectangle SourceRectangle
    {
        get { return animation.SourceRectangle; }
    }

    public override Texture2D Texture
    {
        get { return animation.Texture; }
    }
}