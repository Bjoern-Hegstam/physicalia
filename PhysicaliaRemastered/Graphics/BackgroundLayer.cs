using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Graphics;
using XNALibrary.Graphics.Sprites;

namespace PhysicaliaRemastered.Graphics;

/// <summary>
/// Represents a background layer in the game. The layer has a depth value
/// that dictates how fast the layer moves relative to the player.
/// </summary>
public class BackgroundLayer
{
    private Sprite backgroundSprite;
    private float depthValue;
    private Vector2 position;
    private Vector2 startPosition;

    private bool loopX, loopY;

    public Sprite Background
    {
        get => backgroundSprite;
        set => backgroundSprite = value;
    }

    /// <summary>
    /// Gets or sets the layer's depth value. A value of zero means that the
    /// layer will not move at all, while a value of one has the layer moving
    /// at the same velocity as the player.
    /// </summary>
    public float Depth
    {
        get => depthValue;
        set => depthValue = value;
    }

    public bool LoopX
    {
        get => loopX;
        set => loopX = value;
    }

    public bool LoopY
    {
        get => loopY;
        set => loopY = value;
    }

    public Vector2 Position
    {
        get => position;
        set => position = value;
    }

    public Vector2 StartPosition
    {
        get => startPosition;
        set => startPosition = value;
    }

    public BackgroundLayer(Sprite background, float depthValue)
    {
            backgroundSprite = background;
            this.depthValue = depthValue;
            startPosition = position = Vector2.Zero;
            loopX = loopY = false;
        }

    public void Update(Vector2 positionDelta)
    {
            position += positionDelta * depthValue;
        }

    public void Draw(SpriteBatch spriteBatch)
    {
            spriteBatch.Draw(backgroundSprite.Texture,
                             position,
                             backgroundSprite.SourceRectangle,
                             Color.White);
        }

    public void Draw(SpriteBatch spriteBatch, ScreenSampler screenSampler)
    {
            // Don't draw anything if the background isn't visible
            if (!LoopX &&
                screenSampler.Position.X * depthValue > backgroundSprite.SourceRectangle.Width)
                return;

            if (!LoopY &&
                 screenSampler.Position.Y * depthValue > backgroundSprite.SourceRectangle.Height)
                return;

            // Do a simple draw if no looping is used
            if (!loopX && !loopY)
            {
                Draw(spriteBatch);
                return;
            }

            Vector2 startPos = position;

            while (startPos.X < -backgroundSprite.SourceRectangle.Width)
                startPos.X += backgroundSprite.SourceRectangle.Width;

            while (startPos.Y < -backgroundSprite.SourceRectangle.Height)
                startPos.Y += backgroundSprite.SourceRectangle.Height;

            for (float y = startPos.Y; y < screenSampler.Position.Y + screenSampler.Height; y += backgroundSprite.SourceRectangle.Height)
            {
                for (float x = startPos.X; x < screenSampler.Position.X + screenSampler.Width; x += backgroundSprite.SourceRectangle.Width)
                {
                    spriteBatch.Draw(backgroundSprite.Texture,
                         new Vector2(x, y),
                         backgroundSprite.SourceRectangle,
                         Color.White);

                    if (!loopX)
                        break;
                }

                if (!loopY)
                    break;
            }

        }

    public static int Compare(BackgroundLayer x, BackgroundLayer y)
    {
            if (x.Depth > y.Depth)
                return 1;
            else if (x.Depth < y.Depth)
                return -1;
            return 0;
        }
}