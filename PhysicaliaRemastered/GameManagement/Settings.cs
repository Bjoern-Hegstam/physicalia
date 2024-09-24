using System;
using PhysicaliaRemastered.Input;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.GameManagement;

public class Settings
{
    public static Random Random { get; } = new();

    public required KeyboardInputMap KeyboardMap { get; init; }
    public required GamePadInputMap GamePadMap { get; init; }

    public required InputType InputType { get; init; }
    public InputMap InputMap => InputType switch
    {
        InputType.Gamepad => GamePadMap,
        InputType.Keyboard => KeyboardMap,
        _ => throw new ArgumentOutOfRangeException()
    };
    
    public required float PlayerStartHealth { get; init; }
    public required Sprite FullHealthUi { get; init; }
    public required Sprite EmptyHealthUi { get; init; }
}