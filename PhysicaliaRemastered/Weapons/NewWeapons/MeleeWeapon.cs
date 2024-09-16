using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace PhysicaliaRemastered.Weapons.NewWeapons;

public class MeleeWeapon : Weapon
{
    #region Fields

    private Vector2 warmupVibration;
    private Vector2 fireVibration;

    #endregion

    #region Constructor

    public MeleeWeapon(int weaponID, IParticleEngine particleEngine)
        : base(weaponID, particleEngine)
    {
            this.warmupVibration = this.fireVibration = Vector2.Zero;

            this.InfiniteAmmo = true;

            // Make sure all ammunition fields are set no -1, indicating inifinite ammo
            this.MaxAmmo = this.AmmoCount = -1;
            this.StoreAmmoCount();
        }

    #endregion

    #region Weapon members

    public override void Start()
    {
            // TODO: The vibration settings should be moved to xml and perhaps also up to the base class
            GamePad.SetVibration(PlayerIndex.One, this.warmupVibration.X, this.warmupVibration.Y);

            base.Start();
        }

    public override void Stop()
    {
            GamePad.SetVibration(PlayerIndex.One, 0F, 0F);

            base.Stop();
        }

    protected override void OnStartFire()
    {
            GamePad.SetVibration(PlayerIndex.One, this.fireVibration.X, this.fireVibration.Y);
        }

    protected override void FireWeapon()
    {
            
        }

    public override void LoadXml(XmlReader reader)
    {
            while (reader.Read())
            {
                if (reader.NodeType == System.Xml.XmlNodeType.Element &&
                    reader.LocalName == "Vibration")
                {
                    float low, high;

                    reader.ReadToFollowing("Warmup");
                    low = float.Parse(reader.GetAttribute("low"));
                    high = float.Parse(reader.GetAttribute("high"));

                    this.warmupVibration = new Vector2(low, high);

                    reader.ReadToFollowing("Fire");
                    low = float.Parse(reader.GetAttribute("low"));
                    high = float.Parse(reader.GetAttribute("high"));

                    this.fireVibration = new Vector2(low, high);
                }

                if (reader.NodeType == System.Xml.XmlNodeType.EndElement &&
                    reader.LocalName == "WeaponData")
                    return;
            }
        }

    #endregion
}