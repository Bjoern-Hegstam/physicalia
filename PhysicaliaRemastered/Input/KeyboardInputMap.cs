using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework.Input;

namespace PhysicaliaRemastered.Input;

/// <summary>
/// Maps the keys on a Keyboard to the actions available in Physicalia.
/// </summary>
internal class KeyboardInputMap : InputMap
{
    private readonly Dictionary<InputAction, Keys> _keys;

    public KeyboardInputMap()
    {
        _keys = new Dictionary<InputAction, Keys>();
    }

    public override bool IsPressed(InputAction action)
    {
        return InputHandler.IsPressed(_keys[action]);
    }

    public override bool IsHolding(InputAction action)
    {
        return InputHandler.IsHolding(_keys[action]);
    }

    public override bool IsReleased(InputAction action)
    {
        return InputHandler.IsReleased(_keys[action]);
    }

    public override void SetButton(InputAction action, int button)
    {
        _keys[action] = (Keys)button;
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
                reader.LocalName == "Key")
            {
                InputAction action = (InputAction)Enum.Parse(typeof(InputAction), reader.GetAttribute("action"));
                Keys key = (Keys)Enum.Parse(typeof(Keys), reader.GetAttribute("value"));

                _keys[action] = key;
            }
        }
    }
}