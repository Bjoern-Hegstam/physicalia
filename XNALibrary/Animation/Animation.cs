using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Animation;

public class Animation
{
    private readonly Rectangle _baseFrame;

    private int _columnCount;
    private int _rowCount;

    private int _index;
    private int _row;
    private int _column;

    private Rectangle _sourceRectangle;

    public int Columns
    {
        get => _columnCount;
        set
        {
            _columnCount = value;
            if (_column >= _columnCount)
            {
                _column = _columnCount - 1;
                UpdateSourceRectangle();
            }
        }
    }

    public int Rows
    {
        get => _rowCount;
        set
        {
            _rowCount = value;
            if (_row >= _rowCount)
            {
                _row = _rowCount - 1;
                UpdateSourceRectangle();
            }
        }
    }

    public float Framerate { get; set; }

    public int FrameIndex
    {
        get => _index;
        set
        {
            // See if it's time to loop around
            if (Loop && value >= (_rowCount * _columnCount))
            {
                _row = _column = _index = 0;
                UpdateSourceRectangle();
                return;
            }

            // Make sure the index is within the allowed range
            value = MathHelper.Clamp(value, 0, _rowCount * _columnCount);

            _index = value;
            _row = _index / _columnCount;
            _column = _index % _columnCount;
            UpdateSourceRectangle();
        }
    }

    public bool IsActive { get; set; }

    public bool Loop { get; set; }

    public float TimeTillNextFrame { get; set; }

    public Rectangle SourceRectangle => _sourceRectangle;

    public Texture2D Texture { get; init; }

    public Animation(Rectangle startFrame, int columns, int rows, float framerate, Texture2D texture)
    {
        IsActive = false;
        _sourceRectangle = _baseFrame = startFrame;
        _columnCount = columns;
        _rowCount = rows;
        Framerate = framerate;
        TimeTillNextFrame = 1 / Framerate;
        _row = _column = _index = 0;
        Texture = texture;
        UpdateSourceRectangle();
    }

    public void Play()
    {
        IsActive = true;
    }

    public void Pause()
    {
        IsActive = false;
    }

    public void Stop()
    {
        IsActive = false;
        FrameIndex = 0;
        TimeTillNextFrame = 1 / Framerate;
    }

    /// <summary>
    /// Updates the position of the source rectangle to the position of
    /// the current frame.
    /// </summary>
    private void UpdateSourceRectangle()
    {
        _sourceRectangle.X = _baseFrame.X + _column * _baseFrame.Width;
        _sourceRectangle.Y = _baseFrame.Y + _row * _baseFrame.Height;
    }

    /// <summary>
    /// Creates a shallow copy of the current instance.
    /// </summary>
    /// <returns>A new Animation that is a copy of the current animation.</returns>
    public Animation Copy()
    {
        var animation = new Animation(_baseFrame,
            _columnCount,
            _rowCount,
            Framerate,
            Texture)
        {
            Loop = Loop
        };
        return animation;
    }
}