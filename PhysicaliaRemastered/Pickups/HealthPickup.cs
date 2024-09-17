using System.Xml;
using PhysicaliaRemastered.GameManagement;
using XNALibrary.Graphics.Sprites;
using XNALibrary.Interfaces;

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

    public static HealthPickup CreateFromXml(XmlReader reader, ISpriteLibrary spriteLibrary)
    {
        reader.ReadToFollowing("Sprite");
        int spriteKey = int.Parse(reader.GetAttribute("key"));

        reader.ReadToFollowing("Health");
        int health = int.Parse(reader.GetAttribute("value"));

        return new HealthPickup(null, health, spriteLibrary.GetSprite(spriteKey));
    }
}