namespace PhysicaliaRemastered.Input
{
    /// <summary>
    /// Enumeration of the available actions.
    /// </summary>
    public enum InputAction
    {
        MenuUp,
        MenuDown,
        MenuLeft,
        MenuRight,
        MenuStart,
        MenuBack,
        NextWeapon,
        PreviousWeapon,
        WalkLeft,
        WalkRight,
        Jump,
        Shoot,
    }

    /// <summary>
    /// Intended to work as a layer of abstraction between the input device used
    /// and the actions available in Physicalia.
    /// </summary>
    public abstract class InputMap
    {
        private XNALibrary.Services.IInputHandler inputHandler;

        public XNALibrary.Services.IInputHandler InputHandler
        {
            get { return this.inputHandler; }
            set { this.inputHandler = value; }
        }

        public abstract bool IsPressed(InputAction action);
        public abstract bool IsHolding(InputAction action);
        public abstract bool IsReleased(InputAction action);

        public abstract void SetButton(InputAction action, int button);
        public abstract void LoadXml(string path);
    }
}
