using System.Xml;

namespace PhysicaliaRemastered.Pickups.Modifiers;

public class GravityReverser : ModifierPickup
{
    public GravityReverser(Level level, Sprite icon, Sprite sprite, float duration)
        : base(level, icon, sprite, duration) { }

    public override void Activate()
    {
            this.Level.Player.Acceleration *= -1;
        }

    public override void Deactivate()
    {
            this.Level.Player.Acceleration *= -1;
        }

    public static GravityReverser CreateFromXml(XmlReader reader, ISpriteLibrary spriteLibrary)
    {
            reader.ReadToFollowing("Icon");
            int iconKey = int.Parse(reader.GetAttribute("key"));

            reader.ReadToFollowing("Sprite");
            int spriteKey = int.Parse(reader.GetAttribute("key"));

            reader.ReadToFollowing("Duration");
            int duration = int.Parse(reader.GetAttribute("value"));

            return new GravityReverser(null, spriteLibrary.GetSprite(iconKey), spriteLibrary.GetSprite(spriteKey), duration);
        }
}