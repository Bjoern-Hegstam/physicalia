using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNALibrary;

namespace PhysicaliaRemastered.Input;

internal class GamePadInputMap : InputMap
{
    private readonly Dictionary<InputAction, Buttons> _buttons;

    public GamePadInputMap()
    {
        _buttons = new Dictionary<InputAction, Buttons>();
    }

    public override bool IsPressed(InputAction action)
    {
        return InputHandler.IsPressed(PlayerIndex.One, _buttons[action]);
    }

    public override bool IsHolding(InputAction action)
    {
        return InputHandler.IsHolding(PlayerIndex.One, _buttons[action]);
    }

    public override bool IsReleased(InputAction action)
    {
        return InputHandler.IsReleased(PlayerIndex.One, _buttons[action]);
    }

    public override void SetButton(InputAction action, int button)
    {
        _buttons[action] = (Buttons)button;
    }

    public override void LoadXml(string path)
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
                var action = (InputAction)Enum.Parse(typeof(InputAction), reader.GetAttribute("action") ?? throw new ResourceLoadException());
                var button = (Buttons)Enum.Parse(typeof(Buttons), reader.GetAttribute("value") ?? throw new ResourceLoadException());

                _buttons[action] = button;
            }
        }
    }
}