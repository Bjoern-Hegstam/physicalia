using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using PhysicaliaRemastered.Actors.Enemies;
using XNALibrary.Graphics.Animation;
using XNALibrary.Interfaces;

namespace PhysicaliaRemastered.Actors.EnemyManagement;

public class EnemyBank : IEnemyBank
{
    /// <summary>
    /// Dictionary mapping the Id's of the enemy types to the base enemies.
    /// The animation keys kept by the enemies are those going to the
    /// base animations.
    /// </summary>
    private Dictionary<int, Enemy> enemyBank;

    private IAnimationManager animationManager;

    /// <summary>
    /// Creates a new EnemyBank instance and adds that instance as a service
    /// to the games service collection.
    /// </summary>
    /// <param name="device">GraphicsDevice used when loading textures.</param>
    /// <param name="animationManager">The AnimationManager used by the EnemyBank
    /// when creating animations for new enemies.</param>
    public EnemyBank(IAnimationManager animationManager)
    {
            this.animationManager = animationManager;

            enemyBank = new Dictionary<int, Enemy>();
        }

    public void LoadXml(string path)
    {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            readerSettings.IgnoreWhitespace = true;
            readerSettings.IgnoreProcessingInstructions = true;

            using (XmlReader reader = XmlReader.Create(path, readerSettings))
            {
                LoadXml(reader);
            }
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
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Enemy")
                {
                    string typeName = reader.GetAttribute("type");
                    int typeID = int.Parse(reader.GetAttribute("typeID"));

                    Enemy enemy = CreateBaseEnemy(typeName);

                    SetupEnemy(reader, enemy);
                    LoadAnimations(reader, enemy);
                    
                    enemyBank.Add(typeID, enemy);
                }

                if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.LocalName == "EnemyBank")
                    return;
            }
        }

    /// <summary>
    /// Adds the enemy to the bank of base enemies.
    /// </summary>
    /// <param name="typeID">ID of the enemies type.</param>
    /// <param name="enemy">The enemy to add.</param>
    public void AddBaseEnemy(int typeID, Enemy enemy)
    {
            enemyBank.Add(typeID, enemy);
        }

    /// <summary>
    /// Creates a new Enemy based on the passed in type id.
    /// </summary>
    /// <param name="typeID">Id of the type of Enemy to create.</param>
    /// /// <param name="startValues">Start values to give the Enemy.</param>
    /// <returns>The created enemy or null if no type using the id has been stored.</returns>
    public Enemy CreateEnemy(int typeID, ActorStartValues startValues)
    {
            // Make sure the type has been defined
            if (!enemyBank.ContainsKey(typeID))
                return null;

            // Get a copy of the type
            Enemy enemy = enemyBank[typeID].Copy(startValues);

            // Set the enemies animation keys
            SetPlaybackKeys(typeID, enemy);

            // Return the result
            // ^ That's a very excessive comment. <- (So is that...and this)
            return enemy;
        }

    /// <summary>
    /// Sets the values of the passed in enemy as specified by the base
    /// enemy of the same type.
    /// </summary>
    /// <param name="enemy">Enemy to the set the values on.</param>
    public void SetupEnemy(Enemy enemy)
    {
            if (enemyBank.ContainsKey(enemy.TypeID))
                enemyBank[enemy.TypeID].Copy(enemy);
        }

    /// <summary>
    /// Sets up the passed in Enemy according to the xml data.
    /// </summary>
    /// <param name="reader"></param>
    private void SetupEnemy(XmlReader reader, Enemy enemy)
    {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Life")
                {
                    int health = int.Parse(reader.ReadString());
                    enemy.Health = health;
                }

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Damage")
                {
                    int damage = int.Parse(reader.ReadString());
                    enemy.Damage = damage;
                }

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "CollisionBox")
                    enemy.CollisionBox = ReadRectangle(reader);

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "PatrolArea")
                {
                    enemy.PatrolArea = ReadRectangle(reader);
                }

                if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.LocalName == "Setup")
                    return;
            }
        }

    /// <summary>
    /// Loads the base animations for a type and stores them in the AnimationManagers
    /// bank of animations. The keys to the stored animations are saved in
    /// the passed in Enemy's collection of animationkeys, mapping them to
    /// the correct action.
    /// </summary>
    /// <param name="reader">XmlReader to read from.</param>
    /// <param name="enemy">Enemy to give the keys to.</param>
    private void LoadAnimations(XmlReader reader, Enemy enemy)
    {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Animation")
                {
                    int animKey = int.Parse(reader.GetAttribute("key"));
                    int action = int.Parse(reader.GetAttribute("action"));

                    Animation anim = animationManager.AddPlaybackAnimation(animKey);

                    // Add the animation to the Enemy
                    enemy.AddAnimation(action, anim);

                    // Make sure the Enemy always has a valid animation set
                    enemy.CurrentAnimationType = action;
                }

                if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.LocalName == "Setup")
                    return;
            }
        }

    private Rectangle ReadRectangle(XmlReader reader)
    {
            int x = int.Parse(reader.GetAttribute("x"));
            int y = int.Parse(reader.GetAttribute("y"));
            int width = int.Parse(reader.GetAttribute("width"));
            int height = int.Parse(reader.GetAttribute("height"));

            return new Rectangle(x, y, width, height);
        }

    /// <summary>
    /// Creates an Enemy which class name is that of the passed in string.
    /// </summary>
    /// <param name="typeName">Name of the enemy class.</param>
    /// <returns>The created Enemy or null if no type mathed the string.</returns>
    private Enemy CreateBaseEnemy(string typeName)
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
    /// <param name="typeID">Key to the base Enemy from which to create the
    /// playback animations</param>
    /// <param name="enemy">The Enemy to set the animationkeys on.</param>
    /// <returns></returns>
    private void SetPlaybackKeys(int typeID, Enemy enemy)
    {
            // Get the types animation keys
            Dictionary<int, Animation> bankAnimations = enemyBank[typeID].Animations;

            Animation anim;
            
            // For every animation type
            foreach (int animType in bankAnimations.Keys)
            {
                // Get a copy of the base animation
                anim = bankAnimations[animType].Copy();
                
                // Store the animation
                animationManager.AddPlaybackAnimation(anim);
                
                // Give animation to the enemy
                enemy.Animations.Add(animType, anim);
            }
        }
}