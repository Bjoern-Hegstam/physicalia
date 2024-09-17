using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.GameManagement;
using PhysicaliaRemastered.Input;
using XNALibrary.ScreenManagement;

namespace PhysicaliaRemastered.Screens;

public class MenuScreen : Screen
{
    // Distance of the menu items from the right edge
    private const float MenuItemRightPadding = 25F;
    private const float MenuItemBottomPadding = 70F;
    private const float MenuItemSpacing = 5F;

    private readonly Color _menuSelectedColor = Color.SlateGray;
    private readonly Color _menuColor = Color.White;

    private Settings _settings;

    private string[] _menuItems;
    private int _menuIndex;

    private SpriteFont _menuFont;

    private Texture2D _titleTexture;
    private Texture2D _backLevelTexture;

    private GameManager _gameManager;

    public Settings Settings
    {
        set => _settings = value;
    }

    public GameManager GameManager
    {
        set => _gameManager = value;
    }

    public MenuScreen(Game game, ScreenManager screenManager)
        : base(game, screenManager)
    {
    }

    public override void LoadContent(ContentManager contentManager)
    {
        _titleTexture = contentManager.Load<Texture2D>(@"Images\ScreenGraphics\screen_MenuTitle");
        _backLevelTexture = contentManager.Load<Texture2D>(@"Images\ScreenGraphics\screen_MenuBackLevel");

        _menuIndex = 0;
        _menuItems = new string[3];
        _menuItems[0] = "New Game";
        _menuItems[1] = "Load Game";
        _menuItems[2] = "Exit";

        _menuFont = contentManager.Load<SpriteFont>(@"Fonts\MainMenuFont");
    }

    protected override void OnHandleInput()
    {
        if (_settings.InputMap.IsPressed(InputAction.MenuDown))
        {
            _menuIndex++;

            if (_menuIndex >= _menuItems.Length)
            {
                _menuIndex = _menuItems.Length - 1;
            }
        }

        if (_settings.InputMap.IsPressed(InputAction.MenuUp))
        {
            _menuIndex--;

            if (_menuIndex < 0)
            {
                _menuIndex = 0;
            }
        }

        if (_settings.InputMap.IsPressed(InputAction.MenuStart))
        {
            string selectedItem = _menuItems[_menuIndex];

            switch (selectedItem)
            {
                case "New Game":
                    _gameManager.NewSession();
                    ScreenManager.TransitionTo(typeof(GameScreen));
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
                        gameManager.LoadSession(GameSession.LoadFromXml(loadDialog.FileName));
                        ScreenManager.TransitionTo(typeof(GameScreen));
                    }
                    */
                    break;
                case "Exit":
                    Game.Exit();
                    break;
            }
        }
    }

    protected override void OnDraw(SpriteBatch? spriteBatch)
    {
        // Draw backgrounds
        spriteBatch.Draw(_backLevelTexture, Vector2.Zero, Color.White);
        spriteBatch.Draw(_titleTexture, Vector2.Zero, Color.White);

        // Draw menu
        int screenWidth = Game.GraphicsDevice.Viewport.Width;
        int screenHeight = Game.GraphicsDevice.Viewport.Height;

        var textPos = new Vector2(0, screenHeight - MenuItemBottomPadding);

        for (int i = _menuItems.Length - 1; i >= 0; i--)
        {
            // Get the color of the item
            Color textColor = _menuIndex == i ? _menuSelectedColor : _menuColor;

            // Get the position of the item
            Vector2 textSize = _menuFont.MeasureString(_menuItems[i]);
            textPos.X = screenWidth - MenuItemRightPadding - textSize.X;
            textPos.Y -= textSize.Y;

            // Draw the item
            spriteBatch.DrawString(_menuFont, _menuItems[i], textPos, textColor);

            // Offset in Y for the next item
            textPos.Y -= MenuItemSpacing;
        }
    }
}