using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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
        XmlReaderSettings readerSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true
        };

        using XmlReader reader = XmlReader.Create(path, readerSettings);
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Button")
            {
                InputAction action = (InputAction)Enum.Parse(typeof(InputAction), reader.GetAttribute("action"));
                Buttons button = (Buttons)Enum.Parse(typeof(Buttons), reader.GetAttribute("value"));

                _buttons[action] = button;
            }
        }
    }
}