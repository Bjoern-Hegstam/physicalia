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
        ArgumentNullException.ThrowIfNull(screenType);

        if (Transitioning)
        {
            return false;
        }


        _transitionScreen = Screens.SingleOrDefault(screen => screen.GetType() == screenType);
        if (_transitionScreen == null)
        {
            return false;
        }

        _transitionState = ScreenTransitionState.Forward;
        StartTransition();

        return true;
    }

    /// <summary>
    /// Tells the ScreenManager to transition back to the previous screen.
    /// </summary>
    /// <returns>True if the transition was successfully initialized; false otherwise.</returns>
    public bool TransitionBack()
    {
        if (_screenStack.Count <= 1 || Transitioning)
        {
            return false;
        }

        _transitionScreen = _screenStack.Pop();
        _transitionState = ScreenTransitionState.Backward;
        StartTransition();
        return true;
    }

    /// <summary>
    /// Sets the variables needed for starting the transition to a new Screen.
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
        _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        
        _renderTarget = new RenderTarget2D(
            Game.GraphicsDevice,
            Game.GraphicsDevice.PresentationParameters.BackBufferWidth,
            Game.GraphicsDevice.PresentationParameters.BackBufferHeight
        );

        foreach (Screen screen in Screens)
        {
            screen.LoadContent(Game.Content);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (_screenStack.Count == 0)
        {
            return;
        }

        if (Transitioning)
        {
            _screenStack.Peek().Update(gameTime, false);
            _transitionScreen?.Update(gameTime, false);

            _transitionAmount += (float)gameTime.ElapsedGameTime.TotalSeconds * _transitionSpeed;

            if (_transitionAmount >= 1.0F)
            {
                EndTransition();
            }
        }
        else
        {
            _screenStack.Peek().Update(gameTime, true);
        }
    }

    public override void Draw(GameTime gameTime)
    {
        if (_screenStack.Count == 0)
        {
            return;
        }

        if (Transitioning && TransitionEffect != null)
        {
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.Black);

            if (_transitionState == ScreenTransitionState.Forward)
            {
                _transitionScreen?.Draw(_spriteBatch);
            }
            else
            {
                _screenStack.Peek().Draw(_spriteBatch);
            }

            GraphicsDevice.SetRenderTarget(null);

            if (_transitionState == ScreenTransitionState.Forward)
            {
                _screenStack.Peek().Draw(_spriteBatch);
            }
            else
            {
                _transitionScreen?.Draw(_spriteBatch);
            }

            _spriteBatch?.Begin();

            GraphicsDevice.Textures[1] = TransitionEffect.TransitionMask;

            TransitionEffect.TransitionProgress = _transitionAmount;

            TransitionEffect.Begin();

            _spriteBatch?.Draw(_renderTarget, Vector2.Zero, Color.White);

            _spriteBatch?.End();
            TransitionEffect.End();
        }
        else if (Transitioning)
        {
            Vector3 overlay = _transitionAmount < 0.5F
                ? Vector3.Lerp(Color.White.ToVector3(), Color.Black.ToVector3(), _transitionAmount * 2)
                : Vector3.Lerp(Color.Black.ToVector3(), Color.White.ToVector3(), (_transitionAmount - 0.5F) * 2);

            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.Black);

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

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch?.Begin();
            _spriteBatch?.Draw(_renderTarget, Vector2.Zero, new Color(overlay));
            _spriteBatch?.End();
        }
        else
        {
            _screenStack.Peek().Draw(_spriteBatch);
        }
    }
}