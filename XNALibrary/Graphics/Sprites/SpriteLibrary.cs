using System.Xml;
using Microsoft.Xna.Framework;
using XNALibrary.Interfaces;

namespace XNALibrary.Graphics.Sprites;

/// <summary>
/// Manages a collection of sprites and related sprite sheets.
/// </summary>
public class SpriteLibrary : ISpriteLibrary
{
    /// <summary>
    /// The library's collection of sprites.
    /// </summary>
    private readonly Dictionary<int, Sprite> _sprites;

    private readonly ITextureLibrary _textureLibrary;

    /// <summary>
    /// Gets the library's collection of sprites.
    /// </summary>
    public Dictionary<int, Sprite> Sprites => _sprites;

    /// <summary>
    /// Creates a new SpriteLibrary.
    /// </summary>
    public SpriteLibrary(ITextureLibrary textureLibrary)
    {
        _textureLibrary = textureLibrary;

        _sprites = new Dictionary<int, Sprite>();
    }

    /// <summary>
    /// Adds a Sprite to the SpriteLibrary. If the Texture property has not
    /// been set on the sprite, it will be set to the value specified by
    /// the Sprite's TextureKey property.
    /// </summary>
    /// <param name="key">Key to set to the sprite.</param>
    /// <param name="sprite">Sprite to add.</param>
    public void AddSprite(int key, Sprite sprite)
    {
        _sprites.Add(key, sprite);
    }

    /// <summary>
    /// Removes the specified Sprite from the SpriteLibrary.
    /// </summary>
    /// <param name="key"></param>
    public void RemoveSprite(int key)
    {
        if (_sprites.ContainsKey(key))
            _sprites.Remove(key);
    }

    /// <summary>
    /// Gets the specified Sprite from the SpriteLibrary.
    /// </summary>
    /// <param name="key">Key to the Sprite to get.</param>
    /// <returns>The desired Sprite if it's found.</returns>
    public Sprite GetSprite(int key)
    {
        return _sprites[key];
    }

    /// <summary>
    /// Reads in Sprite data from an Xml file.
    /// </summary>
    /// <param name="path">Path to the Xml file.</param>
    public void LoadXml(String path)
    {
        if (path == null)
            throw new ArgumentNullException("Path to Xml file cannot be null!");

        if (path.Length < 5 || path.Substring(path.Length - 3, 3).ToLower() != "xml")
            throw new ArgumentException("File is not of type '.xml'");

        XmlReaderSettings settings = new XmlReaderSettings();
        settings.IgnoreComments = true;
        settings.IgnoreProcessingInstructions = true;

        using XmlReader reader = XmlReader.Create(path, settings);
        LoadXml(reader);
    }

    public void LoadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            // Sprite node ?
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Sprite")
            {
                int id = int.Parse(reader.GetAttribute(0));
                int textureKey = int.Parse(reader.GetAttribute(1));
                int x = int.Parse(reader.GetAttribute(2));
                int y = int.Parse(reader.GetAttribute(3));
                int width = int.Parse(reader.GetAttribute(4));
                int height = int.Parse(reader.GetAttribute(5));

                _sprites.Add(id, new Sprite(_textureLibrary[textureKey], new Rectangle(x, y, width, height)));
            }

            // End of SpriteLibrary element
            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "SpriteLibrary")
            {
                return;
            }
        }
    }
}