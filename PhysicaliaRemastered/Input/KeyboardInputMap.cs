using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework.Input;
using XNALibrary;
using XNALibrary.Input;

namespace PhysicaliaRemastered.Input;

/// <summary>
/// Maps the keys on a Keyboard to the actions available in Physicalia.
/// </summary>
internal class KeyboardInputMap(InputHandler inputHandler) : InputMap
{
    private readonly Dictionary<InputAction, Keys> _keys = new();

    public override bool IsPressed(InputAction action)
    {
        return inputHandler.IsPressed(_keys[action]);
    }

    public override bool IsHolding(InputAction action)
    {
        return inputHandler.IsHolding(_keys[action]);
    }

    public override bool IsReleased(InputAction action)
    {
        return inputHandler.IsReleased(_keys[action]);
    }

    public virtual void SetButton(InputAction action, int button)
    {
        _keys[action] = (Keys)button;
    }

    public virtual void LoadXml(string path)
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

                _keys[action] = key;
            }
        }
    }
}