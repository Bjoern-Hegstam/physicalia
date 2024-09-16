using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Input;

namespace PhysicaliaRemastered.Screens;

public class GameScreen : XNALibrary.Graphics.Screen
{
    public enum PauseMenuOption
    {
        Resume,
        Reset,
        Load,
        Save,
        Menu
    }

    #region Constants

    private const float PAUSE_MENU_Y = 165F;
    private const float PAUSE_MENU_X = 250F;
    private const float PAUSE_MENU_MARGIN = 5F;

    #endregion

    #region Fields

    private GameManager gameManager;
    private ISettings settings;

    // Pause menu fields
    private Texture2D pauseOverlayTexture;
    private Rectangle pauseOverlayArea;

    private int pauseMenuIndex = 0;

    private PauseMenuOption[] pauseMenuOptions = (PauseMenuOption[])Enum.GetValues(typeof(PauseMenuOption));

    #endregion

    public ISettings Settings
    {
        get { return this.gameManager.Settings; }
    }

    public GameManager GameManager
    {
        get { return this.gameManager; }
    }

    public GameScreen(Game game, ScreenManager manager)
        : base(game, manager)
    {
            this.gameManager = new GameManager(this.Game);
            this.settings = this.gameManager.Settings;
        }

    #region Initilization and content loading

    public override void Initialize()
    {
            base.Initialize();

            // Create Pause overlay texture
            this.pauseOverlayTexture = new Texture2D(this.Game.GraphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            this.pauseOverlayTexture.SetData<Color>(new Color[] { new Color(0, 0, 0, 128) });

            this.pauseOverlayArea = new Rectangle();
            this.pauseOverlayArea.Width = this.Game.GraphicsDevice.Viewport.Width;
            this.pauseOverlayArea.Height = this.Game.GraphicsDevice.Viewport.Height;
        }

    public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
    {
            this.gameManager.LoadXml(@"Content\GameData\Game.xml");
            this.gameManager.LoadContent(contentManager);
        }

    #endregion

    protected override void OnUpdate(GameTime gameTime)
    {
            if (this.gameManager.State == GameState.Paused)
                this.HandlePauseMenu();

            // Always update the game
            this.gameManager.Update(gameTime);
        }

    public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
    {
            spriteBatch.Begin();

            this.gameManager.Draw(spriteBatch);

            if (this.gameManager.State == GameState.Paused)
                this.DrawPauseMenu(spriteBatch);

            spriteBatch.End();
        }

    private void HandlePauseMenu()
    {
            // Read input for moving between meny options
            // Because of the way the options are drawn the index goes in the
            // reverse direction of what the player presses
            if (this.settings.InputMap.IsPressed(InputAction.MenuUp))
            {
                this.pauseMenuIndex--;

                if (this.pauseMenuIndex < 0)
                    this.pauseMenuIndex = 0;
            }

            if (this.settings.InputMap.IsPressed(InputAction.MenuDown))
            {
                this.pauseMenuIndex++;

                if (this.pauseMenuIndex >= this.pauseMenuOptions.Length)
                    this.pauseMenuIndex = this.pauseMenuOptions.Length - 1;
            }

            // Check if MenuStart is pressed and take appropriate action
            if (this.settings.InputMap.IsPressed(InputAction.MenuStart))
            {
                PauseMenuOption selectedOption = this.pauseMenuOptions[this.pauseMenuIndex];

                switch (selectedOption)
                {
                    case PauseMenuOption.Resume:
                        this.gameManager.NextState = GameState.Playing;
                        break;
                    case PauseMenuOption.Reset:
                        this.gameManager.ResetLevel();
                        this.pauseMenuIndex = 0;
                        break;
                    case PauseMenuOption.Load:
                        OpenFileDialog loadDialog = new OpenFileDialog();
                        loadDialog.AddExtension = true;
                        loadDialog.DefaultExt = ".xml";
                        loadDialog.Filter = ".xml | *.xml";
                        loadDialog.InitialDirectory = @"Content\GameData\Saves";

                        if (loadDialog.ShowDialog() == DialogResult.OK)
                        {
                            this.gameManager.LoadSession(GameSession.LoadFromXml(loadDialog.FileName));
                            this.pauseMenuIndex = 0;
                        }
                        break;
                    case PauseMenuOption.Save:
                        SaveFileDialog saveDialog = new SaveFileDialog();
                        saveDialog.AddExtension = true;
                        saveDialog.DefaultExt = ".xml";
                        saveDialog.Filter = ".xml | *.xml";
                        saveDialog.InitialDirectory = @"Content\GameData\Saves";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            this.gameManager.SaveSession().SaveToXml(saveDialog.FileName);
                            this.pauseMenuIndex = 0;
                        }
                        break;
                    case PauseMenuOption.Menu:
                        this.ScreenManager.TransitionBack();
                        break;
                    default:
                        break;
                }
            }
        }

    private void DrawPauseMenu(SpriteBatch spriteBatch)
    {
            // Pause menu is drawn in the center of the screen

            // Draw texture overlay over world to fade it out a bit
            if (this.pauseOverlayTexture != null)
                spriteBatch.Draw(this.pauseOverlayTexture, this.pauseOverlayArea, Color.White);

            // Draw pause menu base

            // TODO: Mirror top graphics for bottom?

            // Draw pause menu text
            SpriteFont pauseFont = this.settings.PauseMenuFont;
            Vector2 textPos = new Vector2(PAUSE_MENU_X, PAUSE_MENU_Y);

            float pauseTextHeight = pauseFont.MeasureString("42").Y;

            int optionIndex = 0;
            foreach (PauseMenuOption menuOption in Enum.GetValues(typeof(PauseMenuOption)))
            {
                spriteBatch.DrawString(pauseFont, menuOption.ToString(), textPos, this.pauseMenuIndex == optionIndex ? Color.Yellow : Color.White);

                // Move next item down
                textPos.Y += pauseTextHeight + PAUSE_MENU_MARGIN;

                optionIndex++;
            }
        }
}