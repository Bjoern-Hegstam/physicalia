using System.Xml;
using Microsoft.Xna.Framework;
using XNALibrary.Graphics;

namespace XNALibrary.Animation;

public class AnimationManager(Game game, TextureLibrary textureLibrary) : GameComponent(game)
{
    private readonly Dictionary<int, Animation> _animationBank = new();
    private readonly List<Animation> _playbackAnims = [];

    public Animation GetBankAnimation(int key)
    {
        return _animationBank[key];
    }

    public void AddPlaybackAnimation(Animation animation)
    {
        _playbackAnims.Add(animation);
    }

    /// <summary>
    /// Adds an animation from the bank to the collection of available
    /// animations.
    /// </summary>
    /// <param name="bankKey">Key to the animation in the bank.</param>
    /// <returns>The added animation or null if the animation bank didn't
    /// contain an animation with the specified key.</returns>
    public Animation AddPlaybackAnimation(int bankKey)
    {
        Animation animation = _animationBank[bankKey].Copy();
        _playbackAnims.Add(animation);
        return animation;

    }

    public override void Update(GameTime gameTime)
    {
        // The increase in frame index
        var indexIncrease = 0;

        // Go through every active animation
        foreach (Animation animation in _playbackAnims)
        {
            // Only update active animations
            if (!animation.IsActive)
            {
                continue;
            }

            // Subtract total number of seconds from the count down to till
            // the next frame
            animation.TimeTillNextFrame -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Increase the index increase as long as we need to change frames
            while (animation.TimeTillNextFrame <= 0)
            {
                animation.TimeTillNextFrame += 1 / animation.Framerate;
                indexIncrease++;
            }

            // If a frame change was made then add to the index of the
            // animation and reset the increase
            if (indexIncrease > 0)
            {
                animation.FrameIndex += indexIncrease;
                indexIncrease = 0;
            }

            // See if the animation has come to its last frame and
            // wont loop
            if (animation.FrameIndex == animation.Rows * animation.Columns - 1 && !animation.Loop)
            {
                animation.IsActive = false;
            }
        }
    }

    public void LoadXml(string path)
    {
        using var reader = XmlReader.Create(path);
        LoadXml(reader);
    }

    public void LoadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "Animation") continue;
            
            int id = int.Parse(reader.GetAttribute("id") ?? throw new ResourceLoadException());
            Animation anim = LoadAnimationFromXml(reader);

            _animationBank.Add(id, anim);
        }
    }

    private Animation LoadAnimationFromXml(XmlReader reader)
    {
        reader.ReadToFollowing("TextureKey");
        var textureId = new TextureId(int.Parse(reader.ReadElementContentAsString()));

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
        var anim = new Animation(startFrame, columns, rows, frameRate, textureLibrary[textureId])
        {
            Loop = loop
        };

        return anim;
    }
}