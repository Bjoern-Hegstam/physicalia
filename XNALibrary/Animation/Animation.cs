using Microsoft.Xna.Framework;

namespace XNALibrary.Animation;

public class Animation(AnimationDefinition animationDefinition)
{
    public AnimationDefinition AnimationDefinition => animationDefinition;

    public int FrameCount => animationDefinition.ColumnCount * animationDefinition.RowCount;
    public float Framerate { get; } = animationDefinition.FramesPerSecond;

    private Rectangle _frame = animationDefinition.StartFrame;
    private int _frameIndex;
    private int _frameRow;
    private int _frameColumn;

    private int FrameIndex
    {
        get => _frameIndex;
        set
        {
            _frameIndex = IsLoop && value >= FrameCount
                ? value % FrameCount
                : MathHelper.Clamp(value, 0, FrameCount);

            _frameRow = _frameIndex / animationDefinition.ColumnCount;
            _frameColumn = _frameIndex % animationDefinition.ColumnCount;
            
            UpdateFrameLocation();
        }
    }

    public bool IsActive { get; private set; }

    public bool IsLoop => animationDefinition.IsLoop;

    public float TimeTillNextFrame { get; private set; } = 1 / animationDefinition.FramesPerSecond;

    public Rectangle Frame => _frame;

    public void Play()
    {
        FrameIndex = 0;
        TimeTillNextFrame = 1 / Framerate;
        IsActive = true;
    }

    public void Stop()
    {
        IsActive = false;
    }

    public void Update(GameTime gameTime)
    {
        TimeTillNextFrame -= (float)gameTime.ElapsedGameTime.TotalSeconds;

        var indexIncrease = 0;
        while (TimeTillNextFrame <= 0)
        {
            TimeTillNextFrame += 1 / Framerate;
            indexIncrease++;
        }

        if (indexIncrease > 0)
        {
            FrameIndex += indexIncrease;
        }

        if (FrameIndex == FrameCount - 1 && !IsLoop)
        {
            IsActive = false;
        }
    }

    private void UpdateFrameLocation()
    {
        _frame.Location = animationDefinition.StartFrame.Location +
                          new Point(_frameColumn, _frameRow) * animationDefinition.StartFrame.Size;
    }
}