using System;
using System.Xml;
using Microsoft.Xna.Framework.Input;
using XNALibrary;
using XNALibrary.Input;

namespace PhysicaliaRemastered.Input;

public static class GamePadInputMapLoader
{
    public static GamePadInputMap Load(string path, InputHandler inputHandler)
    {
        var inputMap = new GamePadInputMap(inputHandler);
        
        var readerSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true
        };

        using var reader = XmlReader.Create(path, readerSettings);
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Button" })
            {
                var action = (InputAction)Enum.Parse(
                    typeof(InputAction),
                    reader.GetAttribute("action") ?? throw new ResourceLoadException()
                );
                var button = (Buttons)Enum.Parse(
                    typeof(Buttons),
                    reader.GetAttribute("value") ?? throw new ResourceLoadException()
                );

                inputMap.SetButton(action, button);
            }
        }

        return inputMap;
    }
}