namespace XNALibrary.Animation;

public class AnimationLibrary
{
    private readonly Dictionary<AnimationDefinitionId, AnimationDefinition> _animationDefinitions = new();

    public AnimationDefinition this[AnimationDefinitionId id] => _animationDefinitions[id];

    public void Add(AnimationDefinition animationDefinition)
    {
        _animationDefinitions[animationDefinition.Id] = animationDefinition;
    }
}