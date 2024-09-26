using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Graphics;

public static class SpriteBatchExtensions
{
    public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rectangle, Color color)
    {
        var solidColorTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
        solidColorTexture.SetData([color]);

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
}