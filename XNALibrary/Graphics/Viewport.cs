using Microsoft.Xna.Framework;

namespace XNALibrary.Graphics;

public class Viewport
{
    private Vector2 _position;
    private Rectangle _screen;

    public Vector2 Position
    {
        get => _position;
        set
        {
            _position.X = MathHelper.Clamp(value.X, 0, MaxWidth - _screen.Width);
            _position.Y = MathHelper.Clamp(value.Y, 0, MaxHeight - _screen.Height);
            _screen.X = (int)_position.X;
            _screen.Y = (int)_position.Y;
        }
    }

    public int Width
    {
        get => _screen.Width;
        set => _screen.Width = MathHelper.Clamp(value, 0, MaxWidth);
    }

    public int Height
    {
        get => _screen.Height;
        set => _screen.Height = MathHelper.Clamp(value, 0, MaxHeight);
    }

    public Rectangle ScreenRectangle => _screen;

    public int MaxWidth { get; set; }
    public int MaxHeight { get; set; }

    public Viewport(int x, int y, int width, int height)
    {
        _screen = new Rectangle(x, y, width, height);
        _position = new Vector2(_screen.X, _screen.Y);
        MaxWidth = _screen.Width;
        MaxHeight = _screen.Height;
    }
}