using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.GameManagement;
using PhysicaliaRemastered.Input;
using XNALibrary.Graphics.ScreenManagement;

namespace PhysicaliaRemastered.Screens;

public class GameScreen : Screen
{
    public enum PauseMenuOption
    {
        Resume,
        Reset,
        Load,
        Save,
        Menu
    }

    private const float PAUSE_MENU_Y = 165F;
    private const float PAUSE_MENU_X = 250F;
    private const float PAUSE_MENU_MARGIN = 5F;

    private GameManager gameManager;
    private ISettings settings;

    // Pause menu fields
    private Texture2D pauseOverlayTexture;
    private Rectangle pauseOverlayArea;

    private int pauseMenuIndex = 0;

    private PauseMenuOption[] pauseMenuOptions = (PauseMenuOption[])Enum.GetValues(typeof(PauseMenuOption));

    public ISettings Settings
    {
        get { return gameManager.Settings; }
    }

    public GameManager GameManager
    {
        get { return gameManager; }
    }

    public GameScreen(Game game, ScreenManager manager)
        : base(game, manager)
    {
            gameManager = new GameManager(Game);
            settings = gameManager.Settings;
        }

    public override void Initialize()
    {
            base.Initialize();

            // Create Pause overlay texture
            pauseOverlayTexture = new Texture2D(Game.GraphicsDevice, 1, 1, true, SurfaceFormat.Color);
            pauseOverlayTexture.SetData<Color>(new Color[] { new Color(0, 0, 0, 128) });

            pauseOverlayArea = new Rectangle();
            pauseOverlayArea.Width = Game.GraphicsDevice.Viewport.Width;
            pauseOverlayArea.Height = Game.GraphicsDevice.Viewport.Height;
        }

    public override void LoadContent(ContentManager contentManager)
    {
            gameManager.LoadXml(@"Content\GameData\Game.xml");
            gameManager.LoadContent(contentManager);
        }

    protected override void OnUpdate(GameTime gameTime)
    {
            if (gameManager.State == GameState.Paused)
                HandlePauseMenu();

            // Always update the game
            gameManager.Update(gameTime);
        }

    public override void Draw(SpriteBatch spriteBatch)
    {
            spriteBatch.Begin();

            gameManager.Draw(spriteBatch);

            if (gameManager.State == GameState.Paused)
                DrawPauseMenu(spriteBatch);

            spriteBatch.End();
        }

    private void HandlePauseMenu()
    {
            // Read input for moving between meny options
            // Because of the way the options are drawn the index goes in the
            // reverse direction of what the player presses
            if (settings.InputMap.IsPressed(InputAction.MenuUp))
            {
                pauseMenuIndex--;

                if (pauseMenuIndex < 0)
                    pauseMenuIndex = 0;
            }

            if (settings.InputMap.IsPressed(InputAction.MenuDown))
            {
                pauseMenuIndex++;

                if (pauseMenuIndex >= pauseMenuOptions.Length)
                    pauseMenuIndex = pauseMenuOptions.Length - 1;
            }

            // Check if MenuStart is pressed and take appropriate action
            if (settings.InputMap.IsPressed(InputAction.MenuStart))
            {
                PauseMenuOption selectedOption = pauseMenuOptions[pauseMenuIndex];

                switch (selectedOption)
                {
                    case PauseMenuOption.Resume:
                        gameManager.NextState = GameState.Playing;
                        break;
                    case PauseMenuOption.Reset:
                        gameManager.ResetLevel();
                        pauseMenuIndex = 0;
                        break;
                    case PauseMenuOption.Load:
                        /*
                        OpenFileDialog loadDialog = new OpenFileDialog();
                        loadDialog.AddExtension = true;
                        loadDialog.DefaultExt = ".xml";
                        loadDialog.Filter = ".xml | *.xml";
                        loadDialog.InitialDirectory = @"Content\GameData\Saves";

                        if (loadDialog.ShowDialog() == DialogResult.OK)
                        {
                            gameManager.LoadSession(GameSession.LoadFromXml(loadDialog.FileName));
                            pauseMenuIndex = 0;
                        }
                        */
                        break;
                    case PauseMenuOption.Save:
                        /*
                        SaveFileDialog saveDialog = new SaveFileDialog();
                        saveDialog.AddExtension = true;
                        saveDialog.DefaultExt = ".xml";
                        saveDialog.Filter = ".xml | *.xml";
                        saveDialog.InitialDirectory = @"Content\GameData\Saves";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            gameManager.SaveSession().SaveToXml(saveDialog.FileName);
                            pauseMenuIndex = 0;
                        }
                        */
                        break;
                    case PauseMenuOption.Menu:
                        ScreenManager.TransitionBack();
                        break;
                }
            }
        }

    private void DrawPauseMenu(SpriteBatch spriteBatch)
    {
            // Pause menu is drawn in the center of the screen

            // Draw texture overlay over world to fade it out a bit
            if (pauseOverlayTexture != null)
                spriteBatch.Draw(pauseOverlayTexture, pauseOverlayArea, Color.White);

            // Draw pause menu base

            // TODO: Mirror top graphics for bottom?

            // Draw pause menu text
            SpriteFont pauseFont = settings.PauseMenuFont;
            Vector2 textPos = new Vector2(PAUSE_MENU_X, PAUSE_MENU_Y);

            float pauseTextHeight = pauseFont.MeasureString("42").Y;

            int optionIndex = 0;
            foreach (PauseMenuOption menuOption in Enum.GetValues(typeof(PauseMenuOption)))
            {
                spriteBatch.DrawString(pauseFont, menuOption.ToString(), textPos, pauseMenuIndex == optionIndex ? Color.Yellow : Color.White);

                // Move next item down
                textPos.Y += pauseTextHeight + PAUSE_MENU_MARGIN;

                optionIndex++;
            }
        }
}