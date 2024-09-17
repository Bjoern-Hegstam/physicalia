using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Graphics.ParticleEngine;
using XNALibrary.Graphics.Sprites;
using XNALibrary.Interfaces;

namespace PhysicaliaRemastered.Actors.Bosses;

public class LambdaBoss : Boss
{
    private Sprite _spriteBody;
    private Sprite _spriteArm;
    private Sprite _spriteRail;
    private Sprite _spriteShot;

    public LambdaBoss()
    {
    }

    public override void LoadContent(ISpriteLibrary spriteLibrary)
    {
    }

    public override void CheckCollisions(Player player, Particle[] particles)
    {
    }

    public override void Update(GameTime gameTime)
    {
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 positionOffset)
    {
    }
}