using System.Collections.Generic;
using System.Xml;
using PhysicaliaRemastered.Pickups.Modifiers;

namespace PhysicaliaRemastered.Pickups;

public interface IPickupLibrary
{
    void AddPickup(int key, Pickup modifier);
    void RemovePickup(int key);
    Pickup GetPickup(int key);

    void LoadXml(string path, SpriteLibrary spriteLibrary);
    void LoadXml(XmlReader reader, SpriteLibrary spriteLibrary);
}

public class PickupLibrary : IPickupLibrary
{
    #region Fields

    private Dictionary<int, Pickup> modifierLib;

    #endregion

    #region Constructors

    public PickupLibrary()
    {
            this.modifierLib = new Dictionary<int, Pickup>();
        }

    #endregion

    #region Library access

    public void AddPickup(int key, Pickup modifier)
    {
            this.modifierLib[key] = modifier;
        }

    public void RemovePickup(int key)
    {
            if (this.modifierLib.ContainsKey(key))
                this.modifierLib.Remove(key);
        }

    public Pickup GetPickup(int key)
    {
            if (this.modifierLib.ContainsKey(key))
                return this.modifierLib[key].Copy();

            return null;
        }

    #endregion

    #region Xml loading

    public void LoadXml(string path, SpriteLibrary spriteLibrary)
    {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            readerSettings.IgnoreWhitespace = true;
            readerSettings.IgnoreProcessingInstructions = true;

            using (XmlReader reader = XmlReader.Create(path, readerSettings))
            {
                this.LoadXml(reader, spriteLibrary);
            }
        }

    public void LoadXml(XmlReader reader, SpriteLibrary spriteLibrary)
    {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Pickup")
                {
                    string type = reader.GetAttribute("type");
                    int key = int.Parse(reader.GetAttribute("key"));

                    Pickup pickup = null;
                    switch (type)
                    {
                        case "GravityReverser":
                            pickup = GravityReverser.CreateFromXml(reader, spriteLibrary);
                            break;
                        case "HealthPickup":
                            pickup = HealthPickup.CreateFromXml(reader, spriteLibrary);
                            break;
                    }

                    if (pickup != null)
                    {
                        pickup.ID = key;
                        this.modifierLib.Add(key, pickup);
                    }
                }

                if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.LocalName == "Pickups")
                    return;
            }
        }

    #endregion
}