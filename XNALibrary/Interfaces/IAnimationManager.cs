using System.Xml;
using Microsoft.Xna.Framework;
using XNALibrary.Graphics.Animation;

namespace XNALibrary.Interfaces;

public interface IAnimationManager
{
    bool AddBankAnimation(int key, Animation animation);
    bool AddBankAnimation(int key, Rectangle startFrame, int columns, int rows, float framerate, int textureKey);
    void RemoveBankAnimation(int key);
    Animation GetBankAnimation(int key);

    void ClearPlaybackAnimations();
    bool AddPlaybackAnimation(Animation animation);
    Animation AddPlaybackAnimation(int bankKey);

    bool HasBankAnimation(int bankKey);

    void LoadXml(string path);
    void LoadXml(XmlReader reader);
}