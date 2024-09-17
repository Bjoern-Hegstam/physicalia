using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.ScreenManagement;

/// <summary>
/// Manages over a collection of Screens and handles the Transitioning
/// between these.
/// </summary>
public class ScreenManager(Game game) : DrawableGameComponent(game)
{
    private readonly Stack<Screen> _screenStack = new();

    public List<Screen> Screens { get; } = [];

    /// <summary>
    /// Sets the base Screen of the screen hierarchy.
    /// </summary>
    public Screen? BaseScreen
    {
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "BaseScreen cannot be null!");
            }

            if (_screenStack.Count > 0)
            {
                _screenStack.Clear();
            }

            _screenStack.Push(value);
        }
    }
    
    private enum ScreenTransitionState
    {
        Forward,
        Backward,
        None
    }

    private ScreenTransitionState _transitionState = ScreenTransitionState.None;
    private Screen? _transitionScreen;

    private const float DefaultTransitionSpeed = 0.5F;

    private float _transitionSpeed = DefaultTransitionSpeed;
    private float _transitionAmount;

    /// <summary>
    /// Gets a boolean value indicating whether the manager is currently
    /// transitioning between two Screens.
    /// </summary>
    public bool Transitioning => _transitionState != ScreenTransitionState.None;


    public IScreenTransitionEffect? TransitionEffect { get; set; }

    private SpriteBatch? _spriteBatch;
    private RenderTarget2D? _renderTarget;

    /// <summary>
    /// Makes the ScreenManager start the transition to a new Screen.
    /// </summary>
    /// <param name="screenType">The type of Screen to transition to.</param>
    /// <returns>True if the transition was successfully initialized; false otherwise.</returns>
    public bool TransitionTo(Type screenType)
    {
        if (screenType == null)
        {
            throw new ArgumentNullException(nameof(screenType));
        }

        if (Transitioning)
        {
            return false;
        }

        // Go through every available screen
        foreach (Screen? screen in Screens)
        {
            // See if a screen of the wanted type is available
            if (screen.GetType() == screenType)
            {
                // Found the screen, start the transition
                _transitionScreen = screen;
                _transitionState = ScreenTransitionState.Forward;
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
    /// <returns>True if the transition was successfully initialized; false otherwise.</returns>
    public bool TransitionBack()
    {
        if (_screenStack.Count > 1 && !Transitioning)
        {
            _transitionScreen = _screenStack.Pop();
            _transitionState = ScreenTransitionState.Backward;
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
        _transitionAmount = 0F;
        if (_transitionSpeed < 0)
        {
            _transitionSpeed = Math.Abs(_transitionSpeed);
        }

        // Notify the current screen that it's Transitioning away
        // and the next screen that it's Transitioning in
        if (_transitionState == ScreenTransitionState.Forward)
        {
            _screenStack.Peek().OnTransitionOut(false);
            _transitionScreen?.OnTransitionIn(false);
        }
        else
        {
            _transitionScreen?.OnTransitionOut(false);
            _screenStack.Peek().OnTransitionIn(false);
        }
    }

    /// <summary>
    /// Cleans up after a transition has been made.
    /// </summary>
    private void EndTransition()
    {
        // Notify the screens that the transition is over
        if (_transitionState == ScreenTransitionState.Forward)
        {
            _screenStack.Peek().OnTransitionOut(true);
            _transitionScreen?.OnTransitionIn(true);
        }
        else
        {
            _transitionScreen?.OnTransitionOut(true);
            _screenStack.Peek().OnTransitionIn(true);
        }

        // Push the new screen on the stack if we transitioned forward
        if (_transitionState == ScreenTransitionState.Forward && _transitionScreen != null)
        {
            _screenStack.Push(_transitionScreen);
        }

        // Do needed cleanup
        _transitionScreen = null;
        _transitionState = ScreenTransitionState.None;
        _transitionAmount = 0F;
        _transitionSpeed *= -1;
    }

    public override void Initialize()
    {
        // Initialize all screens
        foreach (Screen screen in Screens)
        {
            screen.Initialize();
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // Initialize the SpriteBatch and RenderTarget2D
        _spriteBatch = new SpriteBatch(Game.GraphicsDevice);

        _renderTarget = new RenderTarget2D(
            Game.GraphicsDevice,
            Game.GraphicsDevice.PresentationParameters.BackBufferWidth,
            Game.GraphicsDevice.PresentationParameters.BackBufferHeight
        );

        // Let screens load content
        foreach (Screen screen in Screens)
        {
            screen.LoadContent(Game.Content);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Don't update if no Screens are in the stack
        if (_screenStack.Count == 0)
        {
            return;
        }

        // Are we transitioning between screens
        if (Transitioning)
        {
            // Let both screens update but not handle input
            _screenStack.Peek().Update(gameTime, false);
            _transitionScreen?.Update(gameTime, false);

            // Increase the transition amount
            _transitionAmount += (float)gameTime.ElapsedGameTime.TotalSeconds * _transitionSpeed;

            // Has the transition come to its end?
            if (_transitionAmount >= 1.0F)
            {
                EndTransition();
            }
        }
        // if not just update the current Screen
        else
        {
            _screenStack.Peek().Update(gameTime, true);
        }
    }

    public override void Draw(GameTime gameTime)
    {
        // Don't draw if no Screens are on the stack
        if (_screenStack.Count == 0)
        {
            return;
        }

        // Are we transitioning between Screens?
        if (Transitioning && TransitionEffect != null)
        {
            // Change the render target
            GraphicsDevice.SetRenderTarget(_renderTarget);

            // Clear the graphics device
            GraphicsDevice.Clear(Color.Black);

            // Draw the next screen to the render target
            if (_transitionState == ScreenTransitionState.Forward)
            {
                _transitionScreen?.Draw(_spriteBatch);
            }
            else
            {
                _screenStack.Peek().Draw(_spriteBatch);
            }

            // Change the render target back
            GraphicsDevice.SetRenderTarget(null);

            // Draw current Screen
            if (_transitionState == ScreenTransitionState.Forward)
            {
                _screenStack.Peek().Draw(_spriteBatch);
            }
            else
            {
                _transitionScreen?.Draw(_spriteBatch);
            }

            // Begin draw call
            _spriteBatch?.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            // Set the transition mask
            GraphicsDevice.Textures[1] = TransitionEffect.TransitionMask;

            // Set parameters on the used effect
            TransitionEffect.TransitionProgress = _transitionAmount;

            // Start effect
            TransitionEffect.Begin();

            // Draw texture of current Screen
            _spriteBatch?.Draw(_renderTarget, Vector2.Zero, Color.White);

            // End draw and the effect
            _spriteBatch?.End();
            TransitionEffect.End();
        }
        else if (Transitioning)
        {
            // Calculate an overlay color for the transition
            Vector3 overlay = _transitionAmount < 0.5F
                ? Vector3.Lerp(Color.White.ToVector3(), Color.Black.ToVector3(), _transitionAmount * 2)
                : Vector3.Lerp(Color.Black.ToVector3(), Color.White.ToVector3(), (_transitionAmount - 0.5F) * 2);

            // Set render target and clear device
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.Black);

            // Draw current screen
            if (_transitionState == ScreenTransitionState.Forward)
            {
                if (_transitionAmount < 0.5F)
                {
                    _screenStack.Peek().Draw(_spriteBatch);
                }
                else
                {
                    _transitionScreen?.Draw(_spriteBatch);
                }
            }
            else
            {
                if (_transitionAmount < 0.5F)
                {
                    _transitionScreen?.Draw(_spriteBatch);
                }
                else
                {
                    _screenStack.Peek().Draw(_spriteBatch);
                }
            }

            // Reset the device
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch?.Begin();

            // Draw screen texture using the overlay color
            _spriteBatch?.Draw(_renderTarget, Vector2.Zero, new Color(overlay));

            _spriteBatch?.End();
        }
        else
            // Draw current Screen
        {
            _screenStack.Peek().Draw(_spriteBatch);
        }
    }
}