using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Graphics.Animation;

public class Animation
{
    /// <summary>
    /// First frame of the animation.
    /// </summary>
    private Rectangle baseFrame;

    /// <summary>
    /// The number of columns in the animation.
    /// </summary>
    private int columnCount;

    /// <summary>
    /// The number of rows in the animation.
    /// </summary>
    private int rowCount;

    /// <summary>
    /// The framerate measured in frames per second.
    /// </summary>
    private float framerate;

    /// <summary>
    /// Index of the current frame.
    /// </summary>
    private int index = 0;

    /// <summary>
    /// Row of the current frame.
    /// </summary>
    private int row = 0;

    /// <summary>
    /// Column of the current frame.
    /// </summary>
    private int column = 0;

    /// <summary>
    /// Rectangle representing the part of the texture that maps to the
    /// current frame.
    /// </summary>
    private Rectangle sourceRectangle;

    /// <summary>
    /// Denotes whether the animation is currently active.
    /// </summary>
    private bool active = false;

    /// <summary>
    /// Boolean indictaing whether the current animation will loop when
    /// the last frame has been reached.
    /// </summary>
    private bool loop = false;

    /// <summary>
    /// The time in seconds left until the next frame.
    /// </summary>
    private float timeTillFrameChange;

    /// <summary>
    /// Texture used.
    /// </summary>
    private Texture2D texture;

    /// <summary>
    /// Gets and sets the number of columns.
    /// </summary>
    public int Columns
    {
        get { return columnCount; }
        set
        {
            columnCount = value;
            if (column >= columnCount)
            {
                column = columnCount - 1;
                UpdateSourceRectangle();
            }
        }
    }

    /// <summary>
    /// Gets and sets the number of rows.
    /// </summary>
    public int Rows
    {
        get { return rowCount; }
        set
        {
            rowCount = value;
            if (row >= rowCount)
            {
                row = rowCount - 1;
                UpdateSourceRectangle();
            }
        }
    }

    /// <summary>
    /// Gets and sets the frame rate in frames per second.
    /// </summary>
    public float Framerate
    {
        get { return framerate; }
        set { framerate = value; }
    }

    /// <summary>
    /// Gets and sets the current frame index.
    /// </summary>
    public int FrameIndex
    {
        get { return index; }
        set
        {
            // See if it's time to loop around
            if (loop && value >= (rowCount * columnCount))
            {
                row = column = index = 0;
                UpdateSourceRectangle();
                return;
            }

            // Make sure the index is within the allowed range
            value = (int)MathHelper.Clamp(value, 0, rowCount * columnCount);
                
            index = value;
            row = index / columnCount;
            column = index % columnCount;
            UpdateSourceRectangle();
        }
    }

    /// <summary>
    /// Gets and sets whether the animation is active.
    /// </summary>
    public bool IsActive
    {
        get { return active; }
        set { active = value; }
    }

    /// <summary>
    /// Gets and sets whether the animation will loop.
    /// </summary>
    public bool Loop
    {
        get { return loop; }
        set { loop = value; }
    }

    /// <summary>
    /// Gets and sets the time, in seconds, left till the change to the
    /// next frame.
    /// </summary>
    public float TimeTillNextFrame
    {
        get { return timeTillFrameChange; }
        set { timeTillFrameChange = value; }
    }

    /// <summary>
    /// Gets the source rectangle of the current frame.
    /// </summary>
    public Rectangle SourceRectangle
    {
        get { return sourceRectangle; }
    }

    /// <summary>
    /// Gets or sets the texture used by the animation
    /// </summary>
    public Texture2D Texture
    {
        get { return texture; }
        set { texture = value; }
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
        active = false;
        sourceRectangle = baseFrame = startFrame;
        columnCount = columns;
        rowCount = rows;
        this.framerate = framerate;
        timeTillFrameChange = 1 / this.framerate;
        row = column = index = 0;
        this.texture = texture;
        UpdateSourceRectangle();
    }

    /// <summary>
    /// Starts playback of the Animation from the current position.
    /// </summary>
    public void Play()
    {
        active = true;
    }

    /// <summary>
    /// Pauses playback of the Animation.
    /// </summary>
    public void Pause()
    {
        active = false;
    }

    /// <summary>
    /// Stops playback of the Animation and sets the position to the start.
    /// </summary>
    public void Stop()
    {
        active = false;
        FrameIndex = 0;
        timeTillFrameChange = 1 / framerate;
    }

    /// <summary>
    /// Updates the position of the source rectangle to the position of
    /// the current frame.
    /// </summary>
    private void UpdateSourceRectangle()
    {
        sourceRectangle.X = baseFrame.X + column * baseFrame.Width;
        sourceRectangle.Y = baseFrame.Y + row * baseFrame.Height;
    }

    /// <summary>
    /// Creates a shallow copy of the current instance.
    /// </summary>
    /// <returns>A new Animation that is a copy of the current animation.</returns>
    public Animation Copy()
    {
        Animation animation = new Animation(baseFrame,
            columnCount,
            rowCount,
            framerate,
            texture);
        animation.loop = loop;
        return animation;
    }
}