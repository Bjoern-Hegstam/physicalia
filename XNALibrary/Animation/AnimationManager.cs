using System.Xml;
using Microsoft.Xna.Framework;
using XNALibrary.Graphics;

namespace XNALibrary.Animation;

public class AnimationManager(Game game, ITextureLibrary textureLibrary) : GameComponent(game), IAnimationManager
{
    private readonly Dictionary<int, Animation> _animationBank = new();
    private readonly List<Animation> _playbackAnims = [];

    /// <summary>
    /// Adds a new animation to the manager.
    /// </summary>
    /// <param name="key">Key to use for identifying the animation.</param>
    /// <param name="animation">Animation to add.</param>
    /// <returns>True if the animation was succesfully added.</returns>
    public bool AddBankAnimation(int key, Animation animation)
    {
        return _animationBank.TryAdd(key, animation);
    }

    /// <summary>
    /// Adds a new animation the manager.
    /// </summary>
    /// <param name="key">Key to use for identifying the animation.</param>
    /// <param name="startFrame">The first frame of the animation.</param>
    /// <param name="columns">The number of columns making up the animation.</param>
    /// <param name="rows">The number of rows making up the animation.</param>
    /// <param name="framerate">Framerate of the animation, measured in frames per seconds.</param>
    /// <param name="textureKey">Key of the texture used by the animation.</param>
    /// <returns>True if the animation was succesfully added; false otherwise</returns>
    public bool AddBankAnimation(int key, Rectangle startFrame, int columns, int rows, float framerate, int textureKey)
    {
        if (_animationBank.ContainsKey(key))
        {
            return false;
        }

        Animation animation = new Animation(startFrame, columns, rows, framerate, textureLibrary[textureKey]);
        _animationBank.Add(key, animation);
        return true;

    }

    public void RemoveBankAnimation(int key)
    {
        _animationBank.Remove(key);
    }

    public Animation GetBankAnimation(int key)
    {
        if (_animationBank.TryGetValue(key, out var animation))
        {
            return animation;
        }

        throw new MissingAnimationException();
    }

    public void ClearAnimationBank()
    {
        _animationBank.Clear();
    }

    public void ClearPlaybackAnimations()
    {
        _playbackAnims.Clear();
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
        if (!_animationBank.TryGetValue(bankKey, out var value))
        {
            throw new MissingAnimationException();
        }

        Animation animation = value.Copy();
        _playbackAnims.Add(animation);
        return animation;

    }

    public bool HasBankAnimation(int bankKey)
    {
        return _animationBank.ContainsKey(bankKey);
    }

    public override void Update(GameTime gameTime)
    {
        // The increase in frame index
        int indexIncrease = 0;

        // Go throught every active animation
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
            
            int id = int.Parse(reader.GetAttribute("id"));
            Animation anim = LoadAnimationFromXml(reader);

            _animationBank.Add(id, anim);
        }
    }

    private Animation LoadAnimationFromXml(XmlReader reader)
    {
        reader.ReadToFollowing("TextureKey");
        var textureKey = int.Parse(reader.ReadElementContentAsString());

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

        Rectangle startFrame = new Rectangle(x, y, width, height);
        Animation anim = new Animation(startFrame, columns, rows, frameRate, textureLibrary[textureKey])
        {
            Loop = loop
        };

        return anim;
    }
}