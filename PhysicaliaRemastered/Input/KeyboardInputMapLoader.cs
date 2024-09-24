using System;
using System.Xml;
using Microsoft.Xna.Framework.Input;
using XNALibrary;
using XNALibrary.Input;

namespace PhysicaliaRemastered.Input;

public static class KeyboardInputMapLoader
{
    public static KeyboardInputMap Load(string path, InputHandler inputHandler)
    {
        var inputMap = new KeyboardInputMap(inputHandler);
        
        var readerSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true
        };

        using var reader = XmlReader.Create(path, readerSettings);
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Key" })
            {
                var action = (InputAction)Enum.Parse(
                    typeof(InputAction),
                    reader.GetAttribute("action") ?? throw new ResourceLoadException()
                );
                var key = (Keys)Enum.Parse(
                    typeof(Keys),
                    reader.GetAttribute("value") ?? throw new ResourceLoadException()
                );

                inputMap.SetButton(action, key);
            }
        }

        return inputMap;
    }
}