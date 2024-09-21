using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Input;
using XNALibrary;
using XNALibrary.Input;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.GameManagement;

public class Settings
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

    public SpriteFont? WorldQuoteFont { get; private set; }

    public SpriteFont? WorldIndexFont { get; private set; }

    public SpriteFont? LevelIndexFont { get; private set; }

    public SpriteFont? PlayerDeadFont { get; private set; }

    public SpriteFont? WeaponAmmoFont { get; private set; }

    public SpriteFont? PauseMenuFont { get; private set; }

    public Sprite? FullHealthUi { get; private set; }

    public Sprite? EmptyHealthUi { get; private set; }

    static Settings()
    {
        Random = new Random();
    }

    public Settings(GameServiceContainer gameServiceContainer)
    {
        var inputHandler = gameServiceContainer.GetService<InputHandler>();
        ArgumentNullException.ThrowIfNull(inputHandler);
        
        _gamePadMap = new GamePadInputMap
        {
            InputHandler = inputHandler
        };

        _keyboardMap = new KeyboardInputMap
        {
            InputHandler = inputHandler
        };

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

    public void LoadXml(string path, SpriteLibrary spriteLibrary)
    {
        var readerSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true
        };

        using var reader = XmlReader.Create(path, readerSettings);
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "InputType" })
            {
                var input = (InputType)Enum.Parse(typeof(InputType), reader.ReadString());
                InputType = input;
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "PlayerHealth" })
            {
                int health = int.Parse(reader.ReadString());
                PlayerStartHealth = health;
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "KeyboardMap" })
            {
                _keyboardMap.LoadXml(reader.ReadString());
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "GamepadMap" })
            {
                _gamePadMap.LoadXml(reader.ReadString());
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "UI" })
            {
                reader.ReadToFollowing("FullHealthBar");
                SpriteId fullBarSpriteId =
                    new SpriteId(reader.GetAttribute("spriteId") ?? throw new ResourceLoadException());
                FullHealthUi = spriteLibrary.GetSprite(fullBarSpriteId);

                reader.ReadToFollowing("EmptyHealthBar");
                SpriteId emptyBarSpriteId =
                    new SpriteId(reader.GetAttribute("spriteId") ?? throw new ResourceLoadException());
                EmptyHealthUi = spriteLibrary.GetSprite(emptyBarSpriteId);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Settings" })
            {
                break;
            }
        }
    }
}