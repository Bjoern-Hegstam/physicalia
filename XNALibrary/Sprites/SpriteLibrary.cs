using System.Xml;
using Microsoft.Xna.Framework;
using XNALibrary.Graphics;

namespace XNALibrary.Sprites;

/// <summary>
/// Manages a collection of sprites and related sprite sheets.
/// </summary>
public class SpriteLibrary(TextureLibrary textureLibrary)
{
    private readonly Dictionary<SpriteId, Sprite> _sprites = new();

    public Sprite GetSprite(SpriteId id)
    {
        return _sprites[id];
    }

    public void LoadXml(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (path.Length < 5 || path.Substring(path.Length - 3, 3).ToLower() != "xml")
        {
            throw new ArgumentException("File is not of type '.xml'");
        }

        var settings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreProcessingInstructions = true
        };

        using var reader = XmlReader.Create(path, settings);
        LoadXml(reader);
    }

    public void LoadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Sprite" })
            {
                var id = new SpriteId(int.Parse(reader.GetAttribute(0)));
                var textureId = new TextureId(int.Parse(reader.GetAttribute(1)));
                int x = int.Parse(reader.GetAttribute(2));
                int y = int.Parse(reader.GetAttribute(3));
                int width = int.Parse(reader.GetAttribute(4));
                int height = int.Parse(reader.GetAttribute(5));

                _sprites.Add(id, new Sprite(textureLibrary[textureId], new Rectangle(x, y, width, height)));
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "SpriteLibrary" })
            {
                return;
            }
        }
    }
}