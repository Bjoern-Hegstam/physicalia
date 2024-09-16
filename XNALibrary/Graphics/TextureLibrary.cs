using System;
using System.Collections.Generic;
using System.Text;
using XNALibrary.Services;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;

namespace XNALibrary.Graphics;

public class TextureLibrary : ITextureLibrary
{
    private Dictionary<int, Texture2D> textureLibrary;

    public Texture2D this[int key]
    {
        get { return this.GetTexture(key); }
    }

    public TextureLibrary()
    {
        this.textureLibrary = new Dictionary<int, Texture2D>();
    }

    public bool AddTexture(int key, Texture2D texture)
    {
        if (!this.textureLibrary.ContainsKey(key))
        {
            this.textureLibrary.Add(key, texture);
            return true;
        }

        return false;
    }

    public bool RemoveTexture(int key)
    {
        return this.textureLibrary.Remove(key);
    }

    public Texture2D GetTexture(int key)
    {
        if (this.textureLibrary.ContainsKey(key))
            return this.textureLibrary[key];

        return null;
    }

    public bool ContainsKey(int key)
    {
        return this.textureLibrary.ContainsKey(key);
    }

    public void Clear()
    {
        this.textureLibrary.Clear();
    }

    public void LoadXml(string path, GraphicsDevice graphics)
    {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.IgnoreComments = true;
        readerSettings.IgnoreProcessingInstructions = true;
        readerSettings.IgnoreWhitespace = true;

        using (XmlReader reader = XmlReader.Create(path, readerSettings))
        {
            this.LoadXml(reader, graphics);
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
                this.AddTexture(id, texture);
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "TextureLibrary")
                return;
        }
    }
}