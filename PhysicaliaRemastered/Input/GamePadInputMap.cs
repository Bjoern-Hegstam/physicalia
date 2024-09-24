using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNALibrary.Input;

namespace PhysicaliaRemastered.Input;

public class GamePadInputMap(InputHandler inputHandler) : InputMap
{
    private readonly Dictionary<InputAction, Buttons> _buttons = new();
    
    public override bool IsPressed(InputAction action)
    {
        return inputHandler.IsPressed(PlayerIndex.One, _buttons[action]);
    }

    public override bool IsHolding(InputAction action)
    {
        return inputHandler.IsHolding(PlayerIndex.One, _buttons[action]);
    }

    public override bool IsReleased(InputAction action)
    {
        return inputHandler.IsReleased(PlayerIndex.One, _buttons[action]);
    }

    public virtual void SetButton(InputAction action, Buttons button)
    {
        _buttons[action] = button;
    }
}