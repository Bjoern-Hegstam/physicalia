using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.GameManagement;
using PhysicaliaRemastered.Input;
using XNALibrary.ScreenManagement;

namespace PhysicaliaRemastered.Screens;

public class GameScreen(Game game, GameManager gameManager, ScreenManager screenManager) : Screen
{
    public enum PauseMenuOption
    {
        Resume,
        Reset,
        Load,
        Save,
        Menu
    }

    private const float PauseMenuY = 165F;
    private const float PauseMenuX = 250F;
    private const float PauseMenuMargin = 5F;

    private Texture2D? _pauseOverlayTexture;
    private Rectangle _pauseOverlayArea;

    private int _pauseMenuIndex;

    private readonly PauseMenuOption[] _pauseMenuOptions = (PauseMenuOption[])Enum.GetValues(typeof(PauseMenuOption));

    private Fonts Fonts => game.Services.GetService<Fonts>();
    private Settings Settings => game.Services.GetService<Settings>();

    public override void Initialize()
    {
        base.Initialize();

        _pauseOverlayTexture = new Texture2D(game.GraphicsDevice, 1, 1, true, SurfaceFormat.Color);
        _pauseOverlayTexture.SetData([new Color(0, 0, 0, 128)]);

        _pauseOverlayArea = new Rectangle
        {
            Width = game.GraphicsDevice.Viewport.Width,
            Height = game.GraphicsDevice.Viewport.Height
        };
    }

    public override void LoadContent(ContentManager contentManager)
    {
        gameManager.LoadXml("Content/GameData/Game.xml", contentManager);
    }

    protected override void OnUpdate(GameTime gameTime)
    {
        if (gameManager.State == GameState.Paused)
        {
            HandlePauseMenu();
        }

        gameManager.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin();

        gameManager.Draw(spriteBatch);

        if (gameManager.State == GameState.Paused)
        {
            DrawPauseMenu(spriteBatch);
        }

        spriteBatch.End();
    }

    private void HandlePauseMenu()
    {
        // Read input for moving between menu options
        // Because of the way the options are drawn the index goes in the
        // reverse direction of what the player presses
        if (Settings.InputMap.IsPressed(InputAction.MenuUp))
        {
            _pauseMenuIndex--;

            if (_pauseMenuIndex < 0)
            {
                _pauseMenuIndex = 0;
            }
        }

        if (Settings.InputMap.IsPressed(InputAction.MenuDown))
        {
            _pauseMenuIndex++;

            if (_pauseMenuIndex >= _pauseMenuOptions.Length)
            {
                _pauseMenuIndex = _pauseMenuOptions.Length - 1;
            }
        }

        // Check if MenuStart is pressed and take appropriate action
        if (Settings.InputMap.IsPressed(InputAction.MenuStart))
        {
            PauseMenuOption selectedOption = _pauseMenuOptions[_pauseMenuIndex];

            switch (selectedOption)
            {
                case PauseMenuOption.Resume:
                    gameManager.NextState = GameState.Playing;
                    break;
                case PauseMenuOption.Reset:
                    gameManager.ResetLevel();
                    _pauseMenuIndex = 0;
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
                        gameManager.LoadGame(SaveGame.LoadFromXml(loadDialog.FileName));
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
                        gameManager.SaveGame().SaveToXml(saveDialog.FileName);
                        pauseMenuIndex = 0;
                    }
                    */
                    break;
                case PauseMenuOption.Menu:
                    screenManager.TransitionBack();
                    break;
            }
        }
    }

    private void DrawPauseMenu(SpriteBatch spriteBatch)
    {
        // Pause menu is drawn in the center of the screen

        // Draw texture overlay over world to fade it out a bit
        if (_pauseOverlayTexture != null)
        {
            spriteBatch.Draw(_pauseOverlayTexture, _pauseOverlayArea, Color.White);
        }

        // Draw pause menu base

        // TODO: Mirror top graphics for bottom?

        // Draw pause menu text
        SpriteFont pauseFont = Fonts.PauseMenu;
        var textPos = new Vector2(PauseMenuX, PauseMenuY);

        float pauseTextHeight = pauseFont.MeasureString("42").Y;

        var optionIndex = 0;
        foreach (PauseMenuOption menuOption in Enum.GetValues(typeof(PauseMenuOption)))
        {
            spriteBatch.DrawString(pauseFont, menuOption.ToString(), textPos,
                _pauseMenuIndex == optionIndex ? Color.Yellow : Color.White);

            // Move next item down
            textPos.Y += pauseTextHeight + PauseMenuMargin;

            optionIndex++;
        }
    }
}