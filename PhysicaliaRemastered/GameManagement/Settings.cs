using System;
using System.Xml;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Input;
using XNALibrary.Graphics.Sprites;
using XNALibrary.Interfaces;

namespace PhysicaliaRemastered.GameManagement;

public enum InputType
{
    Gamepad,
    Keyboard
}

/// <summary>
/// Contains the data needed to create the game.
/// </summary>
public class Settings : ISettings
{
    private InputType inputType;
    private InputMap inputMap;
    private KeyboardInputMap keyboardMap;
    private GamePadInputMap gamePadMap;

    public InputType InputType
    {
        get { return inputType; }
        set
        {
            inputType = value;

            // Change current input map if needed
            if (inputMap == keyboardMap &&
                inputType == InputType.Gamepad)
                inputMap = gamePadMap;
        }
    }

    public InputMap InputMap
    {
        get { return inputMap; }
    }

    private static Random random;

    public static Random Random
    {
        get { return random; }
    }

    private float playerStartHealth;

    public float PlayerStartHealth
    {
        get { return playerStartHealth; }
        set { playerStartHealth = value; }
    }

    private SpriteFont worldQuoteFont;
    private SpriteFont worldIndexFont;
    private SpriteFont levelIndexFont;
    private SpriteFont playerDeadFont;
    private SpriteFont weaponAmmoFont;
    private SpriteFont pauseMenuFont;

    public SpriteFont WorldQuoteFont { get { return worldQuoteFont; } }
    public SpriteFont WorldIndexFont { get { return worldIndexFont; } }
    public SpriteFont LevelIndexFont { get { return levelIndexFont; } }
    public SpriteFont PlayerDeadFont { get { return playerDeadFont; } }
    public SpriteFont WeaponAmmoFont { get { return weaponAmmoFont; } }
    public SpriteFont PauseMenuFont { get { return pauseMenuFont; } }

    private Sprite fullHealthUI;
    private Sprite emptyHealthUI;

    public Sprite FullHealthUI
    {
        get { return fullHealthUI; }
    }

    public Sprite EmptyHealthUI
    {
        get { return emptyHealthUI; }
    }

    static Settings()
    {
        random = new Random();
    }

    public Settings(IInputHandler inputHandler)
    {
        IInputHandler input = inputHandler;
            
        gamePadMap = new GamePadInputMap();
        gamePadMap.InputHandler = input;

        keyboardMap = new KeyboardInputMap();
        keyboardMap.InputHandler = input;

        inputMap = keyboardMap;
        inputType = InputType.Keyboard;
    }

    public void LoadContent(ContentManager contentManager)
    {
        worldQuoteFont = contentManager.Load<SpriteFont>(@"Fonts\WorldQuoteFont");
        worldIndexFont = contentManager.Load<SpriteFont>(@"Fonts\WorldIndexFont");
        levelIndexFont = contentManager.Load<SpriteFont>(@"Fonts\LevelIndexFont");
        playerDeadFont = contentManager.Load<SpriteFont>(@"Fonts\PlayerDeadFont");
        weaponAmmoFont = contentManager.Load<SpriteFont>(@"Fonts\WeaponAmmoFont");
        pauseMenuFont = contentManager.Load<SpriteFont>(@"Fonts\PauseMenuFont");
    }

    public void LoadXml(string path, ISpriteLibrary spriteLibrary)
    {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.IgnoreComments = true;
        readerSettings.IgnoreWhitespace = true;
        readerSettings.IgnoreProcessingInstructions = true;

        using (XmlReader reader = XmlReader.Create(path, readerSettings))
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "InputType")
                {
                    InputType input = (InputType)Enum.Parse(typeof(InputType), reader.ReadString());
                    InputType = input;
                }

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "PlayerHealth")
                {
                    int health = int.Parse(reader.ReadString());
                    playerStartHealth = health;
                }

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "KeyboardMap")
                    keyboardMap.LoadXml(reader.ReadString());

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "GamepadMap")
                    gamePadMap.LoadXml(reader.ReadString());

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "UI")
                {
                    reader.ReadToFollowing("FullHealthBar");
                    int spriteKey = int.Parse(reader.GetAttribute("key"));
                    fullHealthUI = spriteLibrary.GetSprite(spriteKey);

                    reader.ReadToFollowing("EmptyHealthBar");
                    spriteKey = int.Parse(reader.GetAttribute("key"));
                    emptyHealthUI = spriteLibrary.GetSprite(spriteKey);
                }

                if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.LocalName == "Settings")
                    break;
            }
        }
    }
}
    
// Interface for providing the Settings as a service that can be accessed via the Game.
public interface ISettings
{
    InputType InputType { get; set; }
    InputMap InputMap { get; }

    float PlayerStartHealth { get; set; }

    SpriteFont WorldQuoteFont { get; }
    SpriteFont WorldIndexFont { get; }
    SpriteFont LevelIndexFont { get; }
    SpriteFont PlayerDeadFont { get; }
    SpriteFont WeaponAmmoFont { get; }
    SpriteFont PauseMenuFont { get; }

    Sprite FullHealthUI { get; }
    Sprite EmptyHealthUI { get; }

    void LoadContent(ContentManager contentManager);
    void LoadXml(string path, ISpriteLibrary spriteLibrary);
}