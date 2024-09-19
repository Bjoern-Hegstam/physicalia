using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.GameManagement;
using PhysicaliaRemastered.Pickups;
using PhysicaliaRemastered.Weapons.NewWeapons;

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
    private readonly Weapon _weapon;

    public WeaponPickup(Level level, Weapon weapon)
        : base(level)
    {
        _weapon = weapon;
    }

    public override void OnPickedUp()
    {
        if (!PickedUp)
        {
            PickedUp = true;
            Weapon weaponCopy = _weapon.Copy();
            Level.Player.AddWeapon(weaponCopy);
            weaponCopy.Player = Level.Player;
        }
    }

    public override void Reset()
    {
        // Remove the weapon from the player if we've been picked up
        if (PickedUp)
        {
            Level.Player.RemoveWeapon(_weapon.WeaponId, _weapon.AmmoCount);
        }

        base.Reset();
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 viewportPosition)
    {
        spriteBatch.Draw(
            _weapon.WeaponSprite?.Texture,
            viewportPosition,
            _weapon.WeaponSprite?.SourceRectangle,
            Color.White
        );
    }
}