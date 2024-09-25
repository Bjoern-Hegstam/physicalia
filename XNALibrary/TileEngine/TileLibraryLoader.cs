using System.Xml;
using Microsoft.Xna.Framework;
using XNALibrary.Animation;
using XNALibrary.Sprites;

namespace XNALibrary.TileEngine;

public static class TileLibraryLoader
{
    public static TileLibrary Load(string path, SpriteLibrary spriteLibrary, AnimationRunner animationRunner)
    {
        var settings = new XmlReaderSettings
        {
            CloseInput = true,
            IgnoreComments = true,
            IgnoreWhitespace = true,
            CheckCharacters = true
        };

        using var reader = XmlReader.Create(path, settings);
        return Load(reader, spriteLibrary, animationRunner);
    }

    public static TileLibrary Load(XmlReader reader, SpriteLibrary spriteLibrary, AnimationRunner animationRunner)
    {
        var tileLibrary = new TileLibrary();
        
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Tile" })
            {
                var tileId = new TileDefinitionId(reader.GetAttribute("id") ?? throw new ResourceLoadException());
                var spriteId = new SpriteId(reader.GetAttribute("spriteId") ?? throw new ResourceLoadException());

                reader.ReadToFollowing("CollisionBox");
                var collisionBox = new Rectangle(
                    int.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException()),
                    int.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException()),
                    int.Parse(reader.GetAttribute("width") ?? throw new ResourceLoadException()),
                    int.Parse(reader.GetAttribute("height") ?? throw new ResourceLoadException())
                );

                reader.ReadToFollowing("CollisionSides");
                string sides = reader.ReadElementContentAsString();

                List<BoxSide> collisionSides = sides
                    .Split(' ')
                    .Where(side => !string.IsNullOrWhiteSpace(side))
                    .Select(Enum.Parse<BoxSide>)
                    .ToList();

                var tileDefinition = new TileDefinition
                {
                    Sprite = spriteLibrary.GetSprite(spriteId),
                    CollisionBox = collisionBox,
                    CollisionSides = collisionSides
                };
                tileLibrary.AddTileDefinition(tileId, tileDefinition);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "TileLibrary" })
            {
                break;
            }
        }

        return tileLibrary;
    }
}