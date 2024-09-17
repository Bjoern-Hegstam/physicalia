using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Interfaces;

namespace XNALibrary.ScreenManagement;

public class Screen(Game game, ScreenManager manager)
{
    public Texture2D? Background { get; set; }

    public Game Game => game;

    public IInputHandler? InputHandler { get; private set; }

    public ScreenManager ScreenManager { get; } = manager;

    /// <summary>
    /// Lets the ScreenButton load any needed non-graphical services. If overriden a call to
    /// base.Initialize() must be made.
    /// </summary>
    public virtual void Initialize()
    {
        InputHandler = (IInputHandler)game.Services.GetService(typeof(IInputHandler));
    }

    public virtual void LoadContent(ContentManager contentManager)
    {
    }

    public void Update(GameTime gameTime, bool handleInput)
    {
        OnUpdate(gameTime);

        if (!handleInput)
        {
            return;
        }

        OnHandleInput();
    }

    /// <summary>
    /// Allows the Screen to draw any content it needs to display. When drawing calls to SpriteBatch.Begin()
    /// and SpriteBatch.End() must be made.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch used for drawing.</param>
    public virtual void Draw(SpriteBatch? spriteBatch)
    {
        spriteBatch.Begin();

        // Draw background if one has been set
        if (Background != null)
        {
            spriteBatch.Draw(Background, Vector2.Zero, Color.White);
        }

        OnDraw(spriteBatch);

        spriteBatch.End();
    }

    /// <summary>
    /// Override method to handle input. Methods is not called by the ScreenManager
    /// if a transition is taking place.
    /// </summary>
    protected virtual void OnHandleInput()
    {
    }

    /// <summary>
    /// Override this method to draw any special content before the buttons are drawn.
    /// </summary>
    /// <param name="spriteBatch">Initialized SpriteBatch to use for drawing content.</param>
    protected virtual void OnDraw(SpriteBatch? spriteBatch)
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