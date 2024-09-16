using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.GameManagement;
using PhysicaliaRemastered.Pickups;

namespace PhysicaliaRemastered.Weapons;

/// <summary>
/// Class to be used for enabling easy management over weapon pickup.
/// The WeaponPickup contains a reference to the weapon that can be picked up.
/// When the Pickup.DoPickup method is called a copy of the Weapon is given
/// to the player. This means that the base weapon with the original settings
/// will always be kept intact. When Pickup.Reset is called the copy merely taken
/// from the player.
/// </summary>
public class WeaponPickup : Pickup
{
    private NewWeapons.Weapon weapon;

    public WeaponPickup(Level level, NewWeapons.Weapon weapon)
        : base(level)
    {
            this.weapon = weapon;
        }

    public override void DoPickup()
    {
            if (!PickedUp)
            {
                PickedUp = true;
                NewWeapons.Weapon weaponCopy = weapon.Copy();
                Level.Player.AddWeapon(weaponCopy);
                weaponCopy.Player = Level.Player;
            }
        }

    public override void Reset()
    {
            // Remove the weapon from the player if we've been picked up
            if (PickedUp)
                Level.Player.RemoveWeapon(weapon.WeaponID, weapon.AmmoCount);

            base.Reset();
        }

    public override void Draw(SpriteBatch spriteBatch, Vector2 positionOffset)
    {
            spriteBatch.Draw(weapon.WeaponSprite.Texture,
                             positionOffset,
                             weapon.WeaponSprite.SourceRectangle,
                             Color.White);
        }
}