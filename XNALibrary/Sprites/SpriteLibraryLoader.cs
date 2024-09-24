using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Graphics;

namespace XNALibrary.Sprites;

public static class SpriteLibraryLoader
{
    public static SpriteLibrary Load(string path, ContentManager contentManager)
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
        return Load(reader, contentManager);
    }

    private static SpriteLibrary Load(XmlReader reader, ContentManager contentManager)
    {
        var spriteLibrary = new SpriteLibrary();
        
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Sprite" })
            {
                var id = new SpriteId(reader.GetAttribute("id" ) ?? throw new ResourceLoadException());
                var textureId = new TextureId(reader.GetAttribute("textureId") ?? throw new ResourceLoadException());
                int x = int.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
                int y = int.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());
                int width = int.Parse(reader.GetAttribute("width") ?? throw new ResourceLoadException());
                int height = int.Parse(reader.GetAttribute("height") ?? throw new ResourceLoadException());

                var texture2D = contentManager.Load<Texture2D>(textureId.AssetName);
                spriteLibrary.AddSprite(id, new Sprite(texture2D, new Rectangle(x, y, width, height)));
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "SpriteLibrary" })
            {
                break;
            }
        }

        return spriteLibrary;
    }
}