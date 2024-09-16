using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Input;

namespace PhysicaliaRemastered.Screens;

public class MenuScreen : XNALibrary.Graphics.Screen
{
    #region Constants

    // Distance of the menu items from the right edge
    private const float MENU_ITEM_RIGHT_PADDING = 25F;
    private const float MENU_ITEM_BOTTOM_PADDING = 70F;
    private const float MENU_ITEM_SPACING = 5F;

    private readonly Color MENU_SELECTED_COLOR = Color.SlateGray;
    private readonly Color MENU_COLOR = Color.White;

    #endregion

    #region Fields

    private ISettings settings;

    private string[] menuItems;
    private int menuIndex;

    private SpriteFont menuFont;

    private Texture2D titleTexture;
    private Texture2D backLevelTexture;

    private GameManager gameManager;

    #endregion

    public ISettings Settings
    {
        set { this.settings = value; }
    }

    public GameManager GameManager
    {
        set { this.gameManager = value; }
    }

    public MenuScreen(Game game, XNALibrary.Graphics.ScreenManager screenManager)
        : base(game, screenManager) { }

    public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
    {
            this.titleTexture = contentManager.Load<Texture2D>(@"Images\ScreenGraphics\screen_MenuTitle");
            this.backLevelTexture = contentManager.Load<Texture2D>(@"Images\ScreenGraphics\screen_MenuBackLevel");

            this.menuIndex = 0;
            this.menuItems = new string[3];
            this.menuItems[0] = "New Game";
            this.menuItems[1] = "Load Game";
            this.menuItems[2] = "Exit";

            this.menuFont = contentManager.Load<SpriteFont>(@"Fonts\MainMenuFont");
        }

    protected override void OnHandleInput()
    {
            if (this.settings.InputMap.IsPressed(InputAction.MenuDown))
            {
                this.menuIndex++;

                if (this.menuIndex >= this.menuItems.Length)
                    this.menuIndex = this.menuItems.Length - 1;
            }

            if (this.settings.InputMap.IsPressed(InputAction.MenuUp))
            {
                this.menuIndex--;

                if (this.menuIndex < 0)
                    this.menuIndex = 0;
            }

            if (this.settings.InputMap.IsPressed(InputAction.MenuStart))
            {
                string selectedItem = this.menuItems[this.menuIndex];

                switch (selectedItem)
                {
                    case "New Game":
                        this.gameManager.NewSession();
                        this.ScreenManager.TransitionTo(typeof(GameScreen));
                        break;
                    case "Load Game":
                        OpenFileDialog loadDialog = new OpenFileDialog();
                        loadDialog.AddExtension = true;
                        loadDialog.DefaultExt = ".xml";
                        loadDialog.Filter = ".xml | *.xml";
                        loadDialog.InitialDirectory = @"Content\GameData\Saves";

                        if (loadDialog.ShowDialog() == DialogResult.OK)
                        {
                            this.gameManager.LoadSession(GameSession.LoadFromXml(loadDialog.FileName));
                            this.ScreenManager.TransitionTo(typeof(GameScreen));
                        }
                        break;
                    case "Exit":
                        this.Game.Exit();
                        break;
                }
            }
        }

    protected override void OnDrawAfter(SpriteBatch spriteBatch)
    {
            // Draw backgrounds
            spriteBatch.Draw(this.backLevelTexture, Vector2.Zero, Color.White);
            spriteBatch.Draw(this.titleTexture, Vector2.Zero, Color.White);

            // Draw menu
            int screenWidth = this.Game.GraphicsDevice.Viewport.Width;
            int screenHeight = this.Game.GraphicsDevice.Viewport.Height;

            Vector2 textPos = new Vector2(0, screenHeight - MENU_ITEM_BOTTOM_PADDING);

            for (int i = this.menuItems.Length - 1; i >= 0; i--)
            {
                // Get the color of the item
                Color textColor = this.menuIndex == i ? MENU_SELECTED_COLOR : MENU_COLOR;

                // Get the position of the item
                Vector2 textSize = this.menuFont.MeasureString(this.menuItems[i]);
                textPos.X = screenWidth - MENU_ITEM_RIGHT_PADDING - textSize.X;
                textPos.Y -= textSize.Y;

                // Draw the item
                spriteBatch.DrawString(this.menuFont, this.menuItems[i], textPos, textColor);

                // Offset in Y for the next item
                textPos.Y -= MENU_ITEM_SPACING;
            }
        }
}