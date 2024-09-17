using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Animation;

public class Animation
{
    /// <summary>
    /// First frame of the animation.
    /// </summary>
    private readonly Rectangle _baseFrame;

    /// <summary>
    /// The number of columns in the animation.
    /// </summary>
    private int _columnCount;

    /// <summary>
    /// The number of rows in the animation.
    /// </summary>
    private int _rowCount;

    /// <summary>
    /// The framerate measured in frames per second.
    /// </summary>
    private float _framerate;

    /// <summary>
    /// Index of the current frame.
    /// </summary>
    private int _index;

    /// <summary>
    /// Row of the current frame.
    /// </summary>
    private int _row;

    /// <summary>
    /// Column of the current frame.
    /// </summary>
    private int _column;

    /// <summary>
    /// Rectangle representing the part of the texture that maps to the
    /// current frame.
    /// </summary>
    private Rectangle _sourceRectangle;

    /// <summary>
    /// Denotes whether the animation is currently active.
    /// </summary>
    private bool _active;

    /// <summary>
    /// Boolean indictaing whether the current animation will loop when
    /// the last frame has been reached.
    /// </summary>
    private bool _loop;

    /// <summary>
    /// The time in seconds left until the next frame.
    /// </summary>
    private float _timeTillFrameChange;

    /// <summary>
    /// Texture used.
    /// </summary>
    private Texture2D _texture;

    /// <summary>
    /// Gets and sets the number of columns.
    /// </summary>
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

    /// <summary>
    /// Gets and sets the number of rows.
    /// </summary>
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

    /// <summary>
    /// Gets and sets the frame rate in frames per second.
    /// </summary>
    public float Framerate
    {
        get => _framerate;
        set => _framerate = value;
    }

    /// <summary>
    /// Gets and sets the current frame index.
    /// </summary>
    public int FrameIndex
    {
        get => _index;
        set
        {
            // See if it's time to loop around
            if (_loop && value >= (_rowCount * _columnCount))
            {
                _row = _column = _index = 0;
                UpdateSourceRectangle();
                return;
            }

            // Make sure the index is within the allowed range
            value = (int)MathHelper.Clamp(value, 0, _rowCount * _columnCount);

            _index = value;
            _row = _index / _columnCount;
            _column = _index % _columnCount;
            UpdateSourceRectangle();
        }
    }

    /// <summary>
    /// Gets and sets whether the animation is active.
    /// </summary>
    public bool IsActive
    {
        get => _active;
        set => _active = value;
    }

    /// <summary>
    /// Gets and sets whether the animation will loop.
    /// </summary>
    public bool Loop
    {
        get => _loop;
        set => _loop = value;
    }

    /// <summary>
    /// Gets and sets the time, in seconds, left till the change to the
    /// next frame.
    /// </summary>
    public float TimeTillNextFrame
    {
        get => _timeTillFrameChange;
        set => _timeTillFrameChange = value;
    }

    /// <summary>
    /// Gets the source rectangle of the current frame.
    /// </summary>
    public Rectangle SourceRectangle => _sourceRectangle;

    /// <summary>
    /// Gets or sets the texture used by the animation
    /// </summary>
    public Texture2D Texture
    {
        get => _texture;
        set => _texture = value;
    }

    /// <summary>
    /// Creates a new Animation.
    /// </summary>
    /// <param name="startFrame">The first frame of the animation.</param>
    /// <param name="columns">The number of columns making up the animation.</param>
    /// <param name="rows">The number of rows making up the animation.</param>
    /// <param name="framerate">Framerate of the animation, measured in frames per seconds.</param>
    /// <param name="texture">The texture used by the animation.</param>
    public Animation(Rectangle startFrame, int columns, int rows, float framerate, Texture2D texture)
    {
        _active = false;
        _sourceRectangle = _baseFrame = startFrame;
        _columnCount = columns;
        _rowCount = rows;
        _framerate = framerate;
        _timeTillFrameChange = 1 / _framerate;
        _row = _column = _index = 0;
        _texture = texture;
        UpdateSourceRectangle();
    }

    /// <summary>
    /// Starts playback of the Animation from the current position.
    /// </summary>
    public void Play()
    {
        _active = true;
    }

    /// <summary>
    /// Pauses playback of the Animation.
    /// </summary>
    public void Pause()
    {
        _active = false;
    }

    /// <summary>
    /// Stops playback of the Animation and sets the position to the start.
    /// </summary>
    public void Stop()
    {
        _active = false;
        FrameIndex = 0;
        _timeTillFrameChange = 1 / _framerate;
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
        Animation animation = new Animation(_baseFrame,
            _columnCount,
            _rowCount,
            _framerate,
            _texture);
        animation._loop = _loop;
        return animation;
    }
}