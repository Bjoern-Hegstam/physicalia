using System.Xml;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Graphics;

public class TextureLibrary : ITextureLibrary
{
    private readonly Dictionary<int, Texture2D> _textureLibrary = new();

    public Texture2D this[int key] => GetTexture(key);

    public bool AddTexture(int key, Texture2D texture)
    {
        return _textureLibrary.TryAdd(key, texture);
    }

    public bool RemoveTexture(int key)
    {
        return _textureLibrary.Remove(key);
    }

    public Texture2D GetTexture(int key)
    {
        return _textureLibrary.TryGetValue(key, out var texture) ? texture : throw new MissingTextureException();
    }

    public bool ContainsKey(int key)
    {
        return _textureLibrary.ContainsKey(key);
    }

    public void Clear()
    {
        _textureLibrary.Clear();
    }

    public void LoadXml(string path, GraphicsDevice graphics)
    {
        XmlReaderSettings readerSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true
        };

        using XmlReader reader = XmlReader.Create(path, readerSettings);
        LoadXml(reader, graphics);
    }

    public void LoadXml(XmlReader reader, GraphicsDevice graphics)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Texture")
            {
                int id = int.Parse(reader.GetAttribute("id"));

                Texture2D texture = Texture2D.FromFile(graphics, reader.ReadString());
                AddTexture(id, texture);
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "TextureLibrary")
            {
                return;
            }
        }
    }
}