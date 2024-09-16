using System;
using System.Collections.Generic;
using System.Text;
using Physicalia.Input;
using Microsoft.Xna.Framework;
using XNALibrary.Services;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Xml;
using XNALibrary.Graphics;

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
        get { return this.inputType; }
        set
        {
            this.inputType = value;

            // Change current input map if needed
            if (this.inputMap == this.keyboardMap &&
                this.inputType == InputType.Gamepad)
                this.inputMap = this.gamePadMap;
        }
    }

    public InputMap InputMap
    {
        get { return this.inputMap; }
    }

    private static Random random;

    public static Random Random
    {
        get { return random; }
    }

    private float playerStartHealth;

    public float PlayerStartHealth
    {
        get { return this.playerStartHealth; }
        set { this.playerStartHealth = value; }
    }

    private SpriteFont worldQuoteFont;
    private SpriteFont worldIndexFont;
    private SpriteFont levelIndexFont;
    private SpriteFont playerDeadFont;
    private SpriteFont weaponAmmoFont;
    private SpriteFont pauseMenuFont;

    public SpriteFont WorldQuoteFont { get { return this.worldQuoteFont; } }
    public SpriteFont WorldIndexFont { get { return this.worldIndexFont; } }
    public SpriteFont LevelIndexFont { get { return this.levelIndexFont; } }
    public SpriteFont PlayerDeadFont { get { return this.playerDeadFont; } }
    public SpriteFont WeaponAmmoFont { get { return this.weaponAmmoFont; } }
    public SpriteFont PauseMenuFont { get { return this.pauseMenuFont; } }

    private Sprite fullHealthUI;
    private Sprite emptyHealthUI;

    public Sprite FullHealthUI
    {
        get { return this.fullHealthUI; }
    }

    public Sprite EmptyHealthUI
    {
        get { return this.emptyHealthUI; }
    }

    static Settings()
    {
        random = new Random();
    }

    public Settings(IInputHandler inputHandler)
    {
        IInputHandler input = inputHandler;
            
        this.gamePadMap = new GamePadInputMap();
        this.gamePadMap.InputHandler = input;

        this.keyboardMap = new KeyboardInputMap();
        this.keyboardMap.InputHandler = input;

        this.inputMap = this.keyboardMap;
        this.inputType = InputType.Keyboard;
    }

    public void LoadContent(ContentManager contentManager)
    {
        this.worldQuoteFont = contentManager.Load<SpriteFont>(@"Fonts\WorldQuoteFont");
        this.worldIndexFont = contentManager.Load<SpriteFont>(@"Fonts\WorldIndexFont");
        this.levelIndexFont = contentManager.Load<SpriteFont>(@"Fonts\LevelIndexFont");
        this.playerDeadFont = contentManager.Load<SpriteFont>(@"Fonts\PlayerDeadFont");
        this.weaponAmmoFont = contentManager.Load<SpriteFont>(@"Fonts\WeaponAmmoFont");
        this.pauseMenuFont = contentManager.Load<SpriteFont>(@"Fonts\PauseMenuFont");
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
                    this.InputType = input;
                }

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "PlayerHealth")
                {
                    int health = int.Parse(reader.ReadString());
                    this.playerStartHealth = health;
                }

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "KeyboardMap")
                    this.keyboardMap.LoadXml(reader.ReadString());

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "GamepadMap")
                    this.gamePadMap.LoadXml(reader.ReadString());

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "UI")
                {
                    reader.ReadToFollowing("FullHealthBar");
                    int spriteKey = int.Parse(reader.GetAttribute("key"));
                    this.fullHealthUI = spriteLibrary.GetSprite(spriteKey);

                    reader.ReadToFollowing("EmptyHealthBar");
                    spriteKey = int.Parse(reader.GetAttribute("key"));
                    this.emptyHealthUI = spriteLibrary.GetSprite(spriteKey);
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