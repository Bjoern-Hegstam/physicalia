using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhysicaliaRemastered.Actors.Bosses;

public abstract class Boss
{
    private float health;

    public float Health
    {
        get => health;
        set => health = value;
    }

    public Boss()
    {

        }

    public abstract void LoadContent(ISpriteLibrary spriteLibrary);

    public abstract void CheckCollisions(Player player, Particle[] particles);

    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch, Vector2 positionOffset);
}