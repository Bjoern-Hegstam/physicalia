using System.Xml;
using Microsoft.Xna.Framework;
using XNALibrary.Interfaces;

namespace XNALibrary.Graphics.Animation;

public class AnimationManager : GameComponent, IAnimationManager
{
    /// <summary>
    /// The banks animations stored by the AnimationManager.
    /// </summary>
    private readonly Dictionary<int, Animation> _animationBank;

    /// <summary>
    /// The playback animations controlled by the AnimationManager.
    /// </summary>
    private readonly List<Animation> _playbackAnims;

    private readonly ITextureLibrary _textureLibrary;

    public AnimationManager(Game game, ITextureLibrary textureLibrary)
        : base(game)
    {
        _animationBank = new Dictionary<int, Animation>();
        _playbackAnims = new List<Animation>();

        _textureLibrary = textureLibrary;
    }

    /// <summary>
    /// Adds a new animation to the manager.
    /// </summary>
    /// <param name="key">Key to use for identifying the animation.</param>
    /// <param name="animation">Animation to add.</param>
    /// <returns>True if the animation was succesfully added.</returns>
    public bool AddBankAnimation(int key, Animation animation)
    {
        if (!_animationBank.ContainsKey(key))
        {
            _animationBank.Add(key, animation);
            return true;
        }

        return false;
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
        if (!_animationBank.ContainsKey(key))
        {
            Animation animation = new Animation(startFrame, columns, rows, framerate, _textureLibrary[textureKey]);
            _animationBank.Add(key, animation);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes a specified animation from the bank of animations.
    /// </summary>
    /// <param name="storageKey">Key of the animation to remove.</param>
    public void RemoveBankAnimation(int key)
    {
        if (_animationBank.ContainsKey(key))
            _animationBank.Remove(key);
    }

    /// <summary>
    /// Gets the specified animation from the AnimationManager's bank
    /// of animations.
    /// </summary>
    /// <param name="key">Key to the animation to get.</param>
    /// <returns>Animation corresponding to the key or null if
    /// no animation was found.</returns>
    public Animation GetBankAnimation(int key)
    {
        if (_animationBank.ContainsKey(key))
            return _animationBank[key];

        return null;
    }

    /// <summary>
    /// Clears the AnimationManager's bank of animations.
    /// </summary>
    public void ClearAnimationBank()
    {
        _animationBank.Clear();
    }

    /// <summary>
    /// Clears all playback animations.
    /// </summary>
    public void ClearPlaybackAnimations()
    {
        _playbackAnims.Clear();
    }

    /// <summary>
    /// Adds the animation to the AnimationManager.
    /// </summary>
    /// <param name="animation">Animation to add.</param>
    /// <returns>Always true.</returns>
    public bool AddPlaybackAnimation(Animation animation)
    {
        _playbackAnims.Add(animation);

        return true;
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
        if (_animationBank.ContainsKey(bankKey))
        {
            Animation animation = _animationBank[bankKey].Copy();
            _playbackAnims.Add(animation);
            return animation;
        }

        return null;
    }

    /// <summary>
    /// Checks whether the AnimationManager's bank of animations contains
    /// an animation mapped to the specified key.
    /// </summary>
    /// <param name="bankKey">The key to check.</param>
    /// <returns>True if an animation corresponding to the key was found; false otherwise.</returns>
    public bool HasBankAnimation(int bankKey)
    {
        return _animationBank.ContainsKey(bankKey);
    }

    /// <summary>
    /// Update all active animations.
    /// </summary>
    /// <param name="gameTime"></param>
    public override void Update(GameTime gameTime)
    {
        // The increase in frame index
        int indexIncrease = 0;

        // Go throught every active animation
        foreach (Animation animation in _playbackAnims)
        {
            // Only update active animations
            if (!animation.IsActive)
                continue;

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
            // animtion and reset the increase
            if (indexIncrease > 0)
            {
                animation.FrameIndex += indexIncrease;
                indexIncrease = 0;
            }

            // See if the animation has come to its last frame and
            // wont loop
            if (animation.FrameIndex == animation.Rows * animation.Columns - 1 &&
                !animation.Loop)
            {
                animation.IsActive = false;
            }
        }
    }

    public void LoadXml(string path)
    {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.IgnoreComments = true;
        readerSettings.IgnoreWhitespace = true;

        using XmlReader reader = XmlReader.Create(path);
        LoadXml(reader);
    }

    public void LoadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Animation")
            {
                int id = int.Parse(reader.GetAttribute("id"));
                Animation anim = LoadAnimationFromXml(reader);

                _animationBank.Add(id, anim);
            }
        }
    }

    private Animation LoadAnimationFromXml(XmlReader reader)
    {
        int textureKey, x, y, width, height, columns, rows, frameRate;
        bool loop;

        reader.ReadToFollowing("TextureKey");
        textureKey = int.Parse(reader.ReadElementContentAsString());

        reader.ReadToFollowing("StartFrame");
        x = int.Parse(reader.GetAttribute("x"));
        y = int.Parse(reader.GetAttribute("y"));
        width = int.Parse(reader.GetAttribute("width"));
        height = int.Parse(reader.GetAttribute("height"));

        reader.ReadToFollowing("Dimensions");
        columns = int.Parse(reader.GetAttribute("columns"));
        rows = int.Parse(reader.GetAttribute("rows"));

        reader.ReadToFollowing("FrameRate");
        frameRate = int.Parse(reader.ReadElementContentAsString());

        reader.ReadToFollowing("Loop");
        loop = bool.Parse(reader.ReadElementContentAsString());

        Rectangle startFrame = new Rectangle(x, y, width, height);
        Animation anim = new Animation(startFrame, columns, rows, frameRate, _textureLibrary[textureKey]);
        anim.Loop = loop;

        return anim;
    }
}