using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.Actors;
using PhysicaliaRemastered.Input;
using XNALibrary;
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
public class World(Game game, Player player)
{
    private const string LevelPath = "Content/GameData/Worlds/Levels/";
    
    private readonly List<Level> _levels = [];
    private int _levelIndex = -1;

    // Presentation
    private string[] _worldQuoteLines = [];
    private readonly Color _worldIndexColor = Color.White;
    private Color _worldQuoteColor = Color.White;
    private Sprite? _worldSprite;

    private Settings Settings => game.Services.GetService<Settings>();
    
    public int WorldIndex { get; set; } = -1;

    public WorldState State { get; private set; } = WorldState.Start;
    private WorldState _nextState = WorldState.Start;

    public LevelState LevelState => _levels[_levelIndex].State;

    public void LoadXml(string path, TileLibrary tileLibrary, SpriteLibrary spriteLibrary)
    {
        var readerSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true
        };

        using var reader = XmlReader.Create(path, readerSettings);
        LoadXml(reader, tileLibrary, spriteLibrary);
    }

    public void LoadXml(XmlReader reader, TileLibrary tileLibrary, SpriteLibrary spriteLibrary)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "StartSprite" })
            {
                SpriteId spriteId =
                    new SpriteId(reader.GetAttribute("spriteId") ?? throw new ResourceLoadException());
                _worldSprite = spriteLibrary.GetSprite(spriteId);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Quote" })
            {
                // Read color
                string colorString = reader.GetAttribute("color") ?? throw new ResourceLoadException();
                string[] colorValues = colorString.Split(' ');

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
                var quoteBuilder = new StringBuilder();
                List<string> quoteLines = [];

                // Read in any special characters
                while (!(reader is { NodeType: XmlNodeType.EndElement, LocalName: "Quote" }))
                {
                    if (reader.LocalName == "br")
                    {
                        quoteLines.Add(quoteBuilder.ToString());
                        quoteBuilder = new StringBuilder();

                        reader.Read();
                    }
                    else if (reader is { NodeType: XmlNodeType.Element, LocalName: "Char" })
                    {
                        int charNum = int.Parse(reader.ReadString());
                        var character = (char)charNum;
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
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Level" })
            {
                // Create and initialize the new Level
                var level = new Level(game, player);

                using (var levelReader = XmlReader.Create(LevelPath + reader.ReadString(), reader.Settings))
                    level.LoadXml(levelReader, tileLibrary);

                // Store the Level reference
                _levels.Add(level);
                level.LevelIndex = _levels.Count;
                level.WorldIndex = WorldIndex;
            }

            // Read in BossLevels
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "BossLevel" })
            {
                var level = new BossLevel(game, player);

                using (var levelReader = XmlReader.Create(reader.ReadElementContentAsString()))
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
                if (Settings.InputMap.IsPressed(InputAction.MenuStart))
                {
                    _nextState = WorldState.PlayingLevel;
                }

                break;
            case WorldState.PlayingLevel:
                if (_levels[_levelIndex].State == LevelState.Finished)
                {
                    _levels[_levelIndex].Update(gameTime);

                    if (Settings.InputMap.IsPressed(InputAction.MenuStart))
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

                    if (Settings.InputMap.IsPressed(InputAction.MenuStart))
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
        {
            ChangeState();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        switch (State)
        {
            case WorldState.Start:
                // Write draw world index
                string indexString = "World " + WorldIndex;
                Vector2 indexStringSize = Settings.WorldIndexFont!.MeasureString(indexString);
                var indexPosition = new Vector2
                {
                    X = (_levels[0].Viewport.Width - indexStringSize.X) / 2,
                    Y = _levels[0].Viewport.Height / 4f - indexStringSize.Y / 2
                };
                spriteBatch.DrawString(Settings.WorldIndexFont, indexString, indexPosition, _worldIndexColor);

                // Draw quote
                if (_worldQuoteLines.Length > 0)
                {
                    float quoteHeight = Settings.WorldQuoteFont!.MeasureString("W").Y;
                    var quoteStartPos = new Vector2
                    {
                        Y = _levels[0].Viewport.Height * 3f / 4 -
                            quoteHeight * _worldQuoteLines.Length / 2
                    };

                    foreach (string line in _worldQuoteLines)
                    {
                        Vector2 quoteSize = Settings.WorldQuoteFont.MeasureString(line);
                        quoteStartPos.X = (_levels[0].Viewport.Width - quoteSize.X) / 2;

                        spriteBatch.DrawString(
                            Settings.WorldQuoteFont,
                            line,
                            quoteStartPos,
                            _worldQuoteColor
                        );

                        quoteStartPos.Y += quoteHeight;
                    }
                }

                // Draw World Sprite
                var spritePos = new Vector2
                {
                    X = (_levels[0].Viewport.Width - _worldSprite.SourceRectangle.Width) / 2f,
                    Y = (_levels[0].Viewport.Height - _worldSprite.SourceRectangle.Height) / 2f
                };

                spriteBatch.Draw(
                    _worldSprite.Texture,
                    spritePos,
                    _worldSprite.SourceRectangle,
                    Color.White
                );
                break;
            case WorldState.PlayingLevel:
                _levels[_levelIndex].Draw(spriteBatch);
                break;
            case WorldState.Finished:
                spriteBatch.DrawString(Settings.WorldQuoteFont, "World Finished", new Vector2(160, 200), Color.White);
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