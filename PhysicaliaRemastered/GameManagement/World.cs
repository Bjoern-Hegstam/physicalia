using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Actors;
using PhysicaliaRemastered.Input;
using XNALibrary.Graphics.Sprites;
using XNALibrary.Interfaces;

namespace PhysicaliaRemastered.GameManagement;

public enum WorldState
{
    Start,
    PlayingLevel,
    Finished
}

/// <summary>
/// Class representing a World in the game. A World consist of a certain
/// number of Levels and a final BossLevel. If a BossLevel is not present
/// the World will be considered completed after the player has finished
/// the final Level.
/// </summary>
public class World
{
    private const string LEVEL_PATH = @"Content\GameData\Worlds\Levels\";

    private ISettings settings;

    private List<Level> levels;
    private int levelIndex;

    // Presentation
    private int worldIndex;
    private string[] worldQuoteLines;
    private Color worldIndexColor;
    private Color worldQuoteColor;
    private Sprite worldSprite;

    // TODO: Add fields for keeping the text to view when finished

    private WorldState state;
    private WorldState nextState;

    private Game game;
    private Player player;

    public int WorldIndex
    {
        get { return worldIndex; }
        set { worldIndex = value; }
    }

    public WorldState State
    {
        get { return state; }
    }

    public LevelState LevelState
    {
        get { return levels[levelIndex].State; }
    }

    public World(Game game, Player player)
    {
        this.game = game;
        this.player = player;

        settings = (ISettings)this.game.Services.GetService(typeof(ISettings));

        levels = new List<Level>();
        levelIndex = -1;

        nextState = state = WorldState.Start;

        worldIndex = -1;
        worldIndexColor = Color.White;
        worldQuoteLines = null;
        worldQuoteColor = Color.White;
    }

    public void LoadXml(string path, ITileLibrary tileLibrary, ISpriteLibrary spriteLibrary)
    {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.IgnoreComments = true;
        readerSettings.IgnoreProcessingInstructions = true;
        readerSettings.IgnoreWhitespace = true;

        using (XmlReader reader = XmlReader.Create(path, readerSettings))
            LoadXml(reader, tileLibrary, spriteLibrary);
    }

    public void LoadXml(XmlReader reader, ITileLibrary tileLibrary, ISpriteLibrary spriteLibrary)
    {
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "StartSprite")
            {
                int key = int.Parse(reader.GetAttribute("key"));
                worldSprite = spriteLibrary.GetSprite(key);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Quote")
            {
                // Read color
                string[] colorValues = reader.GetAttribute("color").Split(' ');

                // Must have all needed value (rgba)
                if (colorValues.Length < 4)
                    continue;

                byte r = byte.Parse(colorValues[0]);
                byte g = byte.Parse(colorValues[1]);
                byte b = byte.Parse(colorValues[2]);
                byte a = byte.Parse(colorValues[3]);

                worldQuoteColor = new Color(r, b, g, a);

                // Get the quote
                StringBuilder quoteBuilder = new StringBuilder();
                List<string> quoteLines = new List<string>();

                // Read in any special characters
                while (!(reader.NodeType == XmlNodeType.EndElement &&
                         reader.LocalName == "Quote"))
                {
                    if (reader.LocalName == "br")
                    {
                        quoteLines.Add(quoteBuilder.ToString());
                        quoteBuilder = new StringBuilder();

                        reader.Read();
                    }
                    else if (reader.NodeType == XmlNodeType.Element &&
                             reader.LocalName == "Char")
                    {
                        int charNum = int.Parse(reader.ReadString());
                        char character = (char)charNum;
                        quoteBuilder.Append(character);
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                        quoteBuilder.Append(reader.ReadString());
                    else
                        reader.Read();

                }

                // Store parsed quote
                if (quoteBuilder.Length > 0)
                    quoteLines.Add(quoteBuilder.ToString());
                worldQuoteLines = quoteLines.ToArray();
            }

            // Read in Levels
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Level")
            {
                // Create and initialize the new Level
                Level level = new Level(game, player);

                using (XmlReader levelReader = XmlReader.Create(LEVEL_PATH + reader.ReadString(), reader.Settings))
                    level.LoadXml(levelReader, tileLibrary);

                // Store the Level reference
                levels.Add(level);
                level.LevelIndex = levels.Count;
                level.WorldIndex = worldIndex;
            }

            // Read in BossLevels
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "BossLevel")
            {
                BossLevel level = new BossLevel(game, player);

                using (XmlReader levelReader = XmlReader.Create(reader.ReadElementContentAsString()))
                    level.LoadXml(levelReader, tileLibrary);

                levels.Add(level);
                level.LevelIndex = levels.Count;
            }
        }

        // Set the index of the current level
        if (levels.Count > 0)
            levelIndex = 0;
    }

    public void Update(GameTime gameTime)
    {
        switch (state)
        {
            case WorldState.Start:
                if (settings.InputMap.IsPressed(InputAction.MenuStart))
                    nextState = WorldState.PlayingLevel;
                break;
            case WorldState.PlayingLevel:
                if (levels[levelIndex].State == LevelState.Finished)
                {
                    levels[levelIndex].Update(gameTime);

                    if (settings.InputMap.IsPressed(InputAction.MenuStart))
                    {
                        // Go to next level
                        levelIndex++;

                        // World is finished if there are no more levels
                        if (levelIndex >= levels.Count)
                            nextState = WorldState.Finished;
                        else
                            // Reset the next level
                            levels[levelIndex].Reset();
                    }
                }
                else if (levels[levelIndex].State == LevelState.Dead)
                {
                    levels[levelIndex].Update(gameTime);

                    if (settings.InputMap.IsPressed(InputAction.MenuStart))
                        levels[levelIndex].Reset();
                }
                else
                    // Update the current level
                    levels[levelIndex].Update(gameTime);
                break;
            case WorldState.Finished:
                break;
        }

        while (nextState != state)
            ChangeState();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        switch (state)
        {
            case WorldState.Start:
                // Write draw world index
                string indexString = "World " + worldIndex;
                Vector2 indexStringSize = settings.WorldIndexFont.MeasureString(indexString);
                Vector2 indexPosition = new Vector2();
                indexPosition.X = (levels[0].ScreenSampler.Width - indexStringSize.X) / 2;
                indexPosition.Y = levels[0].ScreenSampler.Height / 4 - indexStringSize.Y / 2;
                spriteBatch.DrawString(settings.WorldIndexFont, indexString, indexPosition, worldIndexColor);

                // Draw quote
                if (worldQuoteLines != null)
                {
                    float quoteHeight = settings.WorldQuoteFont.MeasureString("W").Y;
                    Vector2 quoteStartPos = new Vector2();
                    quoteStartPos.Y = levels[0].ScreenSampler.Height * 3 / 4 - quoteHeight * worldQuoteLines.Length / 2;

                    for (int i = 0; i < worldQuoteLines.Length; i++)
                    {
                        Vector2 quoteSize = settings.WorldQuoteFont.MeasureString(worldQuoteLines[i]);
                        quoteStartPos.X = (levels[0].ScreenSampler.Width - quoteSize.X) / 2;

                        spriteBatch.DrawString(settings.WorldQuoteFont,
                            worldQuoteLines[i],
                            quoteStartPos,
                            worldQuoteColor);

                        quoteStartPos.Y += quoteHeight;
                    }
                }

                // Draw World Sprite
                Vector2 spritePos = new Vector2();
                spritePos.X = (levels[0].ScreenSampler.Width - worldSprite.SourceRectangle.Width) / 2;
                spritePos.Y = (levels[0].ScreenSampler.Height - worldSprite.SourceRectangle.Height) / 2;

                spriteBatch.Draw(worldSprite.Texture,
                    spritePos,
                    worldSprite.SourceRectangle,
                    Color.White);
                break;
            case WorldState.PlayingLevel:
                levels[levelIndex].Draw(spriteBatch);
                break;
            case WorldState.Finished:
                spriteBatch.DrawString(settings.WorldQuoteFont, "World Finished", new Vector2(160, 200), Color.White);
                break;
        }
    }

    private void ChangeState()
    {
        switch (nextState)
        {
            case WorldState.Start:
                break;
            case WorldState.PlayingLevel:
                if (levels.Count == 0)
                    nextState = WorldState.Finished;
                else
                    levels[levelIndex].Reset();
                break;
            case WorldState.Finished:
                break;
        }

        state = nextState;
    }

    public void ResetLevel()
    {
        if (levelIndex < levels.Count)
            levels[levelIndex].Reset();
    }

    public void NewSession()
    {
        levelIndex = 0;
        levels[levelIndex].Reset();

        state = nextState = WorldState.Start;
    }

    public void LoadSession(GameSession session)
    {
        levelIndex = session.LevelIndex;
        levels[levelIndex].LoadSession(session);

        state = nextState = WorldState.PlayingLevel;
    }

    public void SaveSession(GameSession session)
    {
        session.LevelIndex = levelIndex;
        levels[levelIndex].SaveSession(session);
    }
}