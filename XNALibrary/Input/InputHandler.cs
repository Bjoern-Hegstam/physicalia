using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNALibrary.Interfaces;

namespace XNALibrary.Input;

/// <summary>
/// The InputHandler updates all available input devices and provides
/// properties and methods for checking the devices current state.
/// </summary>
public class InputHandler : GameComponent, IInputHandler
{
    KeyboardState oldKeyboardState;
    KeyboardState currentKeyboardState;

    MouseState oldMouseState;
    MouseState currentMouseState;

    GamePadState[] oldGamePadState = new GamePadState[4];
    GamePadState[] currentGamePadState = new GamePadState[4];

    /// <summary>
    /// Gets the old state of the keyboard.
    /// </summary>
    public KeyboardState OldKeyBoardState
    {
        get { return this.oldKeyboardState; }
    }

    /// <summary>
    /// Gets the current state of the keyboard.
    /// </summary>
    public KeyboardState CurrentKeyBoardState
    {
        get { return this.currentKeyboardState; }
    }

    /// <summary>
    /// Gets the old state of the mouse.
    /// </summary>
    public MouseState OldMouseState
    {
        get { return this.oldMouseState; }
    }

    /// <summary>
    /// Gets the current state of the mouse.
    /// </summary>
    public MouseState CurrentMouseState
    {
        get { return this.currentMouseState; }
    }

    public GamePadState[] OldGamePadState
    {
        get { return this.oldGamePadState; }
    }

    public GamePadState[] CurrentGamePadState
    {
        get { return this.currentGamePadState; }
    }

    /// <summary>
    /// Creates a new InputHandler. The instance is automatically added
    /// to the games collection of services.
    /// </summary>
    /// <param name="game">Game to associate the instance with.</param>
    public InputHandler(Game game)
        : base(game)
    {
        // Add the InputHandler to the game's collection of Services
        game.Services.AddService(typeof(IInputHandler), this);
    }

    /// <summary>
    /// Checks if a key that was up during the last update has been pressed.
    /// </summary>
    /// <param name="key">Key to evaluate.</param>
    /// <returns>True if the key has been pressed; false otherwise.</returns>
    public bool IsPressed(Keys key)
    {
        return this.oldKeyboardState.IsKeyUp(key) &&
               this.currentKeyboardState.IsKeyDown(key);
    }

    /// <summary>
    /// Checks if a button that was up during the last update has been pressed.
    /// </summary>
    /// <param name="playerIndex">PlayerIndex of the gamepad the button belongs to.</param>
    /// <param name="button">Button to evaluate.</param>
    /// <returns>True if the button has been pressed; false otherwise.</returns>
    public bool IsPressed(PlayerIndex playerIndex, Buttons button)
    {
        return this.oldGamePadState[(int)playerIndex].IsButtonUp(button) &&
               this.currentGamePadState[(int)playerIndex].IsButtonDown(button);
    }

    /// <summary>
    /// Checks if a key that was down during the last update has been released.
    /// </summary>
    /// <param name="key">Key to evaluate.</param>
    /// <returns>True if the key has been released; false otherwise.</returns>
    public bool IsReleased(Keys key)
    {
        return this.oldKeyboardState.IsKeyDown(key) &&
               this.currentKeyboardState.IsKeyUp(key);
    }

    /// <summary>
    /// Checks if a button that was down during the last update has been released.
    /// </summary>
    /// <param name="playerIndex">PlayerIndex of the gamepad the button belongs to.</param>
    /// <param name="button">Button to evaluate.</param>
    /// <returns>True if the button has been released; false otherwise.</returns>
    public bool IsReleased(PlayerIndex playerIndex, Buttons button)
    {
        return this.oldGamePadState[(int)playerIndex].IsButtonDown(button) &&
               this.currentGamePadState[(int)playerIndex].IsButtonUp(button);
    }

    /// <summary>
    /// Checks if a key is being held down.
    /// </summary>
    /// <param name="key">Key to evaluate.</param>
    /// <returns>True if the key is being held down; false otherwise.</returns>
    public bool IsHolding(Keys key)
    {
        return this.oldKeyboardState.IsKeyDown(key) &&
               this.currentKeyboardState.IsKeyDown(key);
    }

    /// <summary>
    /// Checks if a button is being held down.
    /// </summary>
    /// <param name="key">Button to evaluate.</param>
    /// <returns>True if the button is being held down; false otherwise.</returns>
    public bool IsHolding(PlayerIndex playerIndex, Buttons button)
    {
        return this.oldGamePadState[(int)playerIndex].IsButtonDown(button) &&
               this.currentGamePadState[(int)playerIndex].IsButtonDown(button);
    }

    /// <summary>
    /// Calculates the total distance the mouse has moved since the last update.
    /// </summary>
    /// <returns>The moved distance as a Vector2.</returns>
    public Vector2 GetMouseMove()
    {
        Vector2 distance = new Vector2(this.currentMouseState.X - this.oldMouseState.X,
            this.currentMouseState.Y - this.oldMouseState.Y);

        return distance;
    }

    /// <summary>
    /// Calculates the total distance the mouse has moved since the last update.
    /// </summary>
    /// <param name="distance">Vector2 to store the result in.</param>
    public void GetMouseMove(out Vector2 distance)
    {
        distance = new Vector2(this.currentMouseState.X - this.oldMouseState.X,
            this.currentMouseState.Y - this.oldMouseState.Y);
    }

    /// <summary>
    /// Calculates the normalized distance the mouse has moved since the last update.
    /// </summary>
    /// <returns>The moved distance as a normalized Vector2.</returns>
    public Vector2 GetNormalizedMouseMove()
    {
        Vector2 distance = new Vector2(this.currentMouseState.X - this.oldMouseState.X,
            this.currentMouseState.Y - this.oldMouseState.Y);

        distance.Normalize();
        return distance;
    }

    /// <summary>
    /// Calculates the normalized distance the mouse has moved since the last update.
    /// </summary>
    /// <param name="distance">Vector2 to store the normalized result in.</param>
    public void GetNormalizedMouseMove(out Vector2 distance)
    {
        distance = new Vector2(this.currentMouseState.X - this.oldMouseState.X,
            this.currentMouseState.Y - this.oldMouseState.Y);
        distance.Normalize();
    }

    public override void Initialize()
    {
        // Set the initial states of all state members
        this.oldMouseState = this.currentMouseState = Mouse.GetState();
        this.oldKeyboardState = this.currentKeyboardState = Keyboard.GetState();

        foreach (PlayerIndex index in Enum.GetValues(typeof(PlayerIndex)))
        {
            int i = (int)index;
            this.oldGamePadState[i] = this.currentGamePadState[i] = GamePad.GetState(index);
        }

        base.Initialize();
    }

    /// <summary>
    /// Updates all available input devices.
    /// </summary>
    /// <param name="gameTime"></param>
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Keyboard
        this.oldKeyboardState = this.currentKeyboardState;
        this.currentKeyboardState = Keyboard.GetState();

        // Mouse
        this.oldMouseState = this.currentMouseState;
        this.currentMouseState = Mouse.GetState();

        // Gamepads
        foreach (PlayerIndex index in Enum.GetValues(typeof(PlayerIndex)))
        {
            int i = (int)index;
            this.oldGamePadState[i] = this.currentGamePadState[i];
            this.currentGamePadState[i] = GamePad.GetState(index);
        }
    }
}