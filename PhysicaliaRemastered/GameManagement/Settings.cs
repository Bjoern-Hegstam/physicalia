using System;
using System.Xml;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Input;
using XNALibrary.Interfaces;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.GameManagement;

public enum InputType
{
    Gamepad,
    Keyboard
}

/// <summary>
/// Contains the data needed to create the game.
/// </summary>
public class Settings : ISettings
{
    private InputType _inputType;
    private readonly KeyboardInputMap _keyboardMap;
    private readonly GamePadInputMap _gamePadMap;

    public InputType InputType
    {
        get => _inputType;
        set
        {
            _inputType = value;

            // Change current input map if needed
            if (InputMap == _keyboardMap &&
                _inputType == InputType.Gamepad)
            {
                InputMap = _gamePadMap;
            }
        }
    }

    public InputMap InputMap { get; private set; }

    public static Random Random { get; }

    public float PlayerStartHealth { get; set; }

    public SpriteFont WorldQuoteFont { get; private set; }

    public SpriteFont WorldIndexFont { get; private set; }

    public SpriteFont LevelIndexFont { get; private set; }

    public SpriteFont PlayerDeadFont { get; private set; }

    public SpriteFont WeaponAmmoFont { get; private set; }

    public SpriteFont PauseMenuFont { get; private set; }

    public Sprite FullHealthUi { get; private set; }

    public Sprite EmptyHealthUi { get; private set; }

    static Settings()
    {
        Random = new Random();
    }

    public Settings(IInputHandler inputHandler)
    {
        IInputHandler input = inputHandler;

        _gamePadMap = new GamePadInputMap();
        _gamePadMap.InputHandler = input;

        _keyboardMap = new KeyboardInputMap();
        _keyboardMap.InputHandler = input;

        InputMap = _keyboardMap;
        _inputType = InputType.Keyboard;
    }

    public void LoadContent(ContentManager contentManager)
    {
        WorldQuoteFont = contentManager.Load<SpriteFont>(@"Fonts\WorldQuoteFont");
        WorldIndexFont = contentManager.Load<SpriteFont>(@"Fonts\WorldIndexFont");
        LevelIndexFont = contentManager.Load<SpriteFont>(@"Fonts\LevelIndexFont");
        PlayerDeadFont = contentManager.Load<SpriteFont>(@"Fonts\PlayerDeadFont");
        WeaponAmmoFont = contentManager.Load<SpriteFont>(@"Fonts\WeaponAmmoFont");
        PauseMenuFont = contentManager.Load<SpriteFont>(@"Fonts\PauseMenuFont");
    }

    public void LoadXml(string path, ISpriteLibrary spriteLibrary)
    {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.IgnoreComments = true;
        readerSettings.IgnoreWhitespace = true;
        readerSettings.IgnoreProcessingInstructions = true;

        using XmlReader reader = XmlReader.Create(path, readerSettings);
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "InputType")
            {
                InputType input = (InputType)Enum.Parse(typeof(InputType), reader.ReadString());
                InputType = input;
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "PlayerHealth")
            {
                int health = int.Parse(reader.ReadString());
                PlayerStartHealth = health;
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "KeyboardMap")
            {
                _keyboardMap.LoadXml(reader.ReadString());
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "GamepadMap")
            {
                _gamePadMap.LoadXml(reader.ReadString());
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "UI")
            {
                reader.ReadToFollowing("FullHealthBar");
                int spriteKey = int.Parse(reader.GetAttribute("key"));
                FullHealthUi = spriteLibrary.GetSprite(spriteKey);

                reader.ReadToFollowing("EmptyHealthBar");
                spriteKey = int.Parse(reader.GetAttribute("key"));
                EmptyHealthUi = spriteLibrary.GetSprite(spriteKey);
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "Settings")
            {
                break;
            }
        }
    }
}

// Interface for providing the Settings as a service that can be accessed via the Game.
public interface ISettings
{
    InputType InputType { get; set; }
    InputMap InputMap { get; }

    float PlayerStartHealth { get; set; }

    SpriteFont WorldQuoteFont { get; }
    SpriteFont WorldIndexFont { get; }
    SpriteFont LevelIndexFont { get; }
    SpriteFont PlayerDeadFont { get; }
    SpriteFont WeaponAmmoFont { get; }
    SpriteFont PauseMenuFont { get; }

    Sprite FullHealthUi { get; }
    Sprite EmptyHealthUi { get; }

    void LoadContent(ContentManager contentManager);
    void LoadXml(string path, ISpriteLibrary spriteLibrary);
}