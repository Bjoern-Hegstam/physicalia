using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using PhysicaliaRemastered.Weapons.NewWeapons;
using XNALibrary.Animation;
using XNALibrary.ParticleEngine;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.Weapons;

public interface IWeaponBank
{
    void AddWeapon(Weapon weapon);
    void RemoveWeapon(int weaponId);
    Weapon GetWeapon(int weaponId);
}

/// <summary>
/// Class for mananing over a collection of weapons that serve as the base
/// for all weapons in the game.
/// </summary>
public class WeaponBank : IWeaponBank
{
    private readonly Dictionary<int, Weapon> _weaponBank;

    private readonly IParticleEngine _particleEngine;
    private readonly ISpriteLibrary _spriteLibrary;
    private readonly IAnimationManager _animationManager;

    public WeaponBank(IParticleEngine particleEngine, ISpriteLibrary spriteLibrary, IAnimationManager animationManager)
    {
        _particleEngine = particleEngine;
        _spriteLibrary = spriteLibrary;
        _animationManager = animationManager;

        _weaponBank = new Dictionary<int, Weapon>();
    }

    /// <summary>
    /// Adds the passed in Weapon to the bank.
    /// </summary>
    /// <param name="weapon">Weapon to add to the bank.</param>
    public void AddWeapon(Weapon weapon)
    {
        if (!_weaponBank.ContainsValue(weapon))
        {
            _weaponBank.Add(weapon.WeaponId, weapon);
        }
    }

    /// <summary>
    /// Removes the weapon with the specified id form the bank.
    /// </summary>
    /// <param name="weaponId">Id of the weapon to remove.</param>
    public void RemoveWeapon(int weaponId)
    {
        if (_weaponBank.ContainsKey(weaponId))
        {
            _weaponBank.Remove(weaponId);
        }
    }

    /// <summary>
    /// Gets the weapon with the specified id.
    /// </summary>
    /// <param name="weaponId">Id of the weapon to get.</param>
    /// <returns>The weapon with the specified id or null if no match
    /// was found.</returns>
    public Weapon GetWeapon(int weaponId)
    {
        if (_weaponBank.ContainsKey(weaponId))
        {
            return _weaponBank[weaponId];
        }

        return null;
    }

    public void LoadXml(string path)
    {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.IgnoreComments = true;
        readerSettings.IgnoreWhitespace = true;
        readerSettings.IgnoreProcessingInstructions = true;

        using XmlReader reader = XmlReader.Create(path, readerSettings);
        LoadXml(reader);
    }

    public void LoadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Weapon")
            {
                // Get the type of weapon to add
                string weaponType = reader.GetAttribute("type");
                int weaponId = int.Parse(reader.GetAttribute("id"));

                Weapon weapon = null;
                // Create an instance of the wanted type
                switch (weaponType)
                {
                    case "Melee":
                        weapon = new MeleeWeapon(weaponId, _particleEngine);
                        break;
                    case "Projectile":
                        weapon = new ProjectileWeapon(weaponId, _particleEngine);
                        break;
                }

                // Parse the weapon data
                ParseWeaponData(reader, weapon);

                // Add the weapon to the bank
                _weaponBank.Add(weapon.WeaponId, weapon);
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "WeaponBank")
            {
                return;
            }
        }
    }

    private void ParseWeaponData(XmlReader reader, Weapon weapon)
    {
        // Parse xml data
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Graphics")
            {
                // Get sprite
                reader.ReadToFollowing("Sprite");
                int spriteKey = int.Parse(reader.GetAttribute("key"));
                weapon.WeaponSprite = _spriteLibrary.GetSprite(spriteKey);

                // Get animations
                reader.ReadToFollowing("Warmup");
                int warmUpKey = int.Parse(reader.GetAttribute("key"));
                weapon.WarmupAnimation = _animationManager.AddPlaybackAnimation(warmUpKey);

                reader.ReadToFollowing("Fire");
                int fireKey = int.Parse(reader.GetAttribute("key"));
                weapon.WeaponFireAnimation = _animationManager.AddPlaybackAnimation(fireKey);

                reader.ReadToFollowing("PlayerOffset");
                float x = float.Parse(reader.GetAttribute("x"));
                float y = float.Parse(reader.GetAttribute("y"));
                weapon.PlayerOffset = new Vector2(x, y);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "FireData")
            {
                // Parse fire data
                weapon.WeaponWarmUp = int.Parse(reader.GetAttribute("warmupTime"));
                weapon.ShotsPerSecond = int.Parse(reader.GetAttribute("shotsPerSecond"));

                // Get particle id
                reader.ReadToFollowing("Particle");
                weapon.ParticleId = int.Parse(reader.GetAttribute("id"));
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Collision")
            {
                weapon.CanCollide = bool.Parse(reader.GetAttribute("canCollide"));

                // Don't read anymore collision data if the weapon can't collide
                if (!weapon.CanCollide)
                {
                    continue;
                }

                weapon.CollisionDamage = int.Parse(reader.GetAttribute("collisionDamage"));

                // Parse collision box
                reader.ReadToFollowing("CollisionBox");
                int x = int.Parse(reader.GetAttribute("x"));
                int y = int.Parse(reader.GetAttribute("y"));
                int width = int.Parse(reader.GetAttribute("width"));
                int height = int.Parse(reader.GetAttribute("height"));

                weapon.CollisionBox = new Rectangle(x, y, width, height);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "WeaponData")
            {
                if (!reader.IsEmptyElement)
                {
                    weapon.LoadXml(reader);
                }
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "Weapon")
            {
                return;
            }
        }
    }
}