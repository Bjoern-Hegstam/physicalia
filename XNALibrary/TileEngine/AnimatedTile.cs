using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.TileEngine;

public class AnimatedTile : Tile
{
    private readonly Animation.Animation _animation;

    public AnimatedTile(Animation.Animation animation)
        : this(animation, Rectangle.Empty, 0)
    {
    }

    public AnimatedTile(Animation.Animation animation, Rectangle collisionBox, BoxSide collisionSides)
        : base(collisionBox, collisionSides)
    {
        _animation = animation;
        _animation.Play();
    }

    public override Rectangle SourceRectangle => _animation.SourceRectangle;

    public override Texture2D Texture => _animation.Texture;
}