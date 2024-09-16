using Microsoft.Xna.Framework;
using PhysicaliaRemastered.Input;

namespace PhysicaliaRemastered.Screens;

public class TitleScreen : XNALibrary.Graphics.Screen
{
    #region Fields

    private ISettings settings;

    #endregion

    public ISettings Settings
    {
        set { this.settings = value; }
    }

    public TitleScreen(Game game, XNALibrary.Graphics.ScreenManager screenManager)
        : base(game, screenManager)
    {

        }

    public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
    {
            // TODO: Load Background
        }

    protected override void OnHandleInput()
    {
            if (this.settings.InputMap.IsPressed(InputAction.MenuStart))
                this.ScreenManager.TransitionTo(typeof(MenuScreen));
        }
}