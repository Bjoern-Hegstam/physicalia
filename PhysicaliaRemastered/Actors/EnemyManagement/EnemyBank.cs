using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using PhysicaliaRemastered.Actors.Enemies;
using XNALibrary;
using XNALibrary.Animation;

namespace PhysicaliaRemastered.Actors.EnemyManagement;

public class EnemyBank
{
    /// <summary>
    /// Dictionary mapping the Id's of the enemy types to the base enemies.
    /// The animation keys kept by the enemies are those going to the
    /// base animations.
    /// </summary>
    private readonly Dictionary<int, Enemy> _enemyBank;

    private readonly IAnimationManager _animationManager;

    /// <summary>
    /// Creates a new EnemyBank instance and adds that instance as a service
    /// to the games service collection.
    /// </summary>
    public EnemyBank(IAnimationManager animationManager)
    {
        _animationManager = animationManager;

        _enemyBank = new Dictionary<int, Enemy>();
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

    /// <summary>
    /// Loads in the Enemy definitions from the passed in XmlReader and stores
    /// them with the id specified in the xml-file, as the key.
    /// </summary>
    /// <param name="reader"></param>
    public void LoadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Enemy" })
            {
                string typeName = reader.GetAttribute("type");
                int typeId = int.Parse(reader.GetAttribute("typeID") ?? throw new ResourceLoadException());

                Enemy enemy = CreateBaseEnemy(typeName);

                SetupEnemy(reader, enemy);
                LoadAnimations(reader, enemy);

                _enemyBank.Add(typeId, enemy);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "EnemyBank" })
            {
                return;
            }
        }
    }

    /// <summary>
    /// Creates a new Enemy based on the passed in type id.
    /// </summary>
    public Enemy CreateEnemy(int typeId, ActorStartValues startValues)
    {
        // Make sure the type has been defined
        if (!_enemyBank.ContainsKey(typeId))
        {
            return null;
        }

        // Get a copy of the type
        Enemy enemy = _enemyBank[typeId].Copy(startValues);

        // Set the enemies animation keys
        SetPlaybackKeys(typeId, enemy);

        // Return the result
        // ^ That's a very excessive comment. <- (So is that...and this)
        return enemy;
    }

    /// <summary>
    /// Sets the values of the passed in enemy as specified by the base
    /// enemy of the same type.
    /// </summary>
    public void SetupEnemy(Enemy enemy)
    {
        if (_enemyBank.TryGetValue(enemy.TypeId, out Enemy value))
        {
            value.Copy(enemy);
        }
    }

    /// <summary>
    /// Sets up the passed in Enemy according to the xml data.
    /// </summary>
    private void SetupEnemy(XmlReader reader, Enemy enemy)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Life" })
            {
                int health = int.Parse(reader.ReadString());
                enemy.Health = health;
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Damage" })
            {
                int damage = int.Parse(reader.ReadString());
                enemy.Damage = damage;
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "CollisionBox" })
            {
                enemy.CollisionBox = ReadRectangle(reader);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "PatrolArea" })
            {
                enemy.PatrolArea = ReadRectangle(reader);
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Setup" })
            {
                return;
            }
        }
    }

    /// <summary>
    /// Loads the base animations for a type and stores them in the AnimationManagers
    /// bank of animations. The keys to the stored animations are saved in
    /// the passed in Enemy's collection of animation keys, mapping them to
    /// the correct action.
    /// </summary>
    /// <param name="reader">XmlReader to read from.</param>
    /// <param name="enemy">Enemy to give the keys to.</param>
    private void LoadAnimations(XmlReader reader, Enemy enemy)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Animation" })
            {
                int animKey = int.Parse(reader.GetAttribute("key") ?? throw new ResourceLoadException());
                int action = int.Parse(reader.GetAttribute("action") ?? throw new ResourceLoadException());

                Animation anim = _animationManager.AddPlaybackAnimation(animKey);

                // Add the animation to the Enemy
                enemy.AddAnimation(action, anim);

                // Make sure the Enemy always has a valid animation set
                enemy.CurrentAnimationType = action;
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "Setup" })
            {
                return;
            }
        }
    }

    private Rectangle ReadRectangle(XmlReader reader)
    {
        int x = int.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
        int y = int.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());
        int width = int.Parse(reader.GetAttribute("width") ?? throw new ResourceLoadException());
        int height = int.Parse(reader.GetAttribute("height") ?? throw new ResourceLoadException());

        return new Rectangle(x, y, width, height);
    }

    /// <summary>
    /// Creates an Enemy which class name is that of the passed in string.
    /// </summary>
    /// <param name="typeName">Name of the enemy class.</param>
    /// <returns>The created Enemy or null if no type mathes the string.</returns>
    private static Enemy? CreateBaseEnemy(string typeName)
    {
        // Create the correct enemy based on the type name
        switch (typeName)
        {
            case "Enemy":
                return new Enemy(new ActorStartValues());
        }

        return null;
    }

    /// <summary>
    /// Sets the animation keys for the passed in Enemy as specified
    /// by the base Enemy mapped to the typeID key.
    /// </summary>
    /// <param name="typeId">Key to the base Enemy from which to create the
    /// playback animations</param>
    /// <param name="enemy">The Enemy to set the animation keys on.</param>
    /// <returns></returns>
    private void SetPlaybackKeys(int typeId, Enemy enemy)
    {
        Dictionary<int, Animation> bankAnimations = _enemyBank[typeId].Animations;

        foreach (int animType in bankAnimations.Keys)
        {
            Animation anim = bankAnimations[animType].Copy();

            _animationManager.AddPlaybackAnimation(anim);

            enemy.Animations.Add(animType, anim);
        }
    }
}