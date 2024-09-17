using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNALibrary.Interfaces;

namespace XNALibrary.ScreenManagement;

public class ScreenButton
{
    private Type _screenTypeLink;

    /// <summary>
    /// The type of Screen linked to by the ScreenButton. A link set to null means the button links
    /// to the previous Screen.
    /// </summary>
    public Type ScreenLink
    {
        get => _screenTypeLink;
        set => _screenTypeLink = value;
    }

    private Texture2D _texture;

    /// <summary>
    /// Gets and Sets the texture. Set to null for no texture.
    /// </summary>
    public Texture2D Texture
    {
        get => _texture;
        set => _texture = value;
    }

    private Rectangle _sourceRectangle;

    /// <summary>
    /// Gets and Setst the source are of the texture used when drawing the button.
    /// </summary>
    public Rectangle SourceRectangle
    {
        get => _sourceRectangle;
        set => _sourceRectangle = value;
    }

    private Color _selectedColor = Color.White;

    /// <summary>
    /// Gets and Sets the tint color used when the button is selected. By default set to Color.White.
    /// </summary>
    public Color SelectedColor
    {
        get => _selectedColor;
        set => _selectedColor = value;
    }

    private Vector2 _position;

    /// <summary>
    /// Gets and Sets the position of the ScreenButton's top-left corner.
    /// </summary>
    public Vector2 Position
    {
        get => _position;
        set => _position = value;
    }

    private readonly IInputHandler _inputHandler;

    private bool _pressed;

    /// <summary>
    /// Gets a boolean value indicating whether the ScreenButton has been pressed.
    /// </summary>
    public bool IsPressed => _pressed;

    private bool _isSelected;

    /// <summary>
    /// Gets and Sets whether the ScreenButton is selected or not.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => _isSelected = value;
    }

    private bool _readMouse;

    /// <summary>
    /// Gets and Sets whether the ScreenButton should react to mouse input. Set to false by default.
    /// </summary>
    public bool ReadMouseInput
    {
        get => _readMouse;
        set => _readMouse = value;
    }

    public ScreenButton(IInputHandler inputHandler, Texture2D texture)
    {
        _inputHandler = inputHandler;
        _sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
        _texture = texture;
    }

    /// <summary>
    /// Called when the ScreenButton should handle any input. If the ScreenButton handles mouse input
    /// the method checks whether the mouse is currently over the button and if it's pressed. Current
    /// implementation does not use per-pixel checks so therefore overriding this method may be
    /// desirable if such are needed.
    /// </summary>
    public virtual void HandleInput()
    {
        // Read mouse input?
        if (_readMouse)
        {
            // Button is always not pressed until proven otherwise
            _pressed = false;

            // Button pressed and mouse in correct area?
            if (_inputHandler.CurrentMouseState.X > _position.X &&
                _inputHandler.CurrentMouseState.X < _position.X + _texture.Width &&
                _inputHandler.CurrentMouseState.Y > _position.Y &&
                _inputHandler.CurrentMouseState.Y < _position.Y + _texture.Height)
            {
                _isSelected = true;

                // Button pressed?
                if (_inputHandler.CurrentMouseState.LeftButton == ButtonState.Pressed &&
                    _inputHandler.OldMouseState.LeftButton == ButtonState.Released)
                    _pressed = true;
            }
            else
                _isSelected = false;
        }
    }

    /// <summary>
    /// Override this method to perform any update logic.
    /// </summary>
    /// <param name="gameTime"></param>
    public virtual void Update(GameTime gameTime)
    {
    }

    /// <summary>
    /// Called when the ScreenButton needs to draw itself.
    /// </summary>
    /// <param name="spriteBatch"></param>
    public virtual void Draw(SpriteBatch spriteBatch)
    {
        if (_texture != null)
        {
            if (_isSelected)
                spriteBatch.Draw(_texture, _position, _sourceRectangle, _selectedColor);
            else
                spriteBatch.Draw(_texture, _position, _sourceRectangle, Color.White);
        }
    }
}