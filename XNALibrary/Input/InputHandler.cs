using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace XNALibrary.Input;

/// <summary>
/// The InputHandler updates all available input devices and provides
/// properties and methods for checking the devices current state.
/// </summary>
public class InputHandler : GameComponent
{
    private KeyboardState _oldKeyboardState;
    private KeyboardState _currentKeyboardState;

    private MouseState _oldMouseState;
    private MouseState _currentMouseState;

    private readonly GamePadState[] _currentGamePadState = new GamePadState[4];
    private readonly GamePadState[] _oldGamePadState = new GamePadState[4];

    /// <summary>
    /// Creates a new InputHandler. The instance is automatically added
    /// to the games collection of services.
    /// </summary>
    /// <param name="game">Game to associate the instance with.</param>
    public InputHandler(Game game)
        : base(game)
    {
        // Add the InputHandler to the game's collection of Services
        game.Services.AddService(typeof(InputHandler), this);
    }

    /// <summary>
    /// Checks if a key that was up during the last update has been pressed.
    /// </summary>
    /// <param name="key">Key to evaluate.</param>
    /// <returns>True if the key has been pressed; false otherwise.</returns>
    public bool IsPressed(Keys key)
    {
        return _oldKeyboardState.IsKeyUp(key) &&
               _currentKeyboardState.IsKeyDown(key);
    }

    /// <summary>
    /// Checks if a button that was up during the last update has been pressed.
    /// </summary>
    /// <param name="playerIndex">PlayerIndex of the gamepad the button belongs to.</param>
    /// <param name="button">Button to evaluate.</param>
    /// <returns>True if the button has been pressed; false otherwise.</returns>
    public bool IsPressed(PlayerIndex playerIndex, Buttons button)
    {
        return _oldGamePadState[(int)playerIndex].IsButtonUp(button) &&
               _currentGamePadState[(int)playerIndex].IsButtonDown(button);
    }

    /// <summary>
    /// Checks if a key that was down during the last update has been released.
    /// </summary>
    /// <param name="key">Key to evaluate.</param>
    /// <returns>True if the key has been released; false otherwise.</returns>
    public bool IsReleased(Keys key)
    {
        return _oldKeyboardState.IsKeyDown(key) &&
               _currentKeyboardState.IsKeyUp(key);
    }

    /// <summary>
    /// Checks if a button that was down during the last update has been released.
    /// </summary>
    /// <param name="playerIndex">PlayerIndex of the gamepad the button belongs to.</param>
    /// <param name="button">Button to evaluate.</param>
    /// <returns>True if the button has been released; false otherwise.</returns>
    public bool IsReleased(PlayerIndex playerIndex, Buttons button)
    {
        return _oldGamePadState[(int)playerIndex].IsButtonDown(button) &&
               _currentGamePadState[(int)playerIndex].IsButtonUp(button);
    }

    /// <summary>
    /// Checks if a key is being held down.
    /// </summary>
    /// <param name="key">Key to evaluate.</param>
    /// <returns>True if the key is being held down; false otherwise.</returns>
    public bool IsHolding(Keys key)
    {
        return _oldKeyboardState.IsKeyDown(key) &&
               _currentKeyboardState.IsKeyDown(key);
    }

    /// <summary>
    /// Checks if a button is being held down.
    /// </summary>
    /// <returns>True if the button is being held down; false otherwise.</returns>
    public bool IsHolding(PlayerIndex playerIndex, Buttons button)
    {
        return _oldGamePadState[(int)playerIndex].IsButtonDown(button) &&
               _currentGamePadState[(int)playerIndex].IsButtonDown(button);
    }

    /// <summary>
    /// Calculates the total distance the mouse has moved since the last update.
    /// </summary>
    /// <returns>The moved distance as a Vector2.</returns>
    public Vector2 GetMouseMove()
    {
        var distance = new Vector2(_currentMouseState.X - _oldMouseState.X,
            _currentMouseState.Y - _oldMouseState.Y);

        return distance;
    }

    /// <summary>
    /// Calculates the total distance the mouse has moved since the last update.
    /// </summary>
    /// <param name="distance">Vector2 to store the result in.</param>
    public void GetMouseMove(out Vector2 distance)
    {
        distance = new Vector2(_currentMouseState.X - _oldMouseState.X,
            _currentMouseState.Y - _oldMouseState.Y);
    }

    /// <summary>
    /// Calculates the normalized distance the mouse has moved since the last update.
    /// </summary>
    /// <returns>The moved distance as a normalized Vector2.</returns>
    public Vector2 GetNormalizedMouseMove()
    {
        var distance = new Vector2(_currentMouseState.X - _oldMouseState.X,
            _currentMouseState.Y - _oldMouseState.Y);

        distance.Normalize();
        return distance;
    }

    /// <summary>
    /// Calculates the normalized distance the mouse has moved since the last update.
    /// </summary>
    /// <param name="distance">Vector2 to store the normalized result in.</param>
    public void GetNormalizedMouseMove(out Vector2 distance)
    {
        distance = new Vector2(_currentMouseState.X - _oldMouseState.X,
            _currentMouseState.Y - _oldMouseState.Y);
        distance.Normalize();
    }

    public override void Initialize()
    {
        // Set the initial states of all state members
        _oldMouseState = _currentMouseState = Mouse.GetState();
        _oldKeyboardState = _currentKeyboardState = Keyboard.GetState();

        foreach (PlayerIndex index in Enum.GetValues(typeof(PlayerIndex)))
        {
            var i = (int)index;
            _oldGamePadState[i] = _currentGamePadState[i] = GamePad.GetState(index);
        }

        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Keyboard
        _oldKeyboardState = _currentKeyboardState;
        _currentKeyboardState = Keyboard.GetState();

        // Mouse
        _oldMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();

        // Gamepads
        foreach (PlayerIndex index in Enum.GetValues(typeof(PlayerIndex)))
        {
            var i = (int)index;
            _oldGamePadState[i] = _currentGamePadState[i];
            _currentGamePadState[i] = GamePad.GetState(index);
        }
    }
}