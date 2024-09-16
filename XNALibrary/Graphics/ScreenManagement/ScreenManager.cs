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
        get { return screens; }
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

            if (screenStack.Count > 0)
                screenStack.Clear();

            screenStack.Push(value);
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
        get { return transitionState != ScreenTransitionState.None; }
    }

    /// <summary>
    /// Gets and Sets the transition speed.
    /// </summary>
    public float TransitionSpeed
    {
        get { return transitionSpeed; }
        set
        {
            if (value <= 0)
                throw new ArgumentException("Transition speed must be greater than zero!");
            else
                transitionSpeed = value;
        }
    }


    private IScreenTransitionEffect transitionEffect;

    public IScreenTransitionEffect TransitionEffect
    {
        get { return transitionEffect; }
        set { transitionEffect = value; }
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
        screenStack = new Stack<Screen>();
        screens = new List<Screen>();

        transitionSpeed = DEFAULT_TRANSITION_SPEED;
        transitionAmount = 0F;
        transitionState = ScreenTransitionState.None;
    }

    public ScreenManager(Game game, IScreenTransitionEffect effect)
        : base(game)
    {
        transitionEffect = effect;
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

        if (Transitioning)
            return false;

        // Go through every available screen
        foreach (Screen screen in screens)
        {
            // See if a screen of the wanted type is available
            if (screen.GetType() == screenType)
            {
                // Found the screen, start the transition
                transitionScreen = screen;
                transitionState = ScreenTransitionState.Forward;
                StartTransition();
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
        if (screenStack.Count > 1 && !Transitioning)
        {
            transitionScreen = screenStack.Pop();
            transitionState = ScreenTransitionState.Backward;
            StartTransition();
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
        transitionAmount = 0F;
        if (transitionSpeed < 0)
            transitionSpeed = Math.Abs(transitionSpeed);

        // Notify the current screen that it's Transitioning away
        // and the next screen that it's Transitioning in
        if (transitionState == ScreenTransitionState.Forward)
        {
            screenStack.Peek().OnTransitionOut(false);
            transitionScreen.OnTransitionIn(false);
        }
        else
        {
            transitionScreen.OnTransitionOut(false);
            screenStack.Peek().OnTransitionIn(false);
        }
    }

    /// <summary>
    /// Cleans up after a transition has been made.
    /// </summary>
    private void EndTransition()
    {
        // Notify the screens that the transition is over
        if (transitionState == ScreenTransitionState.Forward)
        {
            screenStack.Peek().OnTransitionOut(true);
            transitionScreen.OnTransitionIn(true);
        }
        else
        {
            transitionScreen.OnTransitionOut(true);
            screenStack.Peek().OnTransitionIn(true);
        }

        // Push the new screen on the stack if we transitioned forward
        if (transitionState == ScreenTransitionState.Forward)
            screenStack.Push(transitionScreen);

        // Do needed cleanup
        transitionScreen = null;
        transitionState = ScreenTransitionState.None;
        transitionAmount = 0F;
        transitionSpeed *= -1;
    }


    public override void Initialize()
    {
        // Initialize all screens
        foreach (Screen screen in screens)
        {
            screen.Initialize();
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // Initialize the SpriteBatch and RenderTarget2D
        spriteBatch = new SpriteBatch(Game.GraphicsDevice);

        renderTarget = new RenderTarget2D(
            Game.GraphicsDevice,
            Game.GraphicsDevice.PresentationParameters.BackBufferWidth,
            Game.GraphicsDevice.PresentationParameters.BackBufferHeight
        );

        // Let screens load content
        foreach (Screen screen in screens)
        {
            screen.LoadContent(Game.Content);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Don't update if no Screens are in the stack
        if (screenStack.Count == 0)
            return;

        // Are we transitioning between screens
        if (Transitioning)
        {
            // Let both screens update but not handle input
            screenStack.Peek().Update(gameTime, false);
            transitionScreen.Update(gameTime, false);

            // Increase the transition amount
            transitionAmount += (float)gameTime.ElapsedGameTime.TotalSeconds * transitionSpeed;

            // Has the transition come to its end?
            if (transitionAmount >= 1.0F)
                EndTransition();
        }
        // if not just update the current Screen
        else
        {
            screenStack.Peek().Update(gameTime, true);
        }
    }

    public override void Draw(GameTime gameTime)
    {
        // Don't draw if no Screens are on the stack
        if (screenStack.Count == 0)
            return;

        // Are we transitioning between Screens?
        if (Transitioning && transitionEffect != null)
        {
            // Change the render target
            GraphicsDevice.SetRenderTarget(renderTarget);

            // Clear the graphics device
            GraphicsDevice.Clear(Color.Black);

            // Draw the next screen to the render target
            if (transitionState == ScreenTransitionState.Forward)
                transitionScreen.Draw(spriteBatch);
            else
                screenStack.Peek().Draw(spriteBatch);

            // Change the render target back
            GraphicsDevice.SetRenderTarget(null);

            // Draw current Screen
            if (transitionState == ScreenTransitionState.Forward)
                screenStack.Peek().Draw(spriteBatch);
            else
                transitionScreen.Draw(spriteBatch);

            // Begin draw call
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            // Set the transition mask
            GraphicsDevice.Textures[1] = transitionEffect.TransitionMask;

            // Set parameters on the used effect
            transitionEffect.TransitionProgress = transitionAmount;

            // Start effect
            transitionEffect.Begin();

            // Draw texture of current Screen
            spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);

            // End draw and the effect
            spriteBatch.End();
            transitionEffect.End();
        }
        else if (Transitioning)
        {
            Vector3 overlay = Vector3.Zero;

            // Calculate an overlay color for the transition
            if (transitionAmount < 0.5F)
                overlay = Vector3.Lerp(Color.White.ToVector3(), Color.Black.ToVector3(), transitionAmount * 2);
            else
                overlay = Vector3.Lerp(Color.Black.ToVector3(), Color.White.ToVector3(), (transitionAmount - 0.5F) * 2);

            // Set rendertarget and clear device
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Black);

            // Draw current screen
            if (transitionState == ScreenTransitionState.Forward)
            {
                if (transitionAmount < 0.5F)
                    screenStack.Peek().Draw(spriteBatch);
                else
                    transitionScreen.Draw(spriteBatch);
            }
            else
            {
                if (transitionAmount < 0.5F)
                    transitionScreen.Draw(spriteBatch);
                else
                    screenStack.Peek().Draw(spriteBatch);
            }

            // Reset the device
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            // Draw screen texture using the overlay color
            spriteBatch.Draw(renderTarget, Vector2.Zero, new Color(overlay));

            spriteBatch.End();
        }
        else
            // Draw current Screen
            screenStack.Peek().Draw(spriteBatch);
    }
}