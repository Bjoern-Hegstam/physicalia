using System.Xml;
using XNALibrary.Animation;
using XNALibrary.Sprites;

namespace XNALibrary.TileEngine;

public interface ITileLibrary
{
    bool AddTile(int key, Tile tile);
    bool RemoveTile(int key);
    Tile GetTile(int key);

    bool ContainsKey(int key);
    void Clear();

    void LoadXml(string path, SpriteLibrary spriteLibrary, IAnimationManager animationManager);
    void LoadXml(XmlReader reader, SpriteLibrary spriteLibrary, IAnimationManager animationManager);
}