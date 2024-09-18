using System.Xml;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Graphics;

public class TextureLibrary
{
    private readonly Dictionary<TextureId, Texture2D> _textureLibrary = new();

    public Texture2D this[TextureId id] => GetTexture(id);

    public bool AddTexture(TextureId id, Texture2D texture)
    {
        return _textureLibrary.TryAdd(id, texture);
    }

    public Texture2D GetTexture(TextureId id)
    {
        return _textureLibrary[id];
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

                TextureId textureId = new TextureId(id);
                Texture2D texture = Texture2D.FromFile(graphics, reader.ReadString());
                AddTexture(textureId, texture);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "TextureLibrary" })
            {
                return;
            }
        }
    }
}