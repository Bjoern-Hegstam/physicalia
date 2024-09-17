using System.Xml;
using PhysicaliaRemastered.GameManagement;
using XNALibrary;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.Pickups;

public class HealthPickup : Pickup
{
    private readonly float _health;

    public HealthPickup(Level level, float health, Sprite sprite)
        : base(level)
    {
        _health = health;
        Sprite = sprite;
    }

    public override void DoPickup()
    {
        Level.Player.Health += _health;
    }

    public static HealthPickup CreateFromXml(XmlReader reader, SpriteLibrary spriteLibrary)
    {
        reader.ReadToFollowing("Sprite");
        int spriteKey = int.Parse(reader.GetAttribute("key") ?? throw new ResourceLoadException());

        reader.ReadToFollowing("Health");
        int health = int.Parse(reader.GetAttribute("value") ?? throw new ResourceLoadException());

        return new HealthPickup(null, health, spriteLibrary.GetSprite(spriteKey));
    }
}