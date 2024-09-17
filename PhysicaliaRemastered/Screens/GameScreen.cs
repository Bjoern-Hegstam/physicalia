using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.GameManagement;
using PhysicaliaRemastered.Input;
using XNALibrary.ScreenManagement;

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

    private const float PauseMenuY = 165F;
    private const float PauseMenuX = 250F;
    private const float PauseMenuMargin = 5F;

    private readonly ISettings _settings;

    // Pause menu fields
    private Texture2D _pauseOverlayTexture;
    private Rectangle _pauseOverlayArea;

    private int _pauseMenuIndex;

    private readonly PauseMenuOption[] _pauseMenuOptions = (PauseMenuOption[])Enum.GetValues(typeof(PauseMenuOption));

    public ISettings Settings => GameManager.Settings;

    public GameManager GameManager { get; }

    public GameScreen(Game game, ScreenManager manager)
        : base(game, manager)
    {
        GameManager = new GameManager(Game);
        _settings = GameManager.Settings;
    }

    public override void Initialize()
    {
        base.Initialize();

        // Create Pause overlay texture
        _pauseOverlayTexture = new Texture2D(Game.GraphicsDevice, 1, 1, true, SurfaceFormat.Color);
        _pauseOverlayTexture.SetData([new Color(0, 0, 0, 128)]);

        _pauseOverlayArea = new Rectangle
        {
            Width = Game.GraphicsDevice.Viewport.Width,
            Height = Game.GraphicsDevice.Viewport.Height
        };
    }

    public override void LoadContent(ContentManager contentManager)
    {
        GameManager.LoadXml("Content/GameData/Game.xml");
        GameManager.LoadContent(contentManager);
    }

    protected override void OnUpdate(GameTime gameTime)
    {
        if (GameManager.State == GameState.Paused)
        {
            HandlePauseMenu();
        }

        // Always update the game
        GameManager.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin();

        GameManager.Draw(spriteBatch);

        if (GameManager.State == GameState.Paused)
        {
            DrawPauseMenu(spriteBatch);
        }

        spriteBatch.End();
    }

    private void HandlePauseMenu()
    {
        // Read input for moving between meny options
        // Because of the way the options are drawn the index goes in the
        // reverse direction of what the player presses
        if (_settings.InputMap.IsPressed(InputAction.MenuUp))
        {
            _pauseMenuIndex--;

            if (_pauseMenuIndex < 0)
            {
                _pauseMenuIndex = 0;
            }
        }

        if (_settings.InputMap.IsPressed(InputAction.MenuDown))
        {
            _pauseMenuIndex++;

            if (_pauseMenuIndex >= _pauseMenuOptions.Length)
            {
                _pauseMenuIndex = _pauseMenuOptions.Length - 1;
            }
        }

        // Check if MenuStart is pressed and take appropriate action
        if (_settings.InputMap.IsPressed(InputAction.MenuStart))
        {
            PauseMenuOption selectedOption = _pauseMenuOptions[_pauseMenuIndex];

            switch (selectedOption)
            {
                case PauseMenuOption.Resume:
                    GameManager.NextState = GameState.Playing;
                    break;
                case PauseMenuOption.Reset:
                    GameManager.ResetLevel();
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
        if (_pauseOverlayTexture != null)
        {
            spriteBatch.Draw(_pauseOverlayTexture, _pauseOverlayArea, Color.White);
        }

        // Draw pause menu base

        // TODO: Mirror top graphics for bottom?

        // Draw pause menu text
        SpriteFont pauseFont = _settings.PauseMenuFont;
        Vector2 textPos = new Vector2(PauseMenuX, PauseMenuY);

        float pauseTextHeight = pauseFont.MeasureString("42").Y;

        int optionIndex = 0;
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