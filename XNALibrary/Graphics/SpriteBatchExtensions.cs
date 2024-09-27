using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Graphics;

public static class SpriteBatchExtensions
{
    private static readonly Dictionary<Color, Texture2D> ColorTextureCache = [];
    
    public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rectangle, Color color)
    {
        if (!ColorTextureCache.TryGetValue(color, out Texture2D? solidColorTexture))
        {
            solidColorTexture = CreateColorTexture(spriteBatch.GraphicsDevice, color);
            ColorTextureCache.Add(color, solidColorTexture);
        }
        
        const int outlineThickness = 1;

        spriteBatch.Draw(
            solidColorTexture,
            rectangle.Location.ToVector2(),
            new Rectangle(0, 0, rectangle.Width, outlineThickness),
            Color.White
        );

        spriteBatch.Draw(
            solidColorTexture,
            rectangle.Location.ToVector2() + new Vector2 { X = rectangle.Width },
            new Rectangle(0, 0, outlineThickness, rectangle.Height),
            Color.White
        );

        spriteBatch.Draw(
            solidColorTexture,
            rectangle.Location.ToVector2() + new Vector2 { Y = rectangle.Height },
            new Rectangle(0, 0, rectangle.Width, outlineThickness),
            Color.White
        );

        spriteBatch.Draw(
            solidColorTexture,
            rectangle.Location.ToVector2(),
            new Rectangle(0, 0, outlineThickness, rectangle.Height),
            Color.White
        );
    }
    
    private static Texture2D CreateColorTexture(GraphicsDevice graphicsDevice, Color color)
    {
        var texture = new Texture2D(graphicsDevice, 1, 1);
        texture.SetData([color]);
        return texture;
    }

}