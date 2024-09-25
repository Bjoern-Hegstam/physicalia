using System;
using PhysicaliaRemastered.Input;

namespace PhysicaliaRemastered.GameManagement;

public class InputSettings
{
    public required KeyboardInputMap KeyboardMap { get; init; }
    public required GamePadInputMap GamePadMap { get; init; }

    public required InputType InputType { get; init; }
    public InputMap InputMap => InputType switch
    {
        InputType.Gamepad => GamePadMap,
        InputType.Keyboard => KeyboardMap,
        _ => throw new ArgumentOutOfRangeException()
    };
}