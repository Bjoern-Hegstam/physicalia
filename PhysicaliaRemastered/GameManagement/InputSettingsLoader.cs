using System;
using System.Xml;
using Microsoft.Xna.Framework;
using PhysicaliaRemastered.Input;
using XNALibrary.Input;

namespace PhysicaliaRemastered.GameManagement;

public static class InputSettingsLoader
{
    public static InputSettings Load(string path, Game game)
    {
        InputType? inputType = null;
        KeyboardInputMap? keyboardInputMap = null;
        GamePadInputMap? gamePadInputMap = null;
        
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

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "KeyboardMap" })
            {
                keyboardInputMap = KeyboardInputMapLoader.Load(reader.ReadString(), game.Services.GetService<InputHandler>());
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "GamepadMap" })
            {
                gamePadInputMap = GamePadInputMapLoader.Load(reader.ReadString(), game.Services.GetService<InputHandler>());
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Settings" })
            {
                break;
            }
        }

        return new InputSettings
        {
            InputType = inputType ?? throw new NullReferenceException(nameof(inputType)),
            KeyboardMap = keyboardInputMap ?? throw new NullReferenceException(nameof(keyboardInputMap)),
            GamePadMap = gamePadInputMap ?? throw new NullReferenceException(nameof(gamePadInputMap))
        };
    }
}