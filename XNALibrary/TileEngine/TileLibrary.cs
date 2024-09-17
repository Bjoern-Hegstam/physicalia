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
        if (!_tileLibrary.ContainsKey(key))
        {
            _tileLibrary.Add(key, tile);
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

    /// <summary>
    /// Gets the Tile with the specified key.
    /// </summary>
    /// <param name="key">The key to the Tile to get.</param>
    /// <returns>The wanted Tile or null if no matching Tile was found.</returns>
    public Tile GetTile(int key)
    {
        if (_tileLibrary.ContainsKey(key))
            return _tileLibrary[key];

        return null;
    }

    /// <summary>
    /// Checks whether the TileLibrary contains the specified key.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the TileLibrary contains the specified key; false otherwise.</returns>
    public bool ContainsKey(int key)
    {
        return _tileLibrary.ContainsKey(key);
    }

    /// <summary>
    /// Removes all keys and Tiles from the TileLibrary.
    /// </summary>
    public void Clear()
    {
        _tileLibrary.Clear();
    }

    public void LoadXml(string path, ISpriteLibrary spriteLibrary, IAnimationManager animationManager)
    {
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.CloseInput = true;
        settings.IgnoreComments = true;
        settings.IgnoreWhitespace = true;
        settings.CheckCharacters = true;

        using XmlReader reader = XmlReader.Create(path, settings);
        LoadXml(reader, spriteLibrary, animationManager);
    }

    public void LoadXml(XmlReader reader, ISpriteLibrary spriteLibrary, IAnimationManager animationManager)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Tile")
            {
                Tile tile;
                int id = int.Parse(reader.GetAttribute("id"));

                if (reader.GetAttribute("textureType") == "Animation")
                {
                    int animationKey = int.Parse(reader.GetAttribute("textureKey"));
                    Animation.Animation animation = animationManager.GetBankAnimation(animationKey).Copy();
                    animationManager.AddPlaybackAnimation(animation);
                    tile = new AnimatedTile(animation);
                }
                else
                {
                    int spriteKey = int.Parse(reader.GetAttribute("textureKey"));
                    Sprite sprite = spriteLibrary.GetSprite(spriteKey);

                    tile = new SpriteTile(sprite);
                }

                // Get the Tile's collisionbox
                reader.ReadToFollowing("CollisionBox");

                int x = int.Parse(reader.GetAttribute("x"));
                int y = int.Parse(reader.GetAttribute("y"));
                int width = int.Parse(reader.GetAttribute("width"));
                int height = int.Parse(reader.GetAttribute("height"));

                tile.CollisionBox = new Rectangle(x, y, width, height);

                // Get the collision sides of the Tile
                reader.ReadToFollowing("CollisionSides");

                String[] sides = reader.ReadElementContentAsString().Split(' ');

                if (sides.Length > 0 && sides[0] != "")
                    for (int i = 0; i < sides.Length; i++)
                    {
                        BoxSide side = (BoxSide)Enum.Parse(typeof(BoxSide), sides[i]);
                        tile.CollisionSides |= side;
                    }

                // Store the Tile
                _tileLibrary.Add(id, tile);
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "TileLibrary")
                return;
        }
    }
}