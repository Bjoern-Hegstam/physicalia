using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Sprites;

public class Sprite(Texture2D texture, Rectangle sourceRect)
{
    public Texture2D Texture { get;  } = texture;
    public Rectangle SourceRectangle { get; } = sourceRect;
}