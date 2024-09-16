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
    private Dictionary<int, Pickup> modifierLib;

    public PickupLibrary()
    {
            modifierLib = new Dictionary<int, Pickup>();
        }

    public void AddPickup(int key, Pickup modifier)
    {
            modifierLib[key] = modifier;
        }

    public void RemovePickup(int key)
    {
            if (modifierLib.ContainsKey(key))
                modifierLib.Remove(key);
        }

    public Pickup GetPickup(int key)
    {
            if (modifierLib.ContainsKey(key))
                return modifierLib[key].Copy();

            return null;
        }

    public void LoadXml(string path, SpriteLibrary spriteLibrary)
    {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            readerSettings.IgnoreWhitespace = true;
            readerSettings.IgnoreProcessingInstructions = true;

            using (XmlReader reader = XmlReader.Create(path, readerSettings))
            {
                LoadXml(reader, spriteLibrary);
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
                        modifierLib.Add(key, pickup);
                    }
                }

                if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.LocalName == "Pickups")
                    return;
            }
        }
}