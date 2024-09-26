using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Graphics;

namespace XNALibrary.Animation;

public static class AnimationLibraryLoader
{
    public static AnimationLibrary Load(string path, ContentManager contentManager)
    {
        using var reader = XmlReader.Create(path);
        return LoadXml(reader, contentManager);
    }

    private static AnimationLibrary LoadXml(XmlReader reader, ContentManager contentManager)
    {
        AnimationLibrary library = new();

        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "Animation") continue;

            AnimationDefinition animationDefinition = LoadAnimationDefinition(reader, contentManager);

            library.Add(animationDefinition);
        }

        return library;
    }

    private static AnimationDefinition LoadAnimationDefinition(XmlReader reader, ContentManager contentManager)
    {
        var id = new AnimationDefinitionId(reader.GetAttribute("id") ?? throw new ResourceLoadException());
        
        reader.ReadToFollowing("TextureId");
        var textureId = new TextureId(reader.ReadElementContentAsString());
        var texture = contentManager.Load<Texture2D>(textureId.AssetName);

        List<Frame> frames = [];
        reader.ReadToFollowing("Frames");
        while (reader is not {NodeType: XmlNodeType.EndElement, LocalName: "Frames"})
        {
            reader.Read();

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Frame" })
            {
                var x = int.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
                var y = int.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());
                var width = int.Parse(reader.GetAttribute("width") ?? throw new ResourceLoadException());
                var height = int.Parse(reader.GetAttribute("height") ?? throw new ResourceLoadException());

                var frame = new Frame(
                    texture,
                    new Rectangle(x, y, width, height),
                    new Point(width / 2, height / 2)
                );
                frames.Add(frame);
            }
        }
        
        reader.ReadToFollowing("FrameRate");
        var frameRate = int.Parse(reader.ReadElementContentAsString());

        reader.ReadToFollowing("Loop");
        var loop = bool.Parse(reader.ReadElementContentAsString());

        var animationDefinition = new AnimationDefinition(
            id,
            frames,
            frameRate,
            loop
        );

        return animationDefinition;
    }
}