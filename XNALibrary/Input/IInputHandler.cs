using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace XNALibrary.Interfaces;

public interface IInputHandler
{
    KeyboardState OldKeyBoardState { get; }
    KeyboardState CurrentKeyBoardState { get; }

    MouseState OldMouseState { get; }
    MouseState CurrentMouseState { get; }

    GamePadState[] OldGamePadState { get; }
    GamePadState[] CurrentGamePadState { get; }

    bool IsPressed(Keys key);
    bool IsPressed(PlayerIndex playerIndex, Buttons button);

    bool IsReleased(Keys key);
    bool IsReleased(PlayerIndex playerIndex, Buttons button);

    bool IsHolding(Keys key);
    bool IsHolding(PlayerIndex playerIndex, Buttons button);

    Vector2 GetMouseMove();
    Vector2 GetNormalizedMouseMove();

    void GetMouseMove(out Vector2 distance);
    void GetNormalizedMouseMove(out Vector2 distance);
}