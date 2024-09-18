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
                var id = new TileId(int.Parse(reader.GetAttribute("id") ?? throw new ResourceLoadException()));

                Tile tile;
                if (reader.GetAttribute("textureType") == "Animation")
                {
                    int animationKey = int.Parse(reader.GetAttribute("textureKey") ?? throw new ResourceLoadException());
                    Animation.Animation animation = animationManager.GetBankAnimation(animationKey).Copy();
                    animationManager.AddPlaybackAnimation(animation);
                    tile = new AnimatedTile(animation);
                }
                else
                {
                    int spriteKey = int.Parse(reader.GetAttribute("textureKey") ?? throw new ResourceLoadException());
                    Sprite sprite = spriteLibrary.GetSprite(spriteKey);

                    tile = new SpriteTile(sprite);
                }

                // Get the Tile's collision box
                reader.ReadToFollowing("CollisionBox");

                int x = int.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
                int y = int.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());
                int width = int.Parse(reader.GetAttribute("width") ?? throw new ResourceLoadException());
                int height = int.Parse(reader.GetAttribute("height") ?? throw new ResourceLoadException());

                tile.CollisionBox = new Rectangle(x, y, width, height);

                // Get the collision sides of the Tile
                reader.ReadToFollowing("CollisionSides");

                string[] sides = reader.ReadElementContentAsString().Split(' ');

                if (sides.Length > 0 && sides[0] != "")
                {
                    foreach (string sideString in sides)
                    {
                        var side = (BoxSide)Enum.Parse(typeof(BoxSide), sideString);
                        tile.CollisionSides |= side;
                    }
                }

                // Store the Tile
                _tileLibrary.Add(id, tile);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "TileLibrary" })
            {
                return;
            }
        }
    }
}