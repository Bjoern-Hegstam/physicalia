using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNALibrary.Interfaces;

namespace PhysicaliaRemastered.Weapons.NewWeapons;

public class MeleeWeapon : Weapon
{
    private Vector2 warmupVibration;
    private Vector2 fireVibration;

    public MeleeWeapon(int weaponID, IParticleEngine particleEngine)
        : base(weaponID, particleEngine)
    {
            warmupVibration = fireVibration = Vector2.Zero;

            InfiniteAmmo = true;

            // Make sure all ammunition fields are set no -1, indicating inifinite ammo
            MaxAmmo = AmmoCount = -1;
            StoreAmmoCount();
        }

    public override void Start()
    {
            // TODO: The vibration settings should be moved to xml and perhaps also up to the base class
            GamePad.SetVibration(PlayerIndex.One, warmupVibration.X, warmupVibration.Y);

            base.Start();
        }

    public override void Stop()
    {
            GamePad.SetVibration(PlayerIndex.One, 0F, 0F);

            base.Stop();
        }

    protected override void OnStartFire()
    {
            GamePad.SetVibration(PlayerIndex.One, fireVibration.X, fireVibration.Y);
        }

    protected override void FireWeapon()
    {
            
        }

    public override void LoadXml(XmlReader reader)
    {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Vibration")
                {
                    float low, high;

                    reader.ReadToFollowing("Warmup");
                    low = float.Parse(reader.GetAttribute("low"));
                    high = float.Parse(reader.GetAttribute("high"));

                    warmupVibration = new Vector2(low, high);

                    reader.ReadToFollowing("Fire");
                    low = float.Parse(reader.GetAttribute("low"));
                    high = float.Parse(reader.GetAttribute("high"));

                    fireVibration = new Vector2(low, high);
                }

                if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.LocalName == "WeaponData")
                    return;
            }
        }
}