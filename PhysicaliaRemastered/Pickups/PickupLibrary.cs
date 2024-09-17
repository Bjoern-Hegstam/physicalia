using System.Collections.Generic;
using System.Xml;
using PhysicaliaRemastered.Pickups.Modifiers;
using XNALibrary;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.Pickups;

public class PickupLibrary
{
    private readonly Dictionary<int, Pickup> _modifierLib = new();

    public Pickup GetPickup(int key)
    {
        if (_modifierLib.TryGetValue(key, out Pickup? value))
        {
            return value.Copy();
        }

        throw new MissingPickupException();
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
                string type = reader.GetAttribute("type") ?? throw new ResourceLoadException();
                int key = int.Parse(reader.GetAttribute("key") ?? throw new ResourceLoadException());

                Pickup pickup = type switch
                {
                    "GravityReverser" => GravityReverser.CreateFromXml(reader, spriteLibrary),
                    "HealthPickup" => HealthPickup.CreateFromXml(reader, spriteLibrary),
                    _ => throw new InvalidGameStateException($"Unknown pickup type {type}")
                };

                pickup.Id = key;
                _modifierLib.Add(key, pickup);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Pickups" })
            {
                return;
            }
        }
    }
}