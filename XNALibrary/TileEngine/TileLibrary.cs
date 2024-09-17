using System.Xml;
using Microsoft.Xna.Framework;
using XNALibrary.Animation;
using XNALibrary.Sprites;

namespace XNALibrary.TileEngine;

public class TileLibrary : ITileLibrary
{
    private readonly Dictionary<int, Tile> _tileLibrary;

    public TileLibrary()
    {
        _tileLibrary = new Dictionary<int, Tile>();
    }

    /// <summary>
    /// Adds the specified key and Tile to the TileLibrary.
    /// </summary>
    /// <param name="key">The key to the Tile to add.</param>
    /// <param name="tile">The Tile to add.</param>
    /// <returns>True if the Tile was succesfully added; false otherwise.</returns>
    public bool AddTile(int key, Tile tile)
    {
        if (_tileLibrary.TryAdd(key, tile))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes the Tile with the specified key.
    /// </summary>
    /// <param name="key">The key to the Tile to remove.</param>
    /// <returns>True if the Tile was succesfully removed; false otherwise.</returns>
    public bool RemoveTile(int key)
    {
        return _tileLibrary.Remove(key);
    }

    public Tile GetTile(int key)
    {
        if (_tileLibrary.TryGetValue(key, out var tile))
        {
            return tile;
        }

        throw new MissingTileException(); 
    }

    public bool ContainsKey(int key)
    {
        return _tileLibrary.ContainsKey(key);
    }

    public void Clear()
    {
        _tileLibrary.Clear();
    }

    public void LoadXml(string path, SpriteLibrary spriteLibrary, IAnimationManager animationManager)
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

    public void LoadXml(XmlReader reader, SpriteLibrary spriteLibrary, IAnimationManager animationManager)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Tile" })
            {
                Tile tile;
                int id = int.Parse(reader.GetAttribute("id") ?? throw new ResourceLoadException());

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
                    for (var i = 0; i < sides.Length; i++)
                    {
                        var side = (BoxSide)Enum.Parse(typeof(BoxSide), sides[i]);
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