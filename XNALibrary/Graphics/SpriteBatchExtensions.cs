using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Graphics;

public static class SpriteBatchExtensions
{
    public static void DrawRectangle(this SpriteBatch spriteBatch, Vector2 position, Rectangle rectangle, Color color,
        Vector2 origin, SpriteEffects effects)
    {
        var collisionBoxLineTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
        collisionBoxLineTexture.SetData([Color.Red]);

        const int outlineThickness = 1;

        spriteBatch.Draw(
            collisionBoxLineTexture,
            position,
            new Rectangle(0, 0, rectangle.Width, outlineThickness),
            Color.White,
            0F,
            origin,
            1F,
            effects,
            0.7F
        );

        spriteBatch.Draw(
            collisionBoxLineTexture,
            position + new Vector2 { X = rectangle.Width },
            new Rectangle(0, 0, outlineThickness, rectangle.Height),
            Color.White,
            0F,
            origin,
            1F,
            effects,
            0.7F
        );

        spriteBatch.Draw(
            collisionBoxLineTexture,
            position + new Vector2 { Y = rectangle.Height },
            new Rectangle(0, 0, rectangle.Width, outlineThickness),
            Color.White,
            0F,
            origin,
            1F,
            effects,
            0.7F
        );

        spriteBatch.Draw(
            collisionBoxLineTexture,
            position,
            new Rectangle(0, 0, outlineThickness, rectangle.Height),
            Color.White,
            0F,
            origin,
            1F,
            effects,
            0.7F
        );
    }
}