using XNALibrary.Graphics.Sprites;

namespace XNALibrary.Interfaces;

public interface ISpriteLibrary
{
    void AddSprite(int key, Sprite sprite);
    void RemoveSprite(int key);
    Sprite GetSprite(int key);

    void LoadXml(string path);
}