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
    private Dictionary<InputAction, Keys> keys;

    public KeyboardInputMap()
    {
            keys = new Dictionary<InputAction, Keys>();
        }

    public override bool IsPressed(InputAction action)
    {
            return InputHandler.IsPressed(keys[action]);
        }

    public override bool IsHolding(InputAction action)
    {
            return InputHandler.IsHolding(keys[action]);
        }

    public override bool IsReleased(InputAction action)
    {
            return InputHandler.IsReleased(keys[action]);
        }

    public override void SetButton(InputAction action, int button)
    {
            keys[action] = (Keys)button;
        }

    public override void LoadXml(string path)
    {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            readerSettings.IgnoreWhitespace = true;
            readerSettings.IgnoreProcessingInstructions = true;
            
            using (XmlReader reader = XmlReader.Create(path, readerSettings))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element &&
                        reader.LocalName == "Key")
                    {
                        InputAction action = (InputAction)Enum.Parse(typeof(InputAction), reader.GetAttribute("action"));
                        Keys key = (Keys)Enum.Parse(typeof(Keys), reader.GetAttribute("value"));

                        keys[action] = key;
                    }
                }
            }
        }
}