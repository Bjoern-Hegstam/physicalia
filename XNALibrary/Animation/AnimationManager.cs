using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Graphics;

namespace XNALibrary.Animation;

public class AnimationManager(Game game) : GameComponent(game)
{
    private readonly Dictionary<AnimationDefinitionId, AnimationDefinition> _animationDefinitions = new();
    private readonly List<Animation> _playbackAnimations = [];

    public Animation AddPlaybackAnimation(AnimationDefinitionId animationDefinitionId)
    {
        var animation = new Animation(_animationDefinitions[animationDefinitionId]);
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

    public void LoadXml(string path, ContentManager contentManager)
    {
        using var reader = XmlReader.Create(path);
        LoadXml(reader, contentManager);
    }

    public void LoadXml(XmlReader reader, ContentManager contentManager)
    {
        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "Animation") continue;

            AnimationDefinition animationDefinition = LoadAnimationDefinition(reader, contentManager);

            _animationDefinitions.Add(animationDefinition.Id, animationDefinition);
        }
    }

    private AnimationDefinition LoadAnimationDefinition(XmlReader reader, ContentManager contentManager)
    {
        var id = new AnimationDefinitionId(reader.GetAttribute("id") ?? throw new ResourceLoadException());

        reader.ReadToFollowing("TextureId");
        var textureId = new TextureId(reader.ReadElementContentAsString());

        reader.ReadToFollowing("StartFrame");
        var x = int.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
        var y = int.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());
        var width = int.Parse(reader.GetAttribute("width") ?? throw new ResourceLoadException());
        var height = int.Parse(reader.GetAttribute("height") ?? throw new ResourceLoadException());

        reader.ReadToFollowing("Dimensions");
        var columns = int.Parse(reader.GetAttribute("columns") ?? throw new ResourceLoadException());
        var rows = int.Parse(reader.GetAttribute("rows") ?? throw new ResourceLoadException());

        reader.ReadToFollowing("FrameRate");
        var frameRate = int.Parse(reader.ReadElementContentAsString());

        reader.ReadToFollowing("Loop");
        var loop = bool.Parse(reader.ReadElementContentAsString());

        var startFrame = new Rectangle(x, y, width, height);
        var texture = contentManager.Load<Texture2D>(textureId.AssetName);
        var animationDefinition = new AnimationDefinition(
            id,
            texture,
            startFrame,
            columns,
            rows,
            frameRate,
            loop
        );

        return animationDefinition;
    }
}