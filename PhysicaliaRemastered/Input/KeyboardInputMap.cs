using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using XNALibrary.Input;

namespace PhysicaliaRemastered.Input;

public class KeyboardInputMap(InputHandler inputHandler) : InputMap
{
    private readonly Dictionary<InputAction, Keys> _keys = new();

    public override bool IsPressed(InputAction action)
    {
        return inputHandler.IsPressed(_keys[action]);
    }

    public override bool IsHolding(InputAction action)
    {
        return inputHandler.IsHolding(_keys[action]);
    }

    public override bool IsReleased(InputAction action)
    {
        return inputHandler.IsReleased(_keys[action]);
    }

    public virtual void SetButton(InputAction action, Keys button)
    {
        _keys[action] = button;
    }
}