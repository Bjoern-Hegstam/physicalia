using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Graphics.ScreenManagement;

/// <summary>
/// Manages over a collection of Screens and handles the Transitioning
/// between these.
/// </summary>
public class ScreenManager : DrawableGameComponent
{
    private Stack<Screen> screenStack;
    private List<Screen> screens;

    public List<Screen> Screens
    {
        get { return this.screens; }
    }

    /// <summary>
    /// Sets the base Screen of the screen hierarchy.
    /// </summary>
    public Screen BaseScreen
    {
        set
        {
            if (value == null)
                throw new ArgumentNullException("value", "BaseScreen cannot be null!");

            if (this.screenStack.Count > 0)
                this.screenStack.Clear();

            this.screenStack.Push(value);
        }
    }


    private enum ScreenTransitionState
    {
        Forward,
        Backward,
        None
    }

    private ScreenTransitionState transitionState;
    private Screen transitionScreen;

    private const float DEFAULT_TRANSITION_SPEED = 0.5F;

    private float transitionSpeed;
    private float transitionAmount;

    /// <summary>
    /// Gets a boolean value indicating whether the manager is currently
    /// transitioning between two Screens.
    /// </summary>
    public bool Transitioning
    {
        get { return this.transitionState != ScreenTransitionState.None; }
    }

    /// <summary>
    /// Gets and Sets the transition speed.
    /// </summary>
    public float TransitionSpeed
    {
        get { return this.transitionSpeed; }
        set
        {
            if (value <= 0)
                throw new ArgumentException("Transition speed must be greater than zero!");
            else
                this.transitionSpeed = value;
        }
    }


    private IScreenTransitionEffect transitionEffect;

    public IScreenTransitionEffect TransitionEffect
    {
        get { return this.transitionEffect; }
        set { this.transitionEffect = value; }
    }

    private SpriteBatch spriteBatch;
    private RenderTarget2D renderTarget;


    /// <summary>
    /// Creates a new ScreenManager.
    /// </summary>
    /// <param name="game">The game using the manager.</param>
    public ScreenManager(Game game)
        : base(game)
    {
        this.screenStack = new Stack<Screen>();
        this.screens = new List<Screen>();

        this.transitionSpeed = DEFAULT_TRANSITION_SPEED;
        this.transitionAmount = 0F;
        this.transitionState = ScreenTransitionState.None;
    }

    public ScreenManager(Game game, IScreenTransitionEffect effect)
        : base(game)
    {
        this.transitionEffect = effect;
    }


    /// <summary>
    /// Makes the ScreenManager start the transition to a new Screen.
    /// </summary>
    /// <param name="screenType">The type of Screen to transition to.</param>
    /// <returns>True if the transition was succesfully initialized; false otherwise.</returns>
    public bool TransitionTo(Type screenType)
    {
        if (screenType == null)
            throw new ArgumentNullException("screenType cannot be null!");

        if (this.Transitioning)
            return false;

        // Go through every available screen
        foreach (Screen screen in this.screens)
        {
            // See if a screen of the wanted type is available
            if (screen.GetType() == screenType)
            {
                // Found the screen, start the transition
                this.transitionScreen = screen;
                this.transitionState = ScreenTransitionState.Forward;
                this.StartTransition();
                return true;
            }
        }

        // Screen of type not found , return false
        return false;
    }

    /// <summary>
    /// Tells the ScreenManager to transition back to the previous screen.
    /// </summary>
    /// <returns>True if the transition was succesfully initialized; false otherwise.</returns>
    public bool TransitionBack()
    {
        if (this.screenStack.Count > 1 && !this.Transitioning)
        {
            this.transitionScreen = this.screenStack.Pop();
            this.transitionState = ScreenTransitionState.Backward;
            this.StartTransition();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Setst the variables needed for starting the transition to a new Screen.
    /// </summary>
    private void StartTransition()
    {
        // Set needed variables to start the transítion
        this.transitionAmount = 0F;
        if (this.transitionSpeed < 0)
            this.transitionSpeed = Math.Abs(this.transitionSpeed);

        // Notify the current screen that it's Transitioning away
        // and the next screen that it's Transitioning in
        if (this.transitionState == ScreenTransitionState.Forward)
        {
            this.screenStack.Peek().OnTransitionOut(false);
            this.transitionScreen.OnTransitionIn(false);
        }
        else
        {
            this.transitionScreen.OnTransitionOut(false);
            this.screenStack.Peek().OnTransitionIn(false);
        }
    }

    /// <summary>
    /// Cleans up after a transition has been made.
    /// </summary>
    private void EndTransition()
    {
        // Notify the screens that the transition is over
        if (this.transitionState == ScreenTransitionState.Forward)
        {
            this.screenStack.Peek().OnTransitionOut(true);
            this.transitionScreen.OnTransitionIn(true);
        }
        else
        {
            this.transitionScreen.OnTransitionOut(true);
            this.screenStack.Peek().OnTransitionIn(true);
        }

        // Push the new screen on the stack if we transitioned forward
        if (this.transitionState == ScreenTransitionState.Forward)
            this.screenStack.Push(this.transitionScreen);

        // Do needed cleanup
        this.transitionScreen = null;
        this.transitionState = ScreenTransitionState.None;
        this.transitionAmount = 0F;
        this.transitionSpeed *= -1;
    }


    public override void Initialize()
    {
        // Initialize all screens
        foreach (Screen screen in this.screens)
        {
            screen.Initialize();
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // Initialize the SpriteBatch and RenderTarget2D
        this.spriteBatch = new SpriteBatch(this.Game.GraphicsDevice);

        this.renderTarget = new RenderTarget2D(this.Game.GraphicsDevice,
            this.Game.GraphicsDevice.PresentationParameters.BackBufferWidth,
            this.Game.GraphicsDevice.PresentationParameters.BackBufferHeight,
            1,
            SurfaceFormat.Color);

        // Let screens load content
        foreach (Screen screen in this.screens)
        {
            screen.LoadContent(this.Game.Content);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Don't update if no Screens are in the stack
        if (this.screenStack.Count == 0)
            return;

        // Are we transitioning between screens
        if (this.Transitioning)
        {
            // Let both screens update but not handle input
            this.screenStack.Peek().Update(gameTime, false);
            this.transitionScreen.Update(gameTime, false);

            // Increase the transition amount
            this.transitionAmount += (float)gameTime.ElapsedGameTime.TotalSeconds * this.transitionSpeed;

            // Has the transition come to its end?
            if (this.transitionAmount >= 1.0F)
                this.EndTransition();
        }
        // if not just update the current Screen
        else
        {
            this.screenStack.Peek().Update(gameTime, true);
        }
    }

    public override void Draw(GameTime gameTime)
    {
        // Don't draw if no Screens are on the stack
        if (this.screenStack.Count == 0)
            return;

        // Are we transitioning between Screens?
        if (this.Transitioning && this.transitionEffect != null)
        {
            // Change the render target
            this.GraphicsDevice.SetRenderTarget(0, this.renderTarget);

            // Clear the graphics device
            this.GraphicsDevice.Clear(Color.Black);

            // Draw the next screen to the render target
            if (this.transitionState == ScreenTransitionState.Forward)
                this.transitionScreen.Draw(spriteBatch);
            else
                this.screenStack.Peek().Draw(spriteBatch);

            // Change the render target back
            this.GraphicsDevice.SetRenderTarget(0, null);

            // Draw current Screen
            if (this.transitionState == ScreenTransitionState.Forward)
                this.screenStack.Peek().Draw(spriteBatch);
            else
                this.transitionScreen.Draw(spriteBatch);

            // Begin draw call
            this.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);

            // Set the transition mask
            this.GraphicsDevice.Textures[1] = this.transitionEffect.TransitionMask;

            // Set parameters on the used effect
            this.transitionEffect.TransitionProgress = this.transitionAmount;

            // Start effect
            this.transitionEffect.Begin();

            // Draw texture of current Screen
            this.spriteBatch.Draw(this.renderTarget.GetTexture(), Vector2.Zero, Color.White);

            // End draw and the effect
            this.spriteBatch.End();
            this.transitionEffect.End();
        }
        else if (this.Transitioning)
        {
            Vector3 overlay = Vector3.Zero;

            // Calculate an overlay color for the transition
            if (this.transitionAmount < 0.5F)
                overlay = Vector3.Lerp(Color.White.ToVector3(), Color.Black.ToVector3(), this.transitionAmount * 2);
            else
                overlay = Vector3.Lerp(Color.Black.ToVector3(), Color.White.ToVector3(), (this.transitionAmount - 0.5F) * 2);

            // Set rendertarget and clear device
            this.GraphicsDevice.SetRenderTarget(0, this.renderTarget);
            this.GraphicsDevice.Clear(Color.Black);

            // Draw current screen
            if (this.transitionState == ScreenTransitionState.Forward)
            {
                if (this.transitionAmount < 0.5F)
                    this.screenStack.Peek().Draw(spriteBatch);
                else
                    this.transitionScreen.Draw(spriteBatch);
            }
            else
            {
                if (this.transitionAmount < 0.5F)
                    this.transitionScreen.Draw(spriteBatch);
                else
                    this.screenStack.Peek().Draw(spriteBatch);
            }

            // Reset the device
            this.GraphicsDevice.SetRenderTarget(0, null);
            this.GraphicsDevice.Clear(Color.Black);

            this.spriteBatch.Begin();

            // Draw screen texture using the overlay color
            this.spriteBatch.Draw(this.renderTarget.GetTexture(), Vector2.Zero, new Color(overlay));

            this.spriteBatch.End();
        }
        else
            // Draw current Screen
            this.screenStack.Peek().Draw(spriteBatch);
    }
}