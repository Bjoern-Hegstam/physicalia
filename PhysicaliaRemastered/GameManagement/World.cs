using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Physicalia.Input;
using Physicalia.Weapons;
using XNALibrary.Services;
using XNALibrary.Graphics;

namespace PhysicaliaRemastered.GameManagement
{
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

        #region Fields

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

        #endregion

        #region Properties

        public int WorldIndex
        {
            get { return this.worldIndex; }
            set { this.worldIndex = value; }
        }

        public WorldState State
        {
            get { return this.state; }
        }

        public LevelState LevelState
        {
            get { return this.levels[this.levelIndex].State; }
        }

        #endregion

        #region Constructors

        public World(Game game, Player player)
        {
            this.game = game;
            this.player = player;

            this.settings = (ISettings)this.game.Services.GetService(typeof(ISettings));

            this.levels = new List<Level>();
            this.levelIndex = -1;

            this.nextState = this.state = WorldState.Start;

            this.worldIndex = -1;
            this.worldIndexColor = Color.White;
            this.worldQuoteLines = null;
            this.worldQuoteColor = Color.White;
        }

        #endregion

        #region Public methods

        public void LoadXml(string path, ITileLibrary tileLibrary, ISpriteLibrary spriteLibrary)
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            readerSettings.IgnoreProcessingInstructions = true;
            readerSettings.IgnoreWhitespace = true;

            using (XmlReader reader = XmlReader.Create(path, readerSettings))
                this.LoadXml(reader, tileLibrary, spriteLibrary);
        }

        public void LoadXml(XmlReader reader, ITileLibrary tileLibrary, ISpriteLibrary spriteLibrary)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "StartSprite")
                {
                    int key = int.Parse(reader.GetAttribute("key"));
                    this.worldSprite = spriteLibrary.GetSprite(key);
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

                    this.worldQuoteColor = new Color(r, b, g, a);

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
                    this.worldQuoteLines = quoteLines.ToArray();
                }

                // Read in Levels
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Level")
                {
                    // Create and initialize the new Level
                    Level level = new Level(this.game, this.player);

                    using (XmlReader levelReader = XmlReader.Create(LEVEL_PATH + reader.ReadString(), reader.Settings))
                        level.LoadXml(levelReader, tileLibrary);

                    // Store the Level reference
                    this.levels.Add(level);
                    level.LevelIndex = this.levels.Count;
                    level.WorldIndex = this.worldIndex;
                }

                // Read in BossLevels
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "BossLevel")
                {
                    BossLevel level = new BossLevel(this.game, this.player);

                    using (XmlReader levelReader = XmlReader.Create(reader.ReadElementContentAsString()))
                        level.LoadXml(levelReader, tileLibrary);

                    this.levels.Add(level);
                    level.LevelIndex = this.levels.Count;
                }
            }

            // Set the index of the current level
            if (this.levels.Count > 0)
                this.levelIndex = 0;
        }

        public void Update(GameTime gameTime)
        {
            switch (this.state)
            {
                case WorldState.Start:
                    if (this.settings.InputMap.IsPressed(InputAction.MenuStart))
                        this.nextState = WorldState.PlayingLevel;
                    break;
                case WorldState.PlayingLevel:
                    if (this.levels[this.levelIndex].State == LevelState.Finished)
                    {
                        this.levels[this.levelIndex].Update(gameTime);

                        if (this.settings.InputMap.IsPressed(InputAction.MenuStart))
                        {
                            // Go to next level
                            this.levelIndex++;

                            // World is finished if there are no more levels
                            if (this.levelIndex >= this.levels.Count)
                                this.nextState = WorldState.Finished;
                            else
                                // Reset the next level
                                this.levels[this.levelIndex].Reset();
                        }
                    }
                    else if (this.levels[this.levelIndex].State == LevelState.Dead)
                    {
                        this.levels[this.levelIndex].Update(gameTime);

                        if (this.settings.InputMap.IsPressed(InputAction.MenuStart))
                            this.levels[this.levelIndex].Reset();
                    }
                    else
                        // Update the current level
                        this.levels[this.levelIndex].Update(gameTime);
                    break;
                case WorldState.Finished:
                    break;
            }

            while (this.nextState != this.state)
                this.ChangeState();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            switch (this.state)
            {
                case WorldState.Start:
                    // Write draw world index
                    string indexString = "World " + this.worldIndex;
                    Vector2 indexStringSize = this.settings.WorldIndexFont.MeasureString(indexString);
                    Vector2 indexPosition = new Vector2();
                    indexPosition.X = (this.levels[0].ScreenSampler.Width - indexStringSize.X) / 2;
                    indexPosition.Y = this.levels[0].ScreenSampler.Height / 4 - indexStringSize.Y / 2;
                    spriteBatch.DrawString(this.settings.WorldIndexFont, indexString, indexPosition, this.worldIndexColor);

                    // Draw quote
                    if (this.worldQuoteLines != null)
                    {
                        float quoteHeight = this.settings.WorldQuoteFont.MeasureString("W").Y;
                        Vector2 quoteStartPos = new Vector2();
                        quoteStartPos.Y = this.levels[0].ScreenSampler.Height * 3 / 4 - quoteHeight * this.worldQuoteLines.Length / 2;

                        for (int i = 0; i < this.worldQuoteLines.Length; i++)
                        {
                            Vector2 quoteSize = this.settings.WorldQuoteFont.MeasureString(this.worldQuoteLines[i]);
                            quoteStartPos.X = (this.levels[0].ScreenSampler.Width - quoteSize.X) / 2;

                            spriteBatch.DrawString(this.settings.WorldQuoteFont,
                                                   this.worldQuoteLines[i],
                                                   quoteStartPos,
                                                   this.worldQuoteColor);

                            quoteStartPos.Y += quoteHeight;
                        }
                    }

                    // Draw World Sprite
                    Vector2 spritePos = new Vector2();
                    spritePos.X = (this.levels[0].ScreenSampler.Width - this.worldSprite.SourceRectangle.Width) / 2;
                    spritePos.Y = (this.levels[0].ScreenSampler.Height - this.worldSprite.SourceRectangle.Height) / 2;

                    spriteBatch.Draw(this.worldSprite.Texture,
                                     spritePos,
                                     this.worldSprite.SourceRectangle,
                                     Color.White);
                    break;
                case WorldState.PlayingLevel:
                    this.levels[this.levelIndex].Draw(spriteBatch);
                    break;
                case WorldState.Finished:
                    spriteBatch.DrawString(this.settings.WorldQuoteFont, "World Finished", new Vector2(160, 200), Color.White);
                    break;
            }
        }

        #endregion

        #region Private methods

        private void ChangeState()
        {
            switch (this.nextState)
            {
                case WorldState.Start:
                    break;
                case WorldState.PlayingLevel:
                    if (this.levels.Count == 0)
                        this.nextState = WorldState.Finished;
                    else
                        this.levels[levelIndex].Reset();
                    break;
                case WorldState.Finished:
                    break;
            }

            this.state = this.nextState;
        }

        #endregion

        public void ResetLevel()
        {
            if (this.levelIndex < this.levels.Count)
                this.levels[this.levelIndex].Reset();
        }

        #region Session management

        public void NewSession()
        {
            this.levelIndex = 0;
            this.levels[this.levelIndex].Reset();

            this.state = this.nextState = WorldState.Start;
        }

        public void LoadSession(GameSession session)
        {
            this.levelIndex = session.LevelIndex;
            this.levels[this.levelIndex].LoadSession(session);

            this.state = this.nextState = WorldState.PlayingLevel;
        }

        public void SaveSession(GameSession session)
        {
            session.LevelIndex = this.levelIndex;
            this.levels[this.levelIndex].SaveSession(session);
        }

        #endregion
    }
}
