using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using PhysicaliaRemastered.Actors;

namespace PhysicaliaRemastered.GameManagement;

public struct ModifierSave
{
    public int ID;
    public float TimeLeft;

    public ModifierSave(int id, float timeLeft)
    {
        ID = id;
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
    private int worldIndex;
    private int levelIndex;

    // Player
    private ActorStartValues playerValues;
    private float playerHealth;
    private int selectedWeapon;
    private Dictionary<int, WeaponSave> weaponSaves;

    // Level
    private List<ModifierSave> levelModifiers;
    private Dictionary<int, ActiveObjectSave> activatedObjects;

    // EnemyManager
    private Dictionary<int, EnemySave> enemySaves;

    public int WorldIndex
    {
        get { return worldIndex; }
        set { worldIndex = value; }
    }

    public int LevelIndex
    {
        get { return levelIndex; }
        set { levelIndex = value; }
    }

    public ActorStartValues PlayerValues
    {
        get { return playerValues; }
        set { playerValues = value; }
    }

    public float PlayerHealth
    {
        get { return playerHealth; }
        set { playerHealth = value; }
    }

    public int SelectedWeapon
    {
        get { return selectedWeapon; }
        set { selectedWeapon = value; }
    }

    public Dictionary<int, WeaponSave> WeaponSaves
    {
        get { return weaponSaves; }
    }

    public List<ModifierSave> LevelModifiers
    {
        get { return levelModifiers; }
    }

    public Dictionary<int, ActiveObjectSave> ActivatedObjects
    {
        get { return activatedObjects; }
    }

    public Dictionary<int, EnemySave> SavedEnemies
    {
        get { return enemySaves; }
    }

    public GameSession()
    {
        this.weaponSaves = new Dictionary<int, WeaponSave>();
        this.levelModifiers = new List<ModifierSave>();
        this.activatedObjects = new Dictionary<int, ActiveObjectSave>();
        this.enemySaves = new Dictionary<int, EnemySave>();
    }

    public void SaveToXml(string path)
    {
        XmlWriterSettings writerSettings = new XmlWriterSettings();
        writerSettings.Encoding = Encoding.UTF8;

        using (XmlWriter writer = XmlWriter.Create(path, writerSettings))
        {
            writer.WriteStartDocument();

            writer.WriteStartElement("GameSession");

            writer.WriteStartElement("WorldIndex");
            writer.WriteAttributeString("value", this.worldIndex.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("LevelIndex");
            writer.WriteAttributeString("value", this.levelIndex.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("Player");
            writer.WriteAttributeString("health", this.playerHealth.ToString());

            // Player values
            writer.WriteStartElement("PlayerValues");
            this.WriteVector2(writer, "Position", this.playerValues.Position);
            this.WriteVector2(writer, "Velocity", this.playerValues.Velocity);
            this.WriteVector2(writer, "Acceleration", this.playerValues.Acceleration);
            writer.WriteEndElement();

            writer.WriteStartElement("Weapons");
            writer.WriteAttributeString("selected", this.selectedWeapon.ToString());

            foreach (int weaponID in this.weaponSaves.Keys)
            {
                WeaponSave weaponSave = this.weaponSaves[weaponID];

                writer.WriteStartElement("Weapon");
                writer.WriteAttributeString("id", weaponID.ToString());
                writer.WriteAttributeString("ammoCount", weaponSave.AmmoCount.ToString());
                writer.WriteAttributeString("storedAmmo", weaponSave.StoredAmmo.ToString());
                writer.WriteEndElement();
            }

            // End of weapons
            writer.WriteEndElement();

            // End of player
            writer.WriteEndElement();

            // Modifers
            this.WriteModifiers(writer);

            // Activated objects
            this.WriteActiveObjects(writer);

            // Activated enemies
            this.WriteEnemies(writer);

            writer.WriteEndDocument();
        }
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

        foreach (ModifierSave modifier in this.levelModifiers)
        {
            writer.WriteStartElement("Modifier");
            writer.WriteAttributeString("id", modifier.ID.ToString());
            writer.WriteAttributeString("timeLeft", modifier.TimeLeft.ToString());
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private void WriteActiveObjects(XmlWriter writer)
    {
        writer.WriteStartElement("ActiveObjects");

        foreach (int key in this.activatedObjects.Keys)
        {
            ActiveObjectSave save = this.activatedObjects[key];

            writer.WriteStartElement("ActiveObject");
            writer.WriteAttributeString("key", key.ToString());
            writer.WriteAttributeString("active", save.IsActive.ToString());

            this.WriteVector2(writer, "Position", save.Position);

            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private void WriteEnemies(XmlWriter writer)
    {
        writer.WriteStartElement("Enemies");

        foreach (int key in this.enemySaves.Keys)
        {
            EnemySave save = this.enemySaves[key];

            writer.WriteStartElement("Enemy");
            writer.WriteAttributeString("key", key.ToString());
            writer.WriteAttributeString("health", save.Health.ToString());
            writer.WriteAttributeString("active", save.IsActive.ToString());

            this.WriteVector2(writer, "Position", save.Position);
            this.WriteVector2(writer, "Velocity", save.Velocity);

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

        using (XmlReader reader = XmlReader.Create(path, readerSettings))
        {
            reader.ReadToFollowing("WorldIndex");
            session.worldIndex = int.Parse(reader.GetAttribute("value"));

            reader.ReadToFollowing("LevelIndex");
            session.levelIndex = int.Parse(reader.GetAttribute("value"));

            reader.ReadToFollowing("Player");
            session.playerHealth = float.Parse(reader.GetAttribute("health"));

            session.playerValues = ActorStartValues.FromXml(reader, "PlayerValues");

            LoadWeapons(reader, session);
            LoadModifiers(reader, session);
            LoadActiveObjects(reader, session);
            LoadEnemies(reader, session);
        }

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
                    session.selectedWeapon = int.Parse(reader.GetAttribute("selected"));
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Weapon")
            {
                int id = int.Parse(reader.GetAttribute("id"));
                int ammoCount = int.Parse(reader.GetAttribute("ammoCount"));
                int storedAmmo = int.Parse(reader.GetAttribute("storedAmmo"));

                session.weaponSaves.Add(id, new WeaponSave(ammoCount, storedAmmo));
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

                session.levelModifiers.Add(new ModifierSave(id, timeLeft));
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

                session.activatedObjects.Add(key, new ActiveObjectSave(position, active));
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

                session.enemySaves.Add(key, new EnemySave(position, velocity, health, active));
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "Enemies")
                return;
        }
    }
}