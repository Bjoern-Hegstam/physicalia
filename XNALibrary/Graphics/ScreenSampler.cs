using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNALibrary.Graphics;

public class ScreenSampler : XNALibrary.Services.IScreenSampler
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
        get { return this.position; }
        set
        {
            this.position.X = MathHelper.Clamp(value.X, 0, this.maxWidth - this.screen.Width);
            this.position.Y = MathHelper.Clamp(value.Y, 0, this.maxHeight - this.screen.Height);
            this.screen.X = (int)this.position.X;
            this.screen.Y = (int)this.position.Y;
        }
    }

    /// <summary>
    /// Gets and Sets the width if the sampler.
    /// </summary>
    public int Width
    {
        get { return this.screen.Width; }
        set { this.screen.Width = (int)MathHelper.Clamp(value, 0, this.maxWidth); }
    }

    /// <summary>
    /// Gets and Sets the height of the sampler.
    /// </summary>
    public int Height
    {
        get { return this.screen.Height; }
        set { this.screen.Height = (int)MathHelper.Clamp(value, 0, this.maxHeight); }
    }

    public Rectangle ScreenRectangle
    {
        get { return this.screen; }
    }

    /// <summary>
    /// Gets and Sets the maximal allowed width of the sampler.
    /// </summary>
    public int MaxWidth
    {
        get { return this.maxWidth; }
        set { this.maxWidth = value; }
    }

    /// <summary>
    /// Gets and Sets the maximal allowed height of the sampler.
    /// </summary>
    public int MaxHeight
    {
        get { return this.maxHeight; }
        set { this.maxHeight = value; }
    }

    public ScreenSampler(Game game, int x, int y, int width, int height)
        : this(game, new Rectangle(x, y, width, height)) { }

    public ScreenSampler(Game game, Rectangle screenRectangle)
    {
        // Add the sampler as a service.
        //game.Services.AddService(typeof(XNALibrary.Services.IScreenSampler), this);
        // TODO: Look over whether the sample should really be a service
        // Set the screen rectangle and the position of the sampler
        this.screen = screenRectangle;
        this.position = new Vector2(this.screen.X, this.screen.Y);
        this.maxWidth = this.screen.Width;
        this.maxHeight = this.screen.Height;
    }

    public ScreenSampler(Rectangle screenRectangle)
    {
        // Set the screen rectangle and the position of the sampler
        this.screen = screenRectangle;
        this.position = new Vector2(this.screen.X, this.screen.Y);
        this.maxWidth = this.screen.Width;
        this.maxHeight = this.screen.Height;
    }

    /// <summary>
    /// Checks if a specified rectangle is within the screen area.
    /// </summary>
    /// <param name="boundingBox">Rectangle to control.</param>
    /// <returns>True if the rectangle is on screen; false otherwise.</returns>
    public bool IsOnScreen(Rectangle boundingBox)
    {
        return this.screen.Intersects(boundingBox);
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
        return x < this.screen.Right &&
               y < this.screen.Bottom &&
               (x + width) > this.screen.Left &&
               (y + height) > this.screen.Top;
    }
}