using System;
using System.Xml;
using Microsoft.Xna.Framework;
using PhysicaliaRemastered.Input;
using XNALibrary;
using XNALibrary.Input;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.GameManagement;

public static class SettingsLoader
{
    public static Settings Load(string path, Game game)
    {
        InputType? inputType = null;
        int? playerStartHealth = null;
        KeyboardInputMap? keyboardInputMap = null;
        GamePadInputMap? gamePadInputMap = null;
        Sprite? fullHealthUi = null;
        Sprite? emptyHealthUi = null;
        
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
                inputType = Enum.Parse<InputType>(reader.ReadString());
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "PlayerHealth" })
            {
                playerStartHealth = int.Parse(reader.ReadString());
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "KeyboardMap" })
            {
                keyboardInputMap = KeyboardInputMapLoader.Load(reader.ReadString(), game.Services.GetService<InputHandler>());
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "GamepadMap" })
            {
                gamePadInputMap = GamePadInputMapLoader.Load(reader.ReadString(), game.Services.GetService<InputHandler>());
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "UI" })
            {
                var spriteLibrary = game.Services.GetService<SpriteLibrary>();
                reader.ReadToFollowing("FullHealthBar");
                var fullBarSpriteId = new SpriteId(reader.GetAttribute("spriteId") ?? throw new ResourceLoadException());
                fullHealthUi = spriteLibrary.GetSprite(fullBarSpriteId);

                reader.ReadToFollowing("EmptyHealthBar");
                var emptyBarSpriteId = new SpriteId(reader.GetAttribute("spriteId") ?? throw new ResourceLoadException());
                emptyHealthUi = spriteLibrary.GetSprite(emptyBarSpriteId);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Settings" })
            {
                break;
            }
        }

        return new Settings()
        {
            InputType = inputType ?? throw new NullReferenceException(nameof(inputType)),
            KeyboardMap = keyboardInputMap ?? throw new NullReferenceException(nameof(keyboardInputMap)),
            GamePadMap = gamePadInputMap ?? throw new NullReferenceException(nameof(gamePadInputMap)),
            PlayerStartHealth = playerStartHealth ?? throw new NullReferenceException(nameof(playerStartHealth)),
            FullHealthUi = fullHealthUi ?? throw new NullReferenceException(nameof(fullHealthUi)),
            EmptyHealthUi = emptyHealthUi ?? throw new NullReferenceException(nameof(emptyHealthUi))
        };
    }
}