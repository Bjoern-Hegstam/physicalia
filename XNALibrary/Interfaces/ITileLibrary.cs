using System.Xml;
using XNALibrary.Graphics.TileEngine;

namespace XNALibrary.Interfaces;

public interface ITileLibrary
{
    bool AddTile(int key, Tile tile);
    bool RemoveTile(int key);
    Tile GetTile(int key);

    bool ContainsKey(int key);
    void Clear();

    void LoadXml(string path, ISpriteLibrary spriteLibrary, IAnimationManager animationManager);
    void LoadXml(XmlReader reader, ISpriteLibrary spriteLibrary, IAnimationManager animationManager);
}