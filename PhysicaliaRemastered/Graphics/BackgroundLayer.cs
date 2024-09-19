using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Sprites;
using Viewport = XNALibrary.Graphics.Viewport;

namespace PhysicaliaRemastered.Graphics;

/// <summary>
/// Represents a background layer in the game. The layer has a depth value
/// that dictates how fast the layer moves relative to the player.
/// </summary>
public class BackgroundLayer
{
    private readonly Sprite _backgroundSprite;

    /// <summary>
    /// Gets or sets the layer's depth value. A value of zero means that the
    /// layer will not move at all, while a value of one has the layer moving
    /// at the same velocity as the player.
    /// </summary>
    public float Depth { get; set; }

    public bool LoopX { get; set; }

    public bool LoopY { get; set; }

    public Vector2 Position { get; set; }

    public Vector2 StartPosition { get; set; }

    public BackgroundLayer(Sprite background, float depthValue)
    {
        _backgroundSprite = background;
        Depth = depthValue;
        StartPosition = Position = Vector2.Zero;
        LoopX = LoopY = false;
    }

    public void Update(Vector2 positionDelta)
    {
        Position += positionDelta * Depth;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _backgroundSprite.Texture,
            Position,
            _backgroundSprite.SourceRectangle,
            Color.White
        );
    }

    public void Draw(SpriteBatch spriteBatch, Viewport viewport)
    {
        // Don't draw anything if the background isn't visible
        if (!LoopX && viewport.Position.X * Depth > _backgroundSprite.SourceRectangle.Width)
        {
            return;
        }

        if (!LoopY && viewport.Position.Y * Depth > _backgroundSprite.SourceRectangle.Height)
        {
            return;
        }

        // Do a simple draw if no looping is used
        if (!LoopX && !LoopY)
        {
            Draw(spriteBatch);
            return;
        }

        Vector2 startPos = Position;

        while (startPos.X < -_backgroundSprite.SourceRectangle.Width)
        {
            startPos.X += _backgroundSprite.SourceRectangle.Width;
        }

        while (startPos.Y < -_backgroundSprite.SourceRectangle.Height)
        {
            startPos.Y += _backgroundSprite.SourceRectangle.Height;
        }

        for (float y = startPos.Y;
             y < viewport.Position.Y + viewport.Height;
             y += _backgroundSprite.SourceRectangle.Height)
        {
            for (float x = startPos.X;
                 x < viewport.Position.X + viewport.Width;
                 x += _backgroundSprite.SourceRectangle.Width)
            {
                spriteBatch.Draw(
                    _backgroundSprite.Texture,
                    new Vector2(x, y),
                    _backgroundSprite.SourceRectangle,
                    Color.White
                );

                if (!LoopX)
                {
                    break;
                }
            }

            if (!LoopY)
            {
                break;
            }
        }
    }

    public static int Compare(BackgroundLayer x, BackgroundLayer y)
    {
        if (x.Depth > y.Depth)
        {
            return 1;
        }

        if (x.Depth < y.Depth)
        {
            return -1;
        }

        return 0;
    }
}