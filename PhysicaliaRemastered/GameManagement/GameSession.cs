using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using PhysicaliaRemastered.Actors;

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
        LevelModifiers = new List<ModifierSave>();
        ActivatedObjects = new Dictionary<int, ActiveObjectSave>();
        SavedEnemies = new Dictionary<int, EnemySave>();
    }

    public void SaveToXml(string path)
    {
        XmlWriterSettings writerSettings = new XmlWriterSettings();
        writerSettings.Encoding = Encoding.UTF8;

        using XmlWriter writer = XmlWriter.Create(path, writerSettings);
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
        GameSession session = new GameSession();

        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.IgnoreComments = true;
        readerSettings.IgnoreProcessingInstructions = true;
        readerSettings.IgnoreWhitespace = true;

        using XmlReader reader = XmlReader.Create(path, readerSettings);
        reader.ReadToFollowing("WorldIndex");
        session.WorldIndex = int.Parse(reader.GetAttribute("value"));

        reader.ReadToFollowing("LevelIndex");
        session.LevelIndex = int.Parse(reader.GetAttribute("value"));

        reader.ReadToFollowing("Player");
        session.PlayerHealth = float.Parse(reader.GetAttribute("health"));

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
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Weapons")
            {
                if (reader.IsEmptyElement)
                    return;
                else
                    session.SelectedWeapon = int.Parse(reader.GetAttribute("selected"));
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Weapon")
            {
                int id = int.Parse(reader.GetAttribute("id"));
                int ammoCount = int.Parse(reader.GetAttribute("ammoCount"));
                int storedAmmo = int.Parse(reader.GetAttribute("storedAmmo"));

                session.WeaponSaves.Add(id, new WeaponSave(ammoCount, storedAmmo));
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "Weapons")
                break;
        }
    }

    private static void LoadModifiers(XmlReader reader, GameSession session)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Modifiers" &&
                reader.IsEmptyElement)
                return;

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Modifier")
            {
                int id = int.Parse(reader.GetAttribute("id"));
                float timeLeft = float.Parse(reader.GetAttribute("timeLeft"));

                session.LevelModifiers.Add(new ModifierSave(id, timeLeft));
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "Modifiers")
                return;
        }
    }

    private static void LoadActiveObjects(XmlReader reader, GameSession session)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "ActiveObjects" &&
                reader.IsEmptyElement)
                return;

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "ActiveObject")
            {
                int key = int.Parse(reader.GetAttribute("key"));
                bool active = bool.Parse(reader.GetAttribute("active"));

                reader.ReadToFollowing("Position");
                float x = float.Parse(reader.GetAttribute("x"));
                float y = float.Parse(reader.GetAttribute("y"));
                Vector2 position = new Vector2(x, y);

                session.ActivatedObjects.Add(key, new ActiveObjectSave(position, active));
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "ActiveObjects")
                return;
        }
    }

    private static void LoadEnemies(XmlReader reader, GameSession session)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Enemies" &&
                reader.IsEmptyElement)
                return;

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Enemy")
            {
                int key = int.Parse(reader.GetAttribute("key"));
                float health = float.Parse(reader.GetAttribute("health"));
                bool active = bool.Parse(reader.GetAttribute("active"));

                reader.ReadToFollowing("Position");
                float posX = float.Parse(reader.GetAttribute("x"));
                float posY = float.Parse(reader.GetAttribute("y"));
                Vector2 position = new Vector2(posX, posY);

                reader.ReadToFollowing("Velocity");
                float velX = float.Parse(reader.GetAttribute("x"));
                float velY = float.Parse(reader.GetAttribute("y"));
                Vector2 velocity = new Vector2(velX, velY);

                session.SavedEnemies.Add(key, new EnemySave(position, velocity, health, active));
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "Enemies")
                return;
        }
    }
}