using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.GameManagement;
using PhysicaliaRemastered.Input;
using XNALibrary.ScreenManagement;

namespace PhysicaliaRemastered.Screens;

public class MainMenuScreen(Game game, GameManager gameManager, ScreenManager screenManager) : Screen
{
    private const float MenuItemRightPadding = 25F;
    private const float MenuItemBottomPadding = 70F;
    private const float MenuItemSpacing = 5F;

    private readonly Color _menuSelectedColor = Color.SlateGray;
    private readonly Color _menuColor = Color.White;

    private readonly List<string> _menuItems =
    [
        "New Game",
        "Load Game",
        "Exit"
    ];

    private int _selectedMenuItemIndex;

    private SpriteFont MenuFont => game.Content.Load<SpriteFont>("Fonts/MainMenuFont");

    private Texture2D TitleTexture => game.Content.Load<Texture2D>("Images/ScreenGraphics/screen_MenuTitle");
    private Texture2D BackLevelTexture => game.Content.Load<Texture2D>("Images/ScreenGraphics/screen_MenuBackLevel");

    private Settings Settings => game.Services.GetService<Settings>();

    protected override void OnHandleInput()
    {
        if (Settings.InputMap.IsPressed(InputAction.MenuDown))
        {
            SelectNextMenuItem();
        } else if (Settings.InputMap.IsPressed(InputAction.MenuUp))
        {
            SelectPreviousMenuItem();
        } else if (Settings.InputMap.IsPressed(InputAction.MenuStart))
        {
            InvokeSelectedMenuItem();
        }
    }

    private void SelectNextMenuItem()
    {
        _selectedMenuItemIndex = (_selectedMenuItemIndex + 1) % _menuItems.Count;
    }

    private void SelectPreviousMenuItem()
    {
        _selectedMenuItemIndex = (_selectedMenuItemIndex - 1 + _menuItems.Count) % _menuItems.Count;
    }

    private void InvokeSelectedMenuItem()
    {
        switch (_menuItems[_selectedMenuItemIndex])
        {
            case "New Game":
                gameManager.NewGame();
                screenManager.TransitionTo(typeof(GameScreen));
                break;
            case "Load Game":
                /*
                    OpenFileDialog loadDialog = new OpenFileDialog();
                    loadDialog.AddExtension = true;
                    loadDialog.DefaultExt = ".xml";
                    loadDialog.Filter = ".xml | *.xml";
                    loadDialog.InitialDirectory = @"Content\GameData\Saves";

                    if (loadDialog.ShowDialog() == DialogResult.OK)
                    {
                        gameManager.LoadGame(SaveGame.LoadFromXml(loadDialog.FileName));
                        ScreenManager.TransitionTo(typeof(GameScreen));
                    }
                    */
                break;
            case "Exit":
                game.Exit();
                break;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin();
        
        DrawBackground(spriteBatch);
        DrawMenu(spriteBatch);
        
        spriteBatch.End();
    }

    private void DrawBackground(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(BackLevelTexture, Vector2.Zero, Color.White);
        spriteBatch.Draw(TitleTexture, Vector2.Zero, Color.White);
    }

    private void DrawMenu(SpriteBatch spriteBatch)
    {
        int screenWidth = game.GraphicsDevice.Viewport.Width;
        int screenHeight = game.GraphicsDevice.Viewport.Height;

        var textPos = new Vector2(0, screenHeight - MenuItemBottomPadding);

        for (int i = _menuItems.Count - 1; i >= 0; i--)
        {
            Color textColor = _selectedMenuItemIndex == i ? _menuSelectedColor : _menuColor;

            Vector2 textSize = MenuFont.MeasureString(_menuItems[i]);
            textPos.X = screenWidth - MenuItemRightPadding - textSize.X;
            textPos.Y -= textSize.Y;

            spriteBatch.DrawString(MenuFont, _menuItems[i], textPos, textColor);

            textPos.Y -= MenuItemSpacing;
        }
    }
}