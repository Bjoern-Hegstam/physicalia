//#define MockCode

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Screens;

namespace PhysicaliaRemastered;

/// <summary>
/// This is the main type for your game
/// </summary>
public class PhysicaliaGame : Game
{
    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;

    InputHandler inputHandler;

    ScreenManager screenManager;

    private TitleScreen titleScreen;
    private MenuScreen menuScreen;
    private GameScreen gameScreen;

    public PhysicaliaGame()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        TargetElapsedTime = TimeSpan.FromSeconds(0.0083333); // 120fps
        graphics.PreferredBackBufferWidth = 640;
        graphics.PreferredBackBufferHeight = 480;

        graphics.IsFullScreen = true;
    }

    /// <summary>
    /// Allows the game to perform any initialization it needs to before starting to run.
    /// This is where it can query for any required services and load any non-graphic
    /// related content.  Calling base.Initialize will enumerate through any components
    /// and initialize them as well.
    /// </summary>
    protected override void Initialize()
    {
        // Create a new InputHandler and add it to the collection of Components
        inputHandler = new InputHandler(this);
        Components.Add(inputHandler);

        // Create a new ScreenManager and add it to the collection of Components
        screenManager = new ScreenManager(this);
        Components.Add(screenManager);

        //this.titleScreen = new TitleScreen(this, this.screenManager);
        menuScreen = new MenuScreen(this, screenManager);
        gameScreen = new GameScreen(this, screenManager);
            
        screenManager.BaseScreen = menuScreen;

        //this.screenManager.Screens.Add(this.titleScreen);
        screenManager.Screens.Add(menuScreen);
        screenManager.Screens.Add(gameScreen);

        base.Initialize();
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
        // Create a new SpriteBatch, which can be used to draw textures.
        spriteBatch = new SpriteBatch(GraphicsDevice);

        // The ScreenManager gets to load content before we get here
        //this.titleScreen.Settings = this.gameScreen.Settings;
        menuScreen.Settings = gameScreen.Settings;

        menuScreen.GameManager = gameScreen.GameManager;

        // TODO: use this.Content to load your game content here
    }

    /// <summary>
    /// UnloadContent will be called once per game and is the place to unload
    /// all content.
    /// </summary>
    protected override void UnloadContent()
    {
        // TODO: Unload any non ContentManager content here
    }

    /// <summary>
    /// Allows the game to run logic such as updating the world,
    /// checking for collisions, gathering input, and playing audio.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Update(GameTime gameTime)
    {
        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
        graphics.GraphicsDevice.Clear(Color.Black);

        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}