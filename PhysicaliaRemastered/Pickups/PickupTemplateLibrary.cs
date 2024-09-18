using System.Collections.Generic;
using System.Xml;
using PhysicaliaRemastered.Pickups.Modifiers;
using XNALibrary;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.Pickups;

public class PickupTemplateLibrary
{
    private readonly Dictionary<PickupTemplateId, Pickup> _pickupTemplates = new();

    public Pickup CreatePickup(PickupTemplateId templateId)
    {
        return _pickupTemplates[templateId].Copy();
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
                int id = int.Parse(reader.GetAttribute("key") ?? throw new ResourceLoadException());

                Pickup pickupTemplate = type switch
                {
                    "GravityReverser" => GravityReverser.CreateFromXml(reader, spriteLibrary),
                    "HealthPickup" => HealthPickup.CreateFromXml(reader, spriteLibrary),
                    _ => throw new InvalidGameStateException($"Unknown pickup type {type}")
                };

                var templateId = new PickupTemplateId(id);
                pickupTemplate.TemplateId = templateId;
                _pickupTemplates.Add(templateId, pickupTemplate);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Pickups" })
            {
                return;
            }
        }
    }
}