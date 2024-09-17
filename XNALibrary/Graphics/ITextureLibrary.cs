using System.Xml;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Graphics;

public interface ITextureLibrary
{
    Texture2D this[int key] { get; }

    bool AddTexture(int key, Texture2D texture);
    bool RemoveTexture(int key);
    Texture2D? GetTexture(int key);

    bool ContainsKey(int key);
    void Clear();

    void LoadXml(string path, GraphicsDevice graphics);
    void LoadXml(XmlReader reader, GraphicsDevice graphics);
}