using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNALibrary.Interfaces;

namespace XNALibrary.Graphics.ScreenManagement;

public class ScreenButton
{
    private Type screenTypeLink;

    /// <summary>
    /// The type of Screen linked to by the ScreenButton. A link set to null means the button links
    /// to the previous Screen.
    /// </summary>
    public Type ScreenLink
    {
        get { return screenTypeLink; }
        set { screenTypeLink = value; }
    }

    private Texture2D texture;

    /// <summary>
    /// Gets and Sets the texture. Set to null for no texture.
    /// </summary>
    public Texture2D Texture
    {
        get { return texture; }
        set { texture = value; }
    }

    private Rectangle sourceRectangle;

    /// <summary>
    /// Gets and Setst the source are of the texture used when drawing the button.
    /// </summary>
    public Rectangle SourceRectangle
    {
        get { return sourceRectangle; }
        set { sourceRectangle = value; }
    }

    private Color selectedColor = Color.White;

    /// <summary>
    /// Gets and Sets the tint color used when the button is selected. By default set to Color.White.
    /// </summary>
    public Color SelectedColor
    {
        get { return selectedColor; }
        set { selectedColor = value; }
    }

    private Vector2 position;

    /// <summary>
    /// Gets and Sets the position of the ScreenButton's top-left corner.
    /// </summary>
    public Vector2 Position
    {
        get { return position; }
        set { position = value; }
    }

    private IInputHandler inputHandler;

    private bool pressed = false;

    /// <summary>
    /// Gets a boolean value indicating whether the ScreenButton has been pressed.
    /// </summary>
    public bool IsPressed
    {
        get { return pressed; }
    }

    private bool isSelected = false;

    /// <summary>
    /// Gets and Sets whether the ScreenButton is selected or not.
    /// </summary>
    public bool IsSelected
    {
        get { return isSelected; }
        set { isSelected = value; }
    }

    private bool readMouse = false;

    /// <summary>
    /// Gets and Sets whether the ScreenButton should react to mouse input. Set to false by default.
    /// </summary>
    public bool ReadMouseInput
    {
        get { return readMouse; }
        set { readMouse = value; }
    }

    public ScreenButton(IInputHandler inputHandler, Texture2D texture)
    {
        this.inputHandler = inputHandler;
        sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
        this.texture = texture;
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
        if (readMouse)
        {
            // Button is always not pressed until proven otherwise
            pressed = false;

            // Button pressed and mouse in correct area?
            if (inputHandler.CurrentMouseState.X > position.X &&
                inputHandler.CurrentMouseState.X < position.X + texture.Width &&
                inputHandler.CurrentMouseState.Y > position.Y &&
                inputHandler.CurrentMouseState.Y < position.Y + texture.Height)
            {
                isSelected = true;

                // Button pressed?
                if (inputHandler.CurrentMouseState.LeftButton == ButtonState.Pressed &&
                    inputHandler.OldMouseState.LeftButton == ButtonState.Released)
                    pressed = true;
            }
            else
                isSelected = false;
        }
    }

    /// <summary>
    /// Override this method to perform any update logic.
    /// </summary>
    /// <param name="gameTime"></param>
    public virtual void Update(GameTime gameTime) { }

    /// <summary>
    /// Called when the ScreenButton needs to draw itself.
    /// </summary>
    /// <param name="spriteBatch"></param>
    public virtual void Draw(SpriteBatch spriteBatch)
    {
        if (texture != null)
        {
            if (isSelected)
                spriteBatch.Draw(texture, position, sourceRectangle, selectedColor);
            else
                spriteBatch.Draw(texture, position, sourceRectangle, Color.White);
        }
    }
}