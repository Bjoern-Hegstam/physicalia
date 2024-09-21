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

    public override void OnPickedUp()
    {
        Level.Player.Health += _health;
    }

    public static HealthPickup CreateFromXml(XmlReader reader, SpriteLibrary spriteLibrary)
    {
        reader.ReadToFollowing("Sprite");
        var spriteId = new SpriteId(reader.GetAttribute("key") ?? throw new ResourceLoadException());

        reader.ReadToFollowing("Health");
        int health = int.Parse(reader.GetAttribute("value") ?? throw new ResourceLoadException());

        return new HealthPickup(null, health, spriteLibrary.GetSprite(spriteId));
    }
}