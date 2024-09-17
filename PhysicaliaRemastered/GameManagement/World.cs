using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Actors;
using PhysicaliaRemastered.Input;
using XNALibrary.Sprites;
using XNALibrary.TileEngine;

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
    private const string LevelPath = "Content/GameData/Worlds/Levels/";

    private readonly ISettings _settings;

    private readonly List<Level> _levels;
    private int _levelIndex;

    // Presentation
    private string[] _worldQuoteLines;
    private readonly Color _worldIndexColor;
    private Color _worldQuoteColor;
    private Sprite _worldSprite;

    // TODO: Add fields for keeping the text to view when finished

    private WorldState _nextState;

    private readonly Game _game;
    private readonly Player _player;

    public int WorldIndex { get; set; }

    public WorldState State { get; private set; }

    public LevelState LevelState => _levels[_levelIndex].State;

    public World(Game game, Player player)
    {
        _game = game;
        _player = player;

        _settings = (ISettings)_game.Services.GetService(typeof(ISettings));

        _levels = new List<Level>();
        _levelIndex = -1;

        _nextState = State = WorldState.Start;

        WorldIndex = -1;
        _worldIndexColor = Color.White;
        _worldQuoteLines = null;
        _worldQuoteColor = Color.White;
    }

    public void LoadXml(string path, ITileLibrary tileLibrary, ISpriteLibrary spriteLibrary)
    {
        XmlReaderSettings readerSettings = new XmlReaderSettings();
        readerSettings.IgnoreComments = true;
        readerSettings.IgnoreProcessingInstructions = true;
        readerSettings.IgnoreWhitespace = true;

        using XmlReader reader = XmlReader.Create(path, readerSettings);
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
                _worldSprite = spriteLibrary.GetSprite(key);
            }

            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Quote")
            {
                // Read color
                string[] colorValues = reader.GetAttribute("color").Split(' ');

                // Must have all needed value (rgba)
                if (colorValues.Length < 4)
                {
                    continue;
                }

                byte r = byte.Parse(colorValues[0]);
                byte g = byte.Parse(colorValues[1]);
                byte b = byte.Parse(colorValues[2]);
                byte a = byte.Parse(colorValues[3]);

                _worldQuoteColor = new Color(r, b, g, a);

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
                    {
                        quoteBuilder.Append(reader.ReadString());
                    }
                    else
                    {
                        reader.Read();
                    }
                }

                // Store parsed quote
                if (quoteBuilder.Length > 0)
                {
                    quoteLines.Add(quoteBuilder.ToString());
                }

                _worldQuoteLines = quoteLines.ToArray();
            }

            // Read in Levels
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Level")
            {
                // Create and initialize the new Level
                Level level = new Level(_game, _player);

                using (XmlReader levelReader = XmlReader.Create(LevelPath + reader.ReadString(), reader.Settings))
                    level.LoadXml(levelReader, tileLibrary);

                // Store the Level reference
                _levels.Add(level);
                level.LevelIndex = _levels.Count;
                level.WorldIndex = WorldIndex;
            }

            // Read in BossLevels
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "BossLevel")
            {
                BossLevel level = new BossLevel(_game, _player);

                using (XmlReader levelReader = XmlReader.Create(reader.ReadElementContentAsString()))
                    level.LoadXml(levelReader, tileLibrary);

                _levels.Add(level);
                level.LevelIndex = _levels.Count;
            }
        }

        // Set the index of the current level
        if (_levels.Count > 0)
        {
            _levelIndex = 0;
        }
    }

    public void Update(GameTime gameTime)
    {
        switch (State)
        {
            case WorldState.Start:
                if (_settings.InputMap.IsPressed(InputAction.MenuStart))
                {
                    _nextState = WorldState.PlayingLevel;
                }

                break;
            case WorldState.PlayingLevel:
                if (_levels[_levelIndex].State == LevelState.Finished)
                {
                    _levels[_levelIndex].Update(gameTime);

                    if (_settings.InputMap.IsPressed(InputAction.MenuStart))
                    {
                        // Go to next level
                        _levelIndex++;

                        // World is finished if there are no more levels
                        if (_levelIndex >= _levels.Count)
                        {
                            _nextState = WorldState.Finished;
                        }
                        else
                            // Reset the next level
                        {
                            _levels[_levelIndex].Reset();
                        }
                    }
                }
                else if (_levels[_levelIndex].State == LevelState.Dead)
                {
                    _levels[_levelIndex].Update(gameTime);

                    if (_settings.InputMap.IsPressed(InputAction.MenuStart))
                    {
                        _levels[_levelIndex].Reset();
                    }
                }
                else
                    // Update the current level
                {
                    _levels[_levelIndex].Update(gameTime);
                }

                break;
            case WorldState.Finished:
                break;
        }

        while (_nextState != State)
            ChangeState();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        switch (State)
        {
            case WorldState.Start:
                // Write draw world index
                string indexString = "World " + WorldIndex;
                Vector2 indexStringSize = _settings.WorldIndexFont.MeasureString(indexString);
                Vector2 indexPosition = new Vector2();
                indexPosition.X = (_levels[0].ScreenSampler.Width - indexStringSize.X) / 2;
                indexPosition.Y = _levels[0].ScreenSampler.Height / 4 - indexStringSize.Y / 2;
                spriteBatch.DrawString(_settings.WorldIndexFont, indexString, indexPosition, _worldIndexColor);

                // Draw quote
                if (_worldQuoteLines != null)
                {
                    float quoteHeight = _settings.WorldQuoteFont.MeasureString("W").Y;
                    Vector2 quoteStartPos = new Vector2();
                    quoteStartPos.Y = _levels[0].ScreenSampler.Height * 3 / 4 -
                                      quoteHeight * _worldQuoteLines.Length / 2;

                    for (int i = 0; i < _worldQuoteLines.Length; i++)
                    {
                        Vector2 quoteSize = _settings.WorldQuoteFont.MeasureString(_worldQuoteLines[i]);
                        quoteStartPos.X = (_levels[0].ScreenSampler.Width - quoteSize.X) / 2;

                        spriteBatch.DrawString(_settings.WorldQuoteFont,
                            _worldQuoteLines[i],
                            quoteStartPos,
                            _worldQuoteColor);

                        quoteStartPos.Y += quoteHeight;
                    }
                }

                // Draw World Sprite
                Vector2 spritePos = new Vector2();
                spritePos.X = (_levels[0].ScreenSampler.Width - _worldSprite.SourceRectangle.Width) / 2;
                spritePos.Y = (_levels[0].ScreenSampler.Height - _worldSprite.SourceRectangle.Height) / 2;

                spriteBatch.Draw(_worldSprite.Texture,
                    spritePos,
                    _worldSprite.SourceRectangle,
                    Color.White);
                break;
            case WorldState.PlayingLevel:
                _levels[_levelIndex].Draw(spriteBatch);
                break;
            case WorldState.Finished:
                spriteBatch.DrawString(_settings.WorldQuoteFont, "World Finished", new Vector2(160, 200), Color.White);
                break;
        }
    }

    private void ChangeState()
    {
        switch (_nextState)
        {
            case WorldState.Start:
                break;
            case WorldState.PlayingLevel:
                if (_levels.Count == 0)
                {
                    _nextState = WorldState.Finished;
                }
                else
                {
                    _levels[_levelIndex].Reset();
                }

                break;
            case WorldState.Finished:
                break;
        }

        State = _nextState;
    }

    public void ResetLevel()
    {
        if (_levelIndex < _levels.Count)
        {
            _levels[_levelIndex].Reset();
        }
    }

    public void NewSession()
    {
        _levelIndex = 0;
        _levels[_levelIndex].Reset();

        State = _nextState = WorldState.Start;
    }

    public void LoadSession(GameSession session)
    {
        _levelIndex = session.LevelIndex;
        _levels[_levelIndex].LoadSession(session);

        State = _nextState = WorldState.PlayingLevel;
    }

    public void SaveSession(GameSession session)
    {
        session.LevelIndex = _levelIndex;
        _levels[_levelIndex].SaveSession(session);
    }
}