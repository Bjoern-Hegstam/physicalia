using System;
using System.Drawing;
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
        get { return this.columnCount; }
        set
        {
            this.columnCount = value;
            if (this.column >= this.columnCount)
            {
                this.column = this.columnCount - 1;
                this.UpdateSourceRectangle();
            }
        }
    }

    /// <summary>
    /// Gets and sets the number of rows.
    /// </summary>
    public int Rows
    {
        get { return this.rowCount; }
        set
        {
            this.rowCount = value;
            if (this.row >= this.rowCount)
            {
                this.row = this.rowCount - 1;
                this.UpdateSourceRectangle();
            }
        }
    }

    /// <summary>
    /// Gets and sets the frame rate in frames per second.
    /// </summary>
    public float Framerate
    {
        get { return this.framerate; }
        set { this.framerate = value; }
    }

    /// <summary>
    /// Gets and sets the current frame index.
    /// </summary>
    public int FrameIndex
    {
        get { return this.index; }
        set
        {
            // See if it's time to loop around
            if (loop && value >= (this.rowCount * this.columnCount))
            {
                this.row = this.column = this.index = 0;
                this.UpdateSourceRectangle();
                return;
            }

            // Make sure the index is within the allowed range
            value = (int)MathHelper.Clamp(value, 0, this.rowCount * this.columnCount);
                
            this.index = value;
            this.row = this.index / this.columnCount;
            this.column = this.index % this.columnCount;
            this.UpdateSourceRectangle();
        }
    }

    /// <summary>
    /// Gets and sets whether the animation is active.
    /// </summary>
    public bool IsActive
    {
        get { return this.active; }
        set { this.active = value; }
    }

    /// <summary>
    /// Gets and sets whether the animation will loop.
    /// </summary>
    public bool Loop
    {
        get { return this.loop; }
        set { this.loop = value; }
    }

    /// <summary>
    /// Gets and sets the time, in seconds, left till the change to the
    /// next frame.
    /// </summary>
    public float TimeTillNextFrame
    {
        get { return this.timeTillFrameChange; }
        set { this.timeTillFrameChange = value; }
    }

    /// <summary>
    /// Gets the source rectangle of the current frame.
    /// </summary>
    public Rectangle SourceRectangle
    {
        get { return this.sourceRectangle; }
    }

    /// <summary>
    /// Gets or sets the texture used by the animation
    /// </summary>
    public Texture2D Texture
    {
        get { return this.texture; }
        set { this.texture = value; }
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
        this.active = false;
        this.sourceRectangle = this.baseFrame = startFrame;
        this.columnCount = columns;
        this.rowCount = rows;
        this.framerate = framerate;
        this.timeTillFrameChange = 1 / this.framerate;
        this.row = this.column = this.index = 0;
        this.texture = texture;
        this.UpdateSourceRectangle();
    }

    /// <summary>
    /// Starts playback of the Animation from the current position.
    /// </summary>
    public void Play()
    {
        this.active = true;
    }

    /// <summary>
    /// Pauses playback of the Animation.
    /// </summary>
    public void Pause()
    {
        this.active = false;
    }

    /// <summary>
    /// Stops playback of the Animation and sets the position to the start.
    /// </summary>
    public void Stop()
    {
        this.active = false;
        this.FrameIndex = 0;
        this.timeTillFrameChange = 1 / this.framerate;
    }

    /// <summary>
    /// Updates the position of the source rectangle to the position of
    /// the current frame.
    /// </summary>
    private void UpdateSourceRectangle()
    {
        this.sourceRectangle.X = this.baseFrame.X + this.column * this.baseFrame.Width;
        this.sourceRectangle.Y = this.baseFrame.Y + this.row * this.baseFrame.Height;
    }

    /// <summary>
    /// Creates a shallow copy of the current instance.
    /// </summary>
    /// <returns>A new Animation that is a copy of the current animation.</returns>
    public Animation Copy()
    {
        Animation animation = new Animation(this.baseFrame,
            this.columnCount,
            this.rowCount,
            this.framerate,
            this.texture);
        animation.loop = this.loop;
        return animation;
    }
}