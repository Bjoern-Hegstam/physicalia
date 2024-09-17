using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Interfaces;

namespace XNALibrary.Graphics.ScreenManagement;

public class Screen
{
    private readonly List<ScreenButton> _buttons;

    /// <summary>
    /// Gets the Screen's collection of buttons.
    /// </summary>
    public List<ScreenButton> Buttons => _buttons;

    private Texture2D _background;

    /// <summary>
    /// Gets and Sets the backgroundimage. Set to null for no background.
    /// </summary>
    public Texture2D Background
    {
        get => _background;
        set => _background = value;
    }

    private readonly Game _game;

    /// <summary>
    /// Gets the game using the Screen.
    /// </summary>
    public Game Game => _game;

    private IInputHandler _inputHandler;

    /// <summary>
    /// Gets the IInputHandler.
    /// </summary>
    public IInputHandler InputHandler => _inputHandler;

    private readonly ScreenManager _manager;

    /// <summary>
    /// Gets the ScreenManager that's managing the Screen.
    /// </summary>
    public ScreenManager ScreenManager => _manager;

    public Screen(Game game, ScreenManager manager)
    {
        _buttons = new List<ScreenButton>();

        _game = game;
        _manager = manager;
    }

    /// <summary>
    /// Lets the ScreenButton load any needed non-graphical services. If overriden a call to
    /// base.Initialize() must be made.
    /// </summary>
    public virtual void Initialize()
    {
        _inputHandler = (IInputHandler)_game.Services.GetService(typeof(IInputHandler));
    }

    public virtual void LoadContent(ContentManager contentManager)
    {
    }

    public virtual void UnloadContent()
    {
    }

    /// <summary>
    /// Updates the Screen.
    /// </summary>
    /// <param name="gameTime"></param>
    /// <param name="handleInput">True if the Screen should handle input.</param>
    public void Update(GameTime gameTime, bool handleInput)
    {
        // Let child update
        OnUpdate(gameTime);

        // Update the buttons
        UpdateButtons(gameTime);

        // Should input be handled?
        if (handleInput)
        {
            HandleInput();

            // Let child handle input
            OnHandleInput();
        }
    }

    /// <summary>
    /// Allows the Screen to draw any content it needs to display. When drawing calls to SpriteBatch.Begin()
    /// and SpriteBatch.End() must be made.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch used for drawing.</param>
    public virtual void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin();

        // Draw background if one has been set
        if (_background != null)
            spriteBatch.Draw(_background, Vector2.Zero, Color.White);

        // Draw content before buttons
        OnDrawBefore(spriteBatch);

        // Draw all buttons
        foreach (ScreenButton button in _buttons)
            button.Draw(spriteBatch);

        // Draw content after buttons
        OnDrawAfter(spriteBatch);

        spriteBatch.End();
    }

    private void UpdateButtons(GameTime gameTime)
    {
        foreach (ScreenButton button in _buttons)
        {
            button.Update(gameTime);
        }
    }

    /// <summary>
    /// Lets the buttons handle input.
    /// </summary>
    private void HandleInput()
    {
        foreach (ScreenButton button in _buttons)
        {
            button.HandleInput();

            // Has the button been pressed?
            if (button.IsPressed)
            {
                if (button.ScreenLink != null)
                    ScreenManager.TransitionTo(button.ScreenLink);
                else
                    ScreenManager.TransitionBack();

                // Don't check any more buttons
                return;
            }
        }
    }

    /// <summary>
    /// Overruide method to handle input. Methods is not called by the ScreenManager
    /// if a transition is taking place.
    /// </summary>
    protected virtual void OnHandleInput()
    {
    }

    /// <summary>
    /// Override this method to draw any special content before the buttons are drawn.
    /// </summary>
    /// <param name="spriteBatch">Initialized SpriteBatch to use for drawing content.</param>
    protected virtual void OnDrawBefore(SpriteBatch spriteBatch)
    {
    }

    /// <summary>
    /// Override this method to draw any special content before the buttons are drawn.
    /// </summary>
    /// <param name="spriteBatch">Initialized SpriteBatch to use for drawing content.</param>
    protected virtual void OnDrawAfter(SpriteBatch spriteBatch)
    {
    }

    /// <summary>
    /// Called when the Screen is updated. Override this method to perform any update code.
    /// </summary>
    /// <param name="gameTime"></param>
    protected virtual void OnUpdate(GameTime gameTime)
    {
    }

    /// <summary>
    /// Called when the Screen is transitioning in.
    /// </summary>
    /// <param name="finished">True if the transition has finished; false otherwise</param>
    public virtual void OnTransitionIn(bool finished)
    {
    }

    /// <summary>
    /// Called when the Screen is transitioning out.
    /// </summary>
    /// <param name="finished">True if the Screen has finished its part of the transition; false otherwise</param>
    public virtual void OnTransitionOut(bool finished)
    {
    }
}