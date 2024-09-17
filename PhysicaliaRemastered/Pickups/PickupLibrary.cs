using System.Collections.Generic;
using System.Xml;
using PhysicaliaRemastered.Pickups.Modifiers;
using XNALibrary;
using XNALibrary.Sprites;

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
    private readonly Dictionary<int, Pickup> _modifierLib;

    public PickupLibrary()
    {
        _modifierLib = new Dictionary<int, Pickup>();
    }

    public void AddPickup(int key, Pickup modifier)
    {
        _modifierLib[key] = modifier;
    }

    public void RemovePickup(int key)
    {
        if (_modifierLib.ContainsKey(key))
        {
            _modifierLib.Remove(key);
        }
    }

    public Pickup GetPickup(int key)
    {
        if (_modifierLib.ContainsKey(key))
        {
            return _modifierLib[key].Copy();
        }

        return null;
    }

    public void LoadXml(string path, SpriteLibrary spriteLibrary)
    {
        var readerSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true
        };

        using var reader = XmlReader.Create(path, readerSettings);
        LoadXml(reader, spriteLibrary);
    }

    public void LoadXml(XmlReader reader, SpriteLibrary spriteLibrary)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Pickup" })
            {
                string type = reader.GetAttribute("type");
                int key = int.Parse(reader.GetAttribute("key") ?? throw new ResourceLoadException());

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
                    pickup.Id = key;
                    _modifierLib.Add(key, pickup);
                }
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Pickups" })
            {
                return;
            }
        }
    }
}