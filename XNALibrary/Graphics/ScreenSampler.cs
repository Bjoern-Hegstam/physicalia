using Microsoft.Xna.Framework;
using XNALibrary.Interfaces;

namespace XNALibrary.Graphics;

public class ScreenSampler : IScreenSampler
{
    /// <summary>
    /// Current position of the sampler.
    /// </summary>
    private Vector2 position;

    /// <summary>
    /// The maximum allowed with of the sampler.
    /// </summary>
    private int maxWidth;

    /// <summary>
    /// The maximum allowed height of the sampler.
    /// </summary>
    private int maxHeight;

    /// <summary>
    /// The screen rectangle.
    /// </summary>
    private Rectangle screen;

    /// <summary>
    /// Gets and Sets the position of the sampler.
    /// </summary>
    public Vector2 Position
    {
        get { return position; }
        set
        {
            position.X = MathHelper.Clamp(value.X, 0, maxWidth - screen.Width);
            position.Y = MathHelper.Clamp(value.Y, 0, maxHeight - screen.Height);
            screen.X = (int)position.X;
            screen.Y = (int)position.Y;
        }
    }

    /// <summary>
    /// Gets and Sets the width if the sampler.
    /// </summary>
    public int Width
    {
        get { return screen.Width; }
        set { screen.Width = (int)MathHelper.Clamp(value, 0, maxWidth); }
    }

    /// <summary>
    /// Gets and Sets the height of the sampler.
    /// </summary>
    public int Height
    {
        get { return screen.Height; }
        set { screen.Height = (int)MathHelper.Clamp(value, 0, maxHeight); }
    }

    public Rectangle ScreenRectangle
    {
        get { return screen; }
    }

    /// <summary>
    /// Gets and Sets the maximal allowed width of the sampler.
    /// </summary>
    public int MaxWidth
    {
        get { return maxWidth; }
        set { maxWidth = value; }
    }

    /// <summary>
    /// Gets and Sets the maximal allowed height of the sampler.
    /// </summary>
    public int MaxHeight
    {
        get { return maxHeight; }
        set { maxHeight = value; }
    }

    public ScreenSampler(Game game, int x, int y, int width, int height)
        : this(game, new Rectangle(x, y, width, height)) { }

    public ScreenSampler(Game game, Rectangle screenRectangle)
    {
        // Add the sampler as a service.
        //game.Services.AddService(typeof(IScreenSampler), this);
        // TODO: Look over whether the sample should really be a service
        // Set the screen rectangle and the position of the sampler
        screen = screenRectangle;
        position = new Vector2(screen.X, screen.Y);
        maxWidth = screen.Width;
        maxHeight = screen.Height;
    }

    public ScreenSampler(Rectangle screenRectangle)
    {
        // Set the screen rectangle and the position of the sampler
        screen = screenRectangle;
        position = new Vector2(screen.X, screen.Y);
        maxWidth = screen.Width;
        maxHeight = screen.Height;
    }

    /// <summary>
    /// Checks if a specified rectangle is within the screen area.
    /// </summary>
    /// <param name="boundingBox">Rectangle to control.</param>
    /// <returns>True if the rectangle is on screen; false otherwise.</returns>
    public bool IsOnScreen(Rectangle boundingBox)
    {
        return screen.Intersects(boundingBox);
    }

    /// <summary>
    /// Checks if a specified rectangle is within the screen area.
    /// </summary>
    /// <param name="x">X-position of the rectangle.</param>
    /// <param name="y">Y-position of the rectangle.</param>
    /// <param name="width">Width of the rectangle.</param>
    /// <param name="height">Height of the rectangle.</param>
    /// <returns>True if the rectangle is on screen; false otherwise.</returns>
    public bool IsOnScreen(int x, int y, int width, int height)
    {
        return x < screen.Right &&
               y < screen.Bottom &&
               (x + width) > screen.Left &&
               (y + height) > screen.Top;
    }
}