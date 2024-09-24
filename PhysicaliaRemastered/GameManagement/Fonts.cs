using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PhysicaliaRemastered.GameManagement;

public class Fonts(ContentManager contentManager)
{
    public SpriteFont WorldQuote { get; } = contentManager.Load<SpriteFont>("Fonts/WorldQuoteFont");
    public SpriteFont WorldIndex { get; } = contentManager.Load<SpriteFont>("Fonts/WorldIndexFont");
    public SpriteFont LevelIndex { get; } = contentManager.Load<SpriteFont>("Fonts/LevelIndexFont");
    public SpriteFont PlayerDead { get; } = contentManager.Load<SpriteFont>("Fonts/PlayerDeadFont");
    public SpriteFont WeaponAmmo { get; } = contentManager.Load<SpriteFont>("Fonts/WeaponAmmoFont");
    public SpriteFont PauseMenu { get; } = contentManager.Load<SpriteFont>("Fonts/PauseMenuFont");
}