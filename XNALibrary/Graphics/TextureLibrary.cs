using System.Xml;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Graphics;

public class TextureLibrary : ITextureLibrary
{
    private readonly Dictionary<int, Texture2D> _textureLibrary;

    public Texture2D this[int key] => GetTexture(key);

    public TextureLibrary()
    {
        _textureLibrary = new Dictionary<int, Texture2D>();
    }

    public bool AddTexture(int key, Texture2D texture)
    {
        if (!_textureLibrary.ContainsKey(key))
        {
            _textureLibrary.Add(key, texture);
            return true;
        }

        return false;
    }

    public bool RemoveTexture(int key)
    {
        return _textureLibrary.Remove(key);
    }

    public Texture2D GetTexture(int key)
    {
        if (_textureLibrary.ContainsKey(key))
            return _textureLibrary[key];

        return null;
    }

    public bool ContainsKey(int key)
    {
        return _textureLibrary.ContainsKey(key);
    }

    public void Clear()
    {
        _textureLibrary.Clear();
    }

    public void LoadXml(string path, GraphicsDevice graphics)
    {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.IgnoreComments = true;
        readerSettings.IgnoreProcessingInstructions = true;
        readerSettings.IgnoreWhitespace = true;

        using XmlReader reader = XmlReader.Create(path, readerSettings);
        LoadXml(reader, graphics);
    }

    public void LoadXml(XmlReader reader, GraphicsDevice graphics)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Texture")
            {
                int id = int.Parse(reader.GetAttribute("id"));

                Texture2D texture = Texture2D.FromFile(graphics, reader.ReadString());
                AddTexture(id, texture);
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "TextureLibrary")
                return;
        }
    }
}