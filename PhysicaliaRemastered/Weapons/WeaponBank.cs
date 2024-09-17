using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using PhysicaliaRemastered.Weapons.NewWeapons;
using XNALibrary;
using XNALibrary.Animation;
using XNALibrary.ParticleEngine;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.Weapons;

/// <summary>
/// Class for managing over a collection of weapons that serve as the base
/// for all weapons in the game.
/// </summary>
public class WeaponBank(IParticleEngine particleEngine, SpriteLibrary spriteLibrary, IAnimationManager animationManager)
{
    private readonly Dictionary<int, Weapon> _weaponBank = new();

    public Weapon GetWeapon(int weaponId)
    {
        return _weaponBank[weaponId];
    }

    public void LoadXml(string path)
    {
        var readerSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            IgnoreProcessingInstructions = true
        };

        using var reader = XmlReader.Create(path, readerSettings);
        LoadXml(reader);
    }

    public void LoadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Weapon" })
            {
                // Get the type of weapon to add
                string weaponType = reader.GetAttribute("type") ?? throw new ResourceLoadException();
                int weaponId = int.Parse(reader.GetAttribute("id") ?? throw new ResourceLoadException());

                Weapon weapon = weaponType switch
                {
                    "Melee" => new MeleeWeapon(weaponId, particleEngine),
                    "Projectile" => new ProjectileWeapon(weaponId, particleEngine),
                    _ => throw new InvalidGameStateException($"Unknown weapon type {weaponType}")
                };

                ParseWeaponData(reader, weapon);

                _weaponBank.Add(weapon.WeaponId, weapon);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "WeaponBank" })
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
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Graphics" })
            {
                // Get sprite
                reader.ReadToFollowing("Sprite");
                int spriteKey = int.Parse(reader.GetAttribute("key") ?? throw new ResourceLoadException());
                weapon.WeaponSprite = spriteLibrary.GetSprite(spriteKey);

                // Get animations
                reader.ReadToFollowing("Warmup");
                int warmUpKey = int.Parse(reader.GetAttribute("key") ?? throw new ResourceLoadException());
                weapon.WarmupAnimation = animationManager.AddPlaybackAnimation(warmUpKey);

                reader.ReadToFollowing("Fire");
                int fireKey = int.Parse(reader.GetAttribute("key") ?? throw new ResourceLoadException());
                weapon.WeaponFireAnimation = animationManager.AddPlaybackAnimation(fireKey);

                reader.ReadToFollowing("PlayerOffset");
                float x = float.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
                float y = float.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());
                weapon.PlayerOffset = new Vector2(x, y);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "FireData" })
            {
                // Parse fire data
                weapon.WeaponWarmUp = int.Parse(reader.GetAttribute("warmupTime") ?? throw new ResourceLoadException());
                weapon.ShotsPerSecond =
                    int.Parse(reader.GetAttribute("shotsPerSecond") ?? throw new ResourceLoadException());

                // Get particle id
                reader.ReadToFollowing("Particle");
                weapon.ParticleId = int.Parse(reader.GetAttribute("id") ?? throw new ResourceLoadException());
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Collision" })
            {
                weapon.CanCollide = bool.Parse(reader.GetAttribute("canCollide") ?? throw new ResourceLoadException());

                // Don't read anymore collision data if the weapon can't collide
                if (!weapon.CanCollide)
                {
                    continue;
                }

                weapon.CollisionDamage =
                    int.Parse(reader.GetAttribute("collisionDamage") ?? throw new ResourceLoadException());

                // Parse collision box
                reader.ReadToFollowing("CollisionBox");
                int x = int.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
                int y = int.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());
                int width = int.Parse(reader.GetAttribute("width") ?? throw new ResourceLoadException());
                int height = int.Parse(reader.GetAttribute("height") ?? throw new ResourceLoadException());

                weapon.CollisionBox = new Rectangle(x, y, width, height);
            }

            if (reader.NodeType == XmlNodeType.Element && reader is { LocalName: "WeaponData", IsEmptyElement: false })
            {
                weapon.LoadXml(reader);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Weapon" })
            {
                return;
            }
        }
    }
}