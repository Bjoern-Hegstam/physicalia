using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework.Input;

namespace PhysicaliaRemastered.Input
{
    /// <summary>
    /// Maps the keys on a Keyboard to the actions available in Physicalia.
    /// </summary>
    class KeyboardInputMap : InputMap
    {
        #region Keys

        private Dictionary<InputAction, Keys> keys;

        #endregion

        #region Constructor

        public KeyboardInputMap()
        {
            this.keys = new Dictionary<InputAction, Keys>();
        }

        #endregion

        #region IInputMap Members

        public override bool IsPressed(InputAction action)
        {
            return this.InputHandler.IsPressed(this.keys[action]);
        }

        public override bool IsHolding(InputAction action)
        {
            return this.InputHandler.IsHolding(this.keys[action]);
        }

        public override bool IsReleased(InputAction action)
        {
            return this.InputHandler.IsReleased(this.keys[action]);
        }

        public override void SetButton(InputAction action, int button)
        {
            this.keys[action] = (Keys)button;
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

                        this.keys[action] = key;
                    }
                }
            }
        }

        #endregion
    }
}
