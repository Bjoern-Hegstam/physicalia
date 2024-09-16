using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using PhysicaliaRemastered.GameManagement;
using PhysicaliaRemastered.Input;

namespace PhysicaliaRemastered.Screens;

public class TitleScreen : XNALibrary.Graphics.Screen
{
    private ISettings settings;

    public ISettings Settings
    {
        set => settings = value;
    }

    public TitleScreen(Game game, XNALibrary.Graphics.ScreenManager screenManager)
        : base(game, screenManager)
    {

        }

    public override void LoadContent(ContentManager contentManager)
    {
            // TODO: Load Background
        }

    protected override void OnHandleInput()
    {
            if (settings.InputMap.IsPressed(InputAction.MenuStart))
                this.ScreenManager.TransitionTo(typeof(MenuScreen));
        }
}