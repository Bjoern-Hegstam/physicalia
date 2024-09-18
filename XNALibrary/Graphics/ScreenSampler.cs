using Microsoft.Xna.Framework;

namespace XNALibrary.Graphics;

public class ScreenSampler
{
    /// <summary>
    /// Current position of the sampler.
    /// </summary>
    private Vector2 _position;

    /// <summary>
    /// The maximum allowed with of the sampler.
    /// </summary>
    private int _maxWidth;

    /// <summary>
    /// The maximum allowed height of the sampler.
    /// </summary>
    private int _maxHeight;

    /// <summary>
    /// The screen rectangle.
    /// </summary>
    private Rectangle _screen;

    /// <summary>
    /// Gets and Sets the position of the sampler.
    /// </summary>
    public Vector2 Position
    {
        get => _position;
        set
        {
            _position.X = MathHelper.Clamp(value.X, 0, _maxWidth - _screen.Width);
            _position.Y = MathHelper.Clamp(value.Y, 0, _maxHeight - _screen.Height);
            _screen.X = (int)_position.X;
            _screen.Y = (int)_position.Y;
        }
    }

    /// <summary>
    /// Gets and Sets the width if the sampler.
    /// </summary>
    public int Width
    {
        get => _screen.Width;
        set => _screen.Width = MathHelper.Clamp(value, 0, _maxWidth);
    }

    /// <summary>
    /// Gets and Sets the height of the sampler.
    /// </summary>
    public int Height
    {
        get => _screen.Height;
        set => _screen.Height = MathHelper.Clamp(value, 0, _maxHeight);
    }

    public Rectangle ScreenRectangle => _screen;

    /// <summary>
    /// Gets and Sets the maximal allowed width of the sampler.
    /// </summary>
    public int MaxWidth
    {
        get => _maxWidth;
        set => _maxWidth = value;
    }

    /// <summary>
    /// Gets and Sets the maximal allowed height of the sampler.
    /// </summary>
    public int MaxHeight
    {
        get => _maxHeight;
        set => _maxHeight = value;
    }

    public ScreenSampler(Game game, int x, int y, int width, int height)
        : this(game, new Rectangle(x, y, width, height))
    {
    }

    public ScreenSampler(Game game, Rectangle screenRectangle)
    {
        // Add the sampler as a service.
        //game.Services.AddService(typeof(IScreenSampler), this);
        // TODO: Look over whether the sample should really be a service
        // Set the screen rectangle and the position of the sampler
        _screen = screenRectangle;
        _position = new Vector2(_screen.X, _screen.Y);
        _maxWidth = _screen.Width;
        _maxHeight = _screen.Height;
    }

    public ScreenSampler(Rectangle screenRectangle)
    {
        // Set the screen rectangle and the position of the sampler
        _screen = screenRectangle;
        _position = new Vector2(_screen.X, _screen.Y);
        _maxWidth = _screen.Width;
        _maxHeight = _screen.Height;
    }

    /// <summary>
    /// Checks if a specified rectangle is within the screen area.
    /// </summary>
    /// <param name="boundingBox">Rectangle to control.</param>
    /// <returns>True if the rectangle is on screen; false otherwise.</returns>
    public bool IsOnScreen(Rectangle boundingBox)
    {
        return _screen.Intersects(boundingBox);
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
        return x < _screen.Right &&
               y < _screen.Bottom &&
               (x + width) > _screen.Left &&
               (y + height) > _screen.Top;
    }
}