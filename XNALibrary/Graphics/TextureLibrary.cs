using System.Xml;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Graphics;

public class TextureLibrary
{
    private readonly Dictionary<int, Texture2D> _textureLibrary = new();

    public Texture2D this[int key] => GetTexture(key);

    public bool AddTexture(int key, Texture2D texture)
    {
        return _textureLibrary.TryAdd(key, texture);
    }

    public Texture2D GetTexture(int key)
    {
        return _textureLibrary.TryGetValue(key, out var texture) ? texture : throw new MissingTextureException();
    }

    public void LoadXml(string path, GraphicsDevice graphics)
    {
        var readerSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true
        };

        using var reader = XmlReader.Create(path, readerSettings);
        LoadXml(reader, graphics);
    }

    public void LoadXml(XmlReader reader, GraphicsDevice graphics)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Texture" })
            {
                int id = int.Parse(reader.GetAttribute("id") ?? throw new ResourceLoadException());

                Texture2D texture = Texture2D.FromFile(graphics, reader.ReadString());
                AddTexture(id, texture);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "TextureLibrary" })
            {
                return;
            }
        }
    }
}