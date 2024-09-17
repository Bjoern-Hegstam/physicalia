using XNALibrary.Input;

namespace PhysicaliaRemastered.Input;

/// <summary>
/// Intended to work as a layer of abstraction between the input device used
/// and the actions available in Physicalia.
/// </summary>
public abstract class InputMap
{
    public InputHandler? InputHandler { get; set; }

    public abstract bool IsPressed(InputAction action);
    public abstract bool IsHolding(InputAction action);
    public abstract bool IsReleased(InputAction action);
}