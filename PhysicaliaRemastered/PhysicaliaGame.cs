using System;
using Microsoft.Xna.Framework;
using PhysicaliaRemastered.Actors.EnemyManagement;
using PhysicaliaRemastered.GameManagement;
using PhysicaliaRemastered.Pickups;
using PhysicaliaRemastered.Screens;
using PhysicaliaRemastered.Weapons;
using XNALibrary.Animation;
using XNALibrary.Input;
using XNALibrary.ParticleEngine;
using XNALibrary.ScreenManagement;
using XNALibrary.Sprites;
using XNALibrary.TileEngine;

namespace PhysicaliaRemastered;

public class PhysicaliaGame : Game
{
    private readonly GraphicsDeviceManager _graphics;

    public PhysicaliaGame()
    {
        TargetElapsedTime = TimeSpan.FromSeconds(1) / 60;

        Content.RootDirectory = "Content";

        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 640;
        _graphics.PreferredBackBufferHeight = 480;

        // _graphics.IsFullScreen = true;
        // _graphics.HardwareModeSwitch = false;
    }

    /// <summary>
    /// Allows the game to perform any initialization it needs to before starting to run.
    /// This is where it can query for any required services and load any non-graphic
    /// related content. Calling base.Initialize will enumerate through any components
    /// and initialize them as well.
    /// </summary>
    protected override void Initialize()
    {
        Services.AddService(new InputHandler(this));
        Services.AddService(new Settings(this));
        Services.AddService(new SpriteLibrary());
        Services.AddService(new AnimationManager(this));
        Services.AddService(new TileLibrary());
        Services.AddService(new ParticleEngine());
        Services.AddService(new EnemyBank(Services));
        Services.AddService(new WeaponBank(Services));
        Services.AddService(new PickupTemplateLibrary());

        Components.Add(Services.GetService<InputHandler>());
        Components.Add(Services.GetService<AnimationManager>());

        var gameManager = new GameManager(this);

        var screenManager = new ScreenManager(this);
        Components.Add(screenManager);

        var mainMenuScreen = new MainMenuScreen(this, gameManager, screenManager);

        screenManager.BaseScreen = mainMenuScreen;
        screenManager.Screens.Add(new GameScreen(this, gameManager, screenManager));

        base.Initialize();
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