using Microsoft.Xna.Framework;

namespace XNALibrary.Animation;

public class Animation(AnimationDefinition animationDefinition)
{
    public AnimationDefinition AnimationDefinition => animationDefinition;

    public int FrameCount => animationDefinition.Frames.Count;
    public float Framerate { get; } = animationDefinition.FramesPerSecond;

    private int _frameIndex;

    private int FrameIndex
    {
        get => _frameIndex;
        set => _frameIndex = value >= FrameCount && IsLoop
            ? value % FrameCount
            : MathHelper.Clamp(value, 0, FrameCount - 1);
    }

    public bool IsActive { get; private set; }

    public bool IsLoop => animationDefinition.IsLoop;

    public float TimeTillNextFrame { get; private set; } = 1 / animationDefinition.FramesPerSecond;

    public Frame CurrentFrame => animationDefinition.Frames[_frameIndex];

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
}