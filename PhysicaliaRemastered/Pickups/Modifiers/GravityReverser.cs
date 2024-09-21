using System.Xml;
using PhysicaliaRemastered.GameManagement;
using XNALibrary;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.Pickups.Modifiers;

public class GravityReverser : ModifierPickup
{
    public GravityReverser(Level level, Sprite icon, Sprite sprite, float duration)
        : base(level, icon, sprite, duration)
    {
    }

    public override void Activate()
    {
        Level.Player.Acceleration *= -1;
    }

    public override void Deactivate()
    {
        Level.Player.Acceleration *= -1;
    }

    public static GravityReverser CreateFromXml(XmlReader reader, SpriteLibrary spriteLibrary)
    {
        reader.ReadToFollowing("Icon");
        SpriteId iconSpriteId =
            new SpriteId(reader.GetAttribute("spriteId") ?? throw new ResourceLoadException());

        reader.ReadToFollowing("Sprite");
        SpriteId spriteId = new SpriteId(reader.GetAttribute("id") ?? throw new ResourceLoadException());

        reader.ReadToFollowing("Duration");
        int duration = int.Parse(reader.GetAttribute("value") ?? throw new ResourceLoadException());

        return new GravityReverser(null, spriteLibrary.GetSprite(iconSpriteId), spriteLibrary.GetSprite(spriteId),
            duration);
    }
}