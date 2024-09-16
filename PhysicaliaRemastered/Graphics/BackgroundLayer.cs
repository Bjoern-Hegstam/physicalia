using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        get { return this.backgroundSprite; }
        set { this.backgroundSprite = value; }
    }

    /// <summary>
    /// Gets or sets the layer's depth value. A value of zero means that the
    /// layer will not move at all, while a value of one has the layer moving
    /// at the same velocity as the player.
    /// </summary>
    public float Depth
    {
        get { return this.depthValue; }
        set { this.depthValue = value; }
    }

    public bool LoopX
    {
        get { return this.loopX; }
        set { this.loopX = value; }
    }

    public bool LoopY
    {
        get { return this.loopY; }
        set { this.loopY = value; }
    }

    public Vector2 Position
    {
        get { return this.position; }
        set { this.position = value; }
    }

    public Vector2 StartPosition
    {
        get { return this.startPosition; }
        set { this.startPosition = value; }
    }

    public BackgroundLayer(Sprite background, float depthValue)
    {
            this.backgroundSprite = background;
            this.depthValue = depthValue;
            this.startPosition = this.position = Vector2.Zero;
            this.loopX = this.loopY = false;
        }

    public void Update(Vector2 positionDelta)
    {
            this.position += positionDelta * this.depthValue;
        }

    public void Draw(SpriteBatch spriteBatch)
    {
            spriteBatch.Draw(this.backgroundSprite.Texture,
                             this.position,
                             this.backgroundSprite.SourceRectangle,
                             Color.White);
        }

    public void Draw(SpriteBatch spriteBatch, ScreenSampler screenSampler)
    {
            // Don't draw anything if the background isn't visible
            if (!this.LoopX &&
                screenSampler.Position.X * this.depthValue > this.backgroundSprite.SourceRectangle.Width)
                return;

            if (!this.LoopY &&
                 screenSampler.Position.Y * this.depthValue > this.backgroundSprite.SourceRectangle.Height)
                return;

            // Do a simple draw if no looping is used
            if (!this.loopX && !this.loopY)
            {
                this.Draw(spriteBatch);
                return;
            }

            Vector2 startPos = this.position;

            while (startPos.X < -this.backgroundSprite.SourceRectangle.Width)
                startPos.X += this.backgroundSprite.SourceRectangle.Width;

            while (startPos.Y < -this.backgroundSprite.SourceRectangle.Height)
                startPos.Y += this.backgroundSprite.SourceRectangle.Height;

            for (float y = startPos.Y; y < screenSampler.Position.Y + screenSampler.Height; y += this.backgroundSprite.SourceRectangle.Height)
            {
                for (float x = startPos.X; x < screenSampler.Position.X + screenSampler.Width; x += this.backgroundSprite.SourceRectangle.Width)
                {
                    spriteBatch.Draw(this.backgroundSprite.Texture,
                         new Vector2(x, y),
                         this.backgroundSprite.SourceRectangle,
                         Color.White);

                    if (!this.loopX)
                        break;
                }

                if (!this.loopY)
                    break;
            }

        }

    #region IComparer<BackgroundLayer> Members

    public static int Compare(BackgroundLayer x, BackgroundLayer y)
    {
            if (x.Depth > y.Depth)
                return 1;
            else if (x.Depth < y.Depth)
                return -1;
            return 0;
        }

    #endregion
}