using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using PhysicaliaRemastered.Actors;
using XNALibrary;

namespace PhysicaliaRemastered.GameManagement;

public struct ModifierSave
{
    public int Id;
    public float TimeLeft;

    public ModifierSave(int id, float timeLeft)
    {
        Id = id;
        TimeLeft = timeLeft;
    }
}

public struct ActiveObjectSave
{
    public Vector2 Position;
    public bool IsActive;

    public ActiveObjectSave(Vector2 position, bool active)
    {
        Position = position;
        IsActive = active;
    }
}

public struct EnemySave
{
    public Vector2 Position;
    public Vector2 Velocity;

    public float Health;
    public bool IsActive;

    public EnemySave(Vector2 position, Vector2 velocity, float health, bool active)
    {
        Position = position;
        Velocity = velocity;
        Health = health;
        IsActive = active;
    }
}

public struct WeaponSave
{
    public int AmmoCount;
    public int StoredAmmo;

    public WeaponSave(int ammoCount, int storedAmmo)
    {
        AmmoCount = ammoCount;
        StoredAmmo = storedAmmo;
    }
}

/// <summary>
/// A GameSession represents a state that the game can be in. A GameSession
/// object can be de-/serialized from/to xml. GameSessions can be used to
/// implement a system where games can be saved and later loaded.
/// </summary>
public class GameSession
{
    // Player
    private ActorStartValues _playerValues;

    // Level

    // EnemyManager

    public int WorldIndex { get; set; }

    public int LevelIndex { get; set; }

    public ActorStartValues PlayerValues
    {
        get => _playerValues;
        set => _playerValues = value;
    }

    public float PlayerHealth { get; set; }

    public int SelectedWeapon { get; set; }

    public Dictionary<int, WeaponSave> WeaponSaves { get; }

    public List<ModifierSave> LevelModifiers { get; }

    public Dictionary<int, ActiveObjectSave> ActivatedObjects { get; }

    public Dictionary<int, EnemySave> SavedEnemies { get; }

    public GameSession()
    {
        WeaponSaves = new Dictionary<int, WeaponSave>();
        LevelModifiers = [];
        ActivatedObjects = new Dictionary<int, ActiveObjectSave>();
        SavedEnemies = new Dictionary<int, EnemySave>();
    }

    public void SaveToXml(string path)
    {
        var writerSettings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8
        };

        using var writer = XmlWriter.Create(path, writerSettings);
        writer.WriteStartDocument();

        writer.WriteStartElement("GameSession");

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

        // Modifers
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

    public static GameSession LoadFromXml(string path)
    {
        var session = new GameSession();

        var readerSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true
        };

        using var reader = XmlReader.Create(path, readerSettings);
        reader.ReadToFollowing("WorldIndex");
        session.WorldIndex = int.Parse(reader.GetAttribute("value") ?? throw new ResourceLoadException());

        reader.ReadToFollowing("LevelIndex");
        session.LevelIndex = int.Parse(reader.GetAttribute("value") ?? throw new ResourceLoadException());

        reader.ReadToFollowing("Player");
        session.PlayerHealth = float.Parse(reader.GetAttribute("health") ?? throw new ResourceLoadException());

        session._playerValues = ActorStartValues.FromXml(reader, "PlayerValues");

        LoadWeapons(reader, session);
        LoadModifiers(reader, session);
        LoadActiveObjects(reader, session);
        LoadEnemies(reader, session);

        return session;
    }

    private static void LoadWeapons(XmlReader reader, GameSession session)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Weapons" })
            {
                if (reader.IsEmptyElement)
                {
                    return;
                }

                session.SelectedWeapon = int.Parse(reader.GetAttribute("selected") ?? throw new ResourceLoadException());
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Weapon" })
            {
                int id = int.Parse(reader.GetAttribute("id") ?? throw new ResourceLoadException());
                int ammoCount = int.Parse(reader.GetAttribute("ammoCount") ?? throw new ResourceLoadException());
                int storedAmmo = int.Parse(reader.GetAttribute("storedAmmo") ?? throw new ResourceLoadException());

                session.WeaponSaves.Add(id, new WeaponSave(ammoCount, storedAmmo));
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Weapons" })
            {
                break;
            }
        }
    }

    private static void LoadModifiers(XmlReader reader, GameSession session)
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

                session.LevelModifiers.Add(new ModifierSave(id, timeLeft));
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Modifiers" })
            {
                return;
            }
        }
    }

    private static void LoadActiveObjects(XmlReader reader, GameSession session)
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

                session.ActivatedObjects.Add(key, new ActiveObjectSave(position, active));
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "ActiveObjects" })
            {
                return;
            }
        }
    }

    private static void LoadEnemies(XmlReader reader, GameSession session)
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

                session.SavedEnemies.Add(key, new EnemySave(position, velocity, health, active));
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Enemies" })
            {
                return;
            }
        }
    }
}