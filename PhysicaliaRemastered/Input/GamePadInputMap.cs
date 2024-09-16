using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace PhysicaliaRemastered.Input
{
    class GamePadInputMap : InputMap
    {
        private Dictionary<InputAction, Buttons> buttons;

        public GamePadInputMap()
        {
            this.buttons = new Dictionary<InputAction, Buttons>();
        }

        #region InputMap Members

        public override bool IsPressed(InputAction action)
        {
            return this.InputHandler.IsPressed(PlayerIndex.One, this.buttons[action]);
        }

        public override bool IsHolding(InputAction action)
        {
            return this.InputHandler.IsHolding(PlayerIndex.One, this.buttons[action]);
        }

        public override bool IsReleased(InputAction action)
        {
            return this.InputHandler.IsReleased(PlayerIndex.One, this.buttons[action]);
        }

        public override void SetButton(InputAction action, int button)
        {
            this.buttons[action] = (Buttons)button;
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
                        reader.LocalName == "Button")
                    {
                        InputAction action = (InputAction)Enum.Parse(typeof(InputAction), reader.GetAttribute("action"));
                        Buttons button = (Buttons)Enum.Parse(typeof(Buttons), reader.GetAttribute("value"));

                        this.buttons[action] = button;
                    }
                }
            }
        }

        #endregion
    }
}
