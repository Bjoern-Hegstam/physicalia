//#define MockCode

using System;
using Microsoft.Xna.Framework;
using PhysicaliaRemastered.Screens;
using XNALibrary.Graphics.ScreenManagement;
using XNALibrary.Input;

namespace PhysicaliaRemastered;

/// <summary>
/// This is the main type for your game
/// </summary>
public class PhysicaliaGame : Game
{
    private readonly GraphicsDeviceManager _graphics;

    private InputHandler _inputHandler;

    private ScreenManager _screenManager;

    private MenuScreen _menuScreen;
    private GameScreen _gameScreen;

    public PhysicaliaGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        TargetElapsedTime = TimeSpan.FromSeconds(0.0083333); // 120fps
        _graphics.PreferredBackBufferWidth = 640;
        _graphics.PreferredBackBufferHeight = 480;

        _graphics.IsFullScreen = false;
    }

    /// <summary>
    /// Allows the game to perform any initialization it needs to before starting to run.
    /// This is where it can query for any required services and load any non-graphic
    /// related content.  Calling base.Initialize will enumerate through any components
    /// and initialize them as well.
    /// </summary>
    protected override void Initialize()
    {
        _inputHandler = new InputHandler(this);
        Components.Add(_inputHandler);

        _screenManager = new ScreenManager(this);
        Components.Add(_screenManager);

        _menuScreen = new MenuScreen(this, _screenManager);
        _gameScreen = new GameScreen(this, _screenManager);

        _screenManager.BaseScreen = _menuScreen;

        _screenManager.Screens.Add(_menuScreen);
        _screenManager.Screens.Add(_gameScreen);

        base.Initialize();
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
        // The ScreenManager gets to load content before we get here
        //this.titleScreen.Settings = this.gameScreen.Settings;
        _menuScreen.Settings = _gameScreen.Settings;

        _menuScreen.GameManager = _gameScreen.GameManager;

        // TODO: use this.Content to load your game content here
    }

    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
        _graphics.GraphicsDevice.Clear(Color.Black);

        base.Draw(gameTime);
    }
}