using System.Xml;
using Microsoft.Xna.Framework;
using XNALibrary.Animation;
using XNALibrary.Sprites;

namespace XNALibrary.TileEngine;

public class TileLibrary
{
    private readonly Dictionary<TileId, Tile> _tileLibrary = new();

    public Tile GetTile(TileId id)
    {
        return _tileLibrary[id];
    }

    public void LoadXml(string path, SpriteLibrary spriteLibrary, AnimationManager animationManager)
    {
        var settings = new XmlReaderSettings
        {
            CloseInput = true,
            IgnoreComments = true,
            IgnoreWhitespace = true,
            CheckCharacters = true
        };

        using var reader = XmlReader.Create(path, settings);
        LoadXml(reader, spriteLibrary, animationManager);
    }

    public void LoadXml(XmlReader reader, SpriteLibrary spriteLibrary, AnimationManager animationManager)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Tile" })
            {
                var tileId = new TileId(reader.GetAttribute("id") ?? throw new ResourceLoadException());
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

                BoxSide collisionSides = sides
                    .Split(' ')
                    .Where(side => !string.IsNullOrWhiteSpace(side))
                    .Select(Enum.Parse<BoxSide>)
                    .Aggregate(BoxSide.None, (current, side) => current | side);

                Sprite sprite = spriteLibrary.GetSprite(spriteId);
                var tile = new Tile(sprite, collisionBox, collisionSides);
                _tileLibrary.Add(tileId, tile);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "TileLibrary" })
            {
                return;
            }
        }
    }
}