using Microsoft.Xna.Framework;

namespace XNALibrary.Animation;

public class AnimationRunner(Game game) : GameComponent(game)
{
    private readonly List<Animation> _playbackAnimations = [];

    public Animation AddPlaybackAnimation(AnimationDefinitionId animationDefinitionId)
    {
        var animationLibrary = Game.Services.GetService<AnimationLibrary>();

        var animation = new Animation(animationLibrary[animationDefinitionId]);
        _playbackAnimations.Add(animation);
        return animation;
    }

    public override void Update(GameTime gameTime)
    {
        foreach (Animation animation in _playbackAnimations.Where(animation => animation.IsActive))
        {
            animation.Update(gameTime);
        }
    }
}