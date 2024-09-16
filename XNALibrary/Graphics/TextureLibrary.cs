using System.Xml;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Interfaces;

namespace XNALibrary.Graphics;

public class TextureLibrary : ITextureLibrary
{
    private Dictionary<int, Texture2D> textureLibrary;

    public Texture2D this[int key]
    {
        get { return GetTexture(key); }
    }

    public TextureLibrary()
    {
        textureLibrary = new Dictionary<int, Texture2D>();
    }

    public bool AddTexture(int key, Texture2D texture)
    {
        if (!textureLibrary.ContainsKey(key))
        {
            textureLibrary.Add(key, texture);
            return true;
        }

        return false;
    }

    public bool RemoveTexture(int key)
    {
        return textureLibrary.Remove(key);
    }

    public Texture2D GetTexture(int key)
    {
        if (textureLibrary.ContainsKey(key))
            return textureLibrary[key];

        return null;
    }

    public bool ContainsKey(int key)
    {
        return textureLibrary.ContainsKey(key);
    }

    public void Clear()
    {
        textureLibrary.Clear();
    }

    public void LoadXml(string path, GraphicsDevice graphics)
    {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.IgnoreComments = true;
        readerSettings.IgnoreProcessingInstructions = true;
        readerSettings.IgnoreWhitespace = true;

        using (XmlReader reader = XmlReader.Create(path, readerSettings))
        {
            LoadXml(reader, graphics);
        }
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