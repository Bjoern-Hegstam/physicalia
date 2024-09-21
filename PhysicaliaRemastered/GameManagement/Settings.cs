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

public class Settings(Game game)
{
    private readonly KeyboardInputMap _keyboardMap = new(game.Services.GetService<InputHandler>());

    private readonly GamePadInputMap _gamePadMap = new(game.Services.GetService<InputHandler>());

    public InputType InputType { get; set; }

    public InputMap InputMap => InputType switch
    {
        InputType.Gamepad => _gamePadMap,
        InputType.Keyboard => _keyboardMap,
        _ => throw new ArgumentOutOfRangeException()
    };

    public static Random Random { get; } = new();

    public float PlayerStartHealth { get; set; }

    public SpriteFont WorldQuoteFont => game.Content.Load<SpriteFont>("Fonts/WorldQuoteFont");
    public SpriteFont WorldIndexFont => game.Content.Load<SpriteFont>("Fonts/WorldIndexFont");
    public SpriteFont LevelIndexFont => game.Content.Load<SpriteFont>("Fonts/LevelIndexFont");
    public SpriteFont PlayerDeadFont => game.Content.Load<SpriteFont>("Fonts/PlayerDeadFont");
    public SpriteFont WeaponAmmoFont => game.Content.Load<SpriteFont>("Fonts/WeaponAmmoFont");
    public SpriteFont PauseMenuFont => game.Content.Load<SpriteFont>("Fonts/PauseMenuFont");

    public Sprite? FullHealthUi { get; private set; }
    public Sprite? EmptyHealthUi { get; private set; }

    public void LoadContent(ContentManager contentManager)
    {
        // Access fonts to force load
        SpriteFont ignored = WorldQuoteFont;
        ignored = WorldIndexFont;
        ignored = LevelIndexFont;
        ignored = PlayerDeadFont;
        ignored = WeaponAmmoFont;
        ignored = PauseMenuFont;
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
                var input = Enum.Parse<InputType>(reader.ReadString());
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