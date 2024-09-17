using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using PhysicaliaRemastered.GameManagement;
using PhysicaliaRemastered.Input;
using XNALibrary.Graphics.ScreenManagement;

namespace PhysicaliaRemastered.Screens;

public class TitleScreen : Screen
{
    private ISettings _settings;

    public ISettings Settings
    {
        set => _settings = value;
    }

    public TitleScreen(Game game, ScreenManager screenManager)
        : base(game, screenManager)
    {
    }

    public override void LoadContent(ContentManager contentManager)
    {
        // TODO: Load Background
    }

    protected override void OnHandleInput()
    {
        if (_settings.InputMap.IsPressed(InputAction.MenuStart))
            ScreenManager.TransitionTo(typeof(MenuScreen));
    }
}