using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.ParticleEngine;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.Actors.Bosses;

public abstract class Boss
{
    public float Health { get; set; }

    public Boss()
    {
    }

    public abstract void LoadContent(ISpriteLibrary spriteLibrary);

    public abstract void CheckCollisions(Player player, Particle[] particles);

    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch, Vector2 positionOffset);
}