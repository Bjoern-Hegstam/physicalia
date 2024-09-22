using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using PhysicaliaRemastered.Actors;
using XNALibrary;

namespace PhysicaliaRemastered.GameManagement;

public record struct ModifierSave(int Id, float TimeLeft);

public record struct ActiveObjectSave(Vector2 Position, bool IsActive);

public record struct EnemySave(Vector2 Position, Vector2 Velocity, float Health, bool IsActive);

public record struct WeaponSave(int AmmoCount, int StoredAmmo);

public class SaveGame
{
    private ActorStartValues _playerValues;

    public int WorldIndex { get; set; }

    public int LevelIndex { get; set; }

    public ActorStartValues PlayerValues
    {
        get => _playerValues;
        set => _playerValues = value;
    }

    public float PlayerHealth { get; set; }

    public int SelectedWeapon { get; set; }

    public Dictionary<int, WeaponSave> WeaponSaves { get; } = new();

    public List<ModifierSave> LevelModifiers { get; } = [];

    public Dictionary<int, ActiveObjectSave> ActivatedObjects { get; } = new();

    public Dictionary<int, EnemySave> SavedEnemies { get; } = new();

    public void SaveToXml(string path)
    {
        var writerSettings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8
        };

        using var writer = XmlWriter.Create(path, writerSettings);
        writer.WriteStartDocument();

        writer.WriteStartElement("SaveGame");

        writer.WriteStartElement("WorldIndex");
        writer.WriteAttributeString("value", WorldIndex.ToString());
        writer.WriteEndElement();

        writer.WriteStartElement("LevelIndex");
        writer.WriteAttributeString("value", LevelIndex.ToString());
        writer.WriteEndElement();

        writer.WriteStartElement("Player");
        writer.WriteAttributeString("health", PlayerHealth.ToString());

        // Player values
        writer.WriteStartElement("PlayerValues");
        WriteVector2(writer, "Position", _playerValues.Position);
        WriteVector2(writer, "Velocity", _playerValues.Velocity);
        WriteVector2(writer, "Acceleration", _playerValues.Acceleration);
        writer.WriteEndElement();

        writer.WriteStartElement("Weapons");
        writer.WriteAttributeString("selected", SelectedWeapon.ToString());

        foreach (int weaponId in WeaponSaves.Keys)
        {
            WeaponSave weaponSave = WeaponSaves[weaponId];

            writer.WriteStartElement("Weapon");
            writer.WriteAttributeString("id", weaponId.ToString());
            writer.WriteAttributeString("ammoCount", weaponSave.AmmoCount.ToString());
            writer.WriteAttributeString("storedAmmo", weaponSave.StoredAmmo.ToString());
            writer.WriteEndElement();
        }

        // End of weapons
        writer.WriteEndElement();

        // End of player
        writer.WriteEndElement();

        // Modifiers
        WriteModifiers(writer);

        // Activated objects
        WriteActiveObjects(writer);

        // Activated enemies
        WriteEnemies(writer);

        writer.WriteEndDocument();
    }

    private void WriteVector2(XmlWriter writer, string elementName, Vector2 value)
    {
        writer.WriteStartElement(elementName);
        writer.WriteAttributeString("x", value.X.ToString());
        writer.WriteAttributeString("y", value.Y.ToString());
        writer.WriteEndElement();
    }

    private void WriteModifiers(XmlWriter writer)
    {
        writer.WriteStartElement("Modifiers");

        foreach (ModifierSave modifier in LevelModifiers)
        {
            writer.WriteStartElement("Modifier");
            writer.WriteAttributeString("id", modifier.Id.ToString());
            writer.WriteAttributeString("timeLeft", modifier.TimeLeft.ToString());
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private void WriteActiveObjects(XmlWriter writer)
    {
        writer.WriteStartElement("ActiveObjects");

        foreach (int key in ActivatedObjects.Keys)
        {
            ActiveObjectSave save = ActivatedObjects[key];

            writer.WriteStartElement("ActiveObject");
            writer.WriteAttributeString("key", key.ToString());
            writer.WriteAttributeString("active", save.IsActive.ToString());

            WriteVector2(writer, "Position", save.Position);

            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private void WriteEnemies(XmlWriter writer)
    {
        writer.WriteStartElement("Enemies");

        foreach (int key in SavedEnemies.Keys)
        {
            EnemySave save = SavedEnemies[key];

            writer.WriteStartElement("Enemy");
            writer.WriteAttributeString("key", key.ToString());
            writer.WriteAttributeString("health", save.Health.ToString());
            writer.WriteAttributeString("active", save.IsActive.ToString());

            WriteVector2(writer, "Position", save.Position);
            WriteVector2(writer, "Velocity", save.Velocity);

            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    public static SaveGame LoadFromXml(string path)
    {
        var saveGame = new SaveGame();

        var readerSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true
        };

        using var reader = XmlReader.Create(path, readerSettings);
        reader.ReadToFollowing("WorldIndex");
        saveGame.WorldIndex = int.Parse(reader.GetAttribute("value") ?? throw new ResourceLoadException());

        reader.ReadToFollowing("LevelIndex");
        saveGame.LevelIndex = int.Parse(reader.GetAttribute("value") ?? throw new ResourceLoadException());

        reader.ReadToFollowing("Player");
        saveGame.PlayerHealth = float.Parse(reader.GetAttribute("health") ?? throw new ResourceLoadException());

        saveGame._playerValues = ActorStartValues.FromXml(reader, "PlayerValues");

        LoadWeapons(reader, saveGame);
        LoadModifiers(reader, saveGame);
        LoadActiveObjects(reader, saveGame);
        LoadEnemies(reader, saveGame);

        return saveGame;
    }

    private static void LoadWeapons(XmlReader reader, SaveGame saveGame)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Weapons" })
            {
                if (reader.IsEmptyElement)
                {
                    return;
                }

                saveGame.SelectedWeapon =
                    int.Parse(reader.GetAttribute("selected") ?? throw new ResourceLoadException());
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Weapon" })
            {
                int id = int.Parse(reader.GetAttribute("id") ?? throw new ResourceLoadException());
                int ammoCount = int.Parse(reader.GetAttribute("ammoCount") ?? throw new ResourceLoadException());
                int storedAmmo = int.Parse(reader.GetAttribute("storedAmmo") ?? throw new ResourceLoadException());

                saveGame.WeaponSaves.Add(id, new WeaponSave(ammoCount, storedAmmo));
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Weapons" })
            {
                break;
            }
        }
    }

    private static void LoadModifiers(XmlReader reader, SaveGame saveGame)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader is { LocalName: "Modifiers", IsEmptyElement: true })
            {
                return;
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Modifier" })
            {
                int id = int.Parse(reader.GetAttribute("id") ?? throw new ResourceLoadException());
                float timeLeft = float.Parse(reader.GetAttribute("timeLeft") ?? throw new ResourceLoadException());

                saveGame.LevelModifiers.Add(new ModifierSave(id, timeLeft));
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Modifiers" })
            {
                return;
            }
        }
    }

    private static void LoadActiveObjects(XmlReader reader, SaveGame saveGame)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader is { LocalName: "ActiveObjects", IsEmptyElement: true })
            {
                return;
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "ActiveObject" })
            {
                int key = int.Parse(reader.GetAttribute("key") ?? throw new ResourceLoadException());
                bool active = bool.Parse(reader.GetAttribute("active") ?? throw new ResourceLoadException());

                reader.ReadToFollowing("Position");
                float x = float.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
                float y = float.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());
                var position = new Vector2(x, y);

                saveGame.ActivatedObjects.Add(key, new ActiveObjectSave(position, active));
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "ActiveObjects" })
            {
                return;
            }
        }
    }

    private static void LoadEnemies(XmlReader reader, SaveGame saveGame)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader is { LocalName: "Enemies", IsEmptyElement: true })
            {
                return;
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Enemy" })
            {
                int key = int.Parse(reader.GetAttribute("key") ?? throw new ResourceLoadException());
                float health = float.Parse(reader.GetAttribute("health") ?? throw new ResourceLoadException());
                bool active = bool.Parse(reader.GetAttribute("active") ?? throw new ResourceLoadException());

                reader.ReadToFollowing("Position");
                float posX = float.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
                float posY = float.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());
                var position = new Vector2(posX, posY);

                reader.ReadToFollowing("Velocity");
                float velX = float.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
                float velY = float.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());
                var velocity = new Vector2(velX, velY);

                saveGame.SavedEnemies.Add(key, new EnemySave(position, velocity, health, active));
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Enemies" })
            {
                return;
            }
        }
    }
}