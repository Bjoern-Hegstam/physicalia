using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Graphics.ScreenManagement;

/// <summary>
/// Interface defining how an effect used for transitioning between two
/// screens should be interacted with.
/// </summary>
public interface IScreenTransitionEffect
{
    /// <summary>
    /// Effect used at transitions.
    /// </summary>
    Effect TransitionEffect { get; }

    /// <summary>
    /// Gets and sets the mask used for transitioning between two Screens.
    /// </summary>
    Texture2D TransitionMask { get; set; }

    /// <summary>
    /// The current progress of the transition. The value has a range of
    /// 0 to 1. Where 0 is when the transition has begun and 1 when it has
    /// completly finished.
    /// </summary>
    float TransitionProgress { set; }

    /// <summary>
    /// Tells the effect to begin using the current technique and pass 0.
    /// </summary>
    void Begin();

    /// <summary>
    /// Tells the effect to stop using the current technique and pass 0.
    /// </summary>
    void End();
}