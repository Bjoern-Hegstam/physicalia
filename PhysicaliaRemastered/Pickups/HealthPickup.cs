using System.Xml;

namespace PhysicaliaRemastered.Pickups;

public class HealthPickup : Pickup
{
    #region Fields

    private float health;

    #endregion

    #region Constructor

    public HealthPickup(Level level, float health, Sprite sprite)
        : base(level)
    {
            this.health = health;
            this.Sprite = sprite;
        }

    #endregion

    #region Pickup members

    public override void DoPickup()
    {
            this.Level.Player.Health += this.health;
        }

    public static HealthPickup CreateFromXml(XmlReader reader, ISpriteLibrary spriteLibrary)
    {
            reader.ReadToFollowing("Sprite");
            int spriteKey = int.Parse(reader.GetAttribute("key"));

            reader.ReadToFollowing("Health");
            int health = int.Parse(reader.GetAttribute("value"));

            return new HealthPickup(null, health, spriteLibrary.GetSprite(spriteKey));
        }

    #endregion
}