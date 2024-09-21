using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNALibrary;
using XNALibrary.Input;

namespace PhysicaliaRemastered.Input;

internal class GamePadInputMap(InputHandler inputHandler) : InputMap
{
    private readonly Dictionary<InputAction, Buttons> _buttons = new();
    
    public override bool IsPressed(InputAction action)
    {
        return inputHandler.IsPressed(PlayerIndex.One, _buttons[action]);
    }

    public override bool IsHolding(InputAction action)
    {
        return inputHandler.IsHolding(PlayerIndex.One, _buttons[action]);
    }

    public override bool IsReleased(InputAction action)
    {
        return inputHandler.IsReleased(PlayerIndex.One, _buttons[action]);
    }

    public virtual void SetButton(InputAction action, int button)
    {
        _buttons[action] = (Buttons)button;
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

                _buttons[action] = button;
            }
        }
    }
}