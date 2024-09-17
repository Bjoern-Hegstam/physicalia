using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Collision;

namespace XNALibrary.TileEngine;

public class TileEngine
{
    /// <summary>
    /// Default length of the sides of a tile.
    /// </summary>
    private const int DefaultTileSide = 32;

    /// <summary>
    /// Default value for the tile map
    /// </summary>
    private const int DefaultTileValue = -1;

    /// <summary>
    /// TileLibrary with the Tiles to use.
    /// </summary>
    private readonly ITileLibrary _tileLibrary;

    /// <summary>
    /// File path to the xml file defining the tiles and the tile map
    /// to be used by the engine.
    /// </summary>
    private String _xmlTilePath;

    /// <summary>
    /// The map used by the engine for drawing the correct tiles.
    /// </summary>
    private int[,] _tileMap;

    /// <summary>
    /// The width of an individual tile.
    /// </summary>
    private int _tileWidth;

    /// <summary>
    /// The height of an individual tile.
    /// </summary>
    private int _tileHeight;

    /// <summary>
    /// The threshold used when determining whether a collision has taken place.
    /// Measured in pixels.
    /// </summary>
    private int _collisionThreshold = 10;

    /// <summary>
    /// The width of the tile map in units.
    /// </summary>
    private int _width;

    /// <summary>
    /// The height of the tile map in units.
    /// </summary>
    private int _height;

    /// <summary>
    /// Gets & Sets the values of the engine's tile map.
    /// </summary>
    /// <param name="x">X-position of the tile, measured in units.</param>
    /// <param name="y">Y-position of the tile, measured in units.</param>
    public int this[int x, int y]
    {
        get
        {
            if (x >= 0 && x < _width &&
                y >= 0 && y < _height)
                return _tileMap[x, y];
            else
                throw new ArgumentException("One or more arguments are invalid!");
        }

        set
        {
            if (_tileLibrary.ContainsKey(value) &&
                x >= 0 && x < _width &&
                y >= 0 && y < _height)
                _tileMap[x, y] = value;
            else
                throw new ArgumentException("One or more arguments are invalid!");
        }
    }

    /// <summary>
    /// Gets & Sets the maximum distance at which the engine will detect a collision.
    /// Measured in units
    /// </summary>
    public int CollisionThreshold
    {
        get => _collisionThreshold;
        set => _collisionThreshold = value;
    }

    /// <summary>
    /// Sets the path to the Xml file defining the Tiles and the tile map
    /// to be loaded by the Engine when TileEngine.LoadXml is called.
    /// </summary>
    public String XmlPath
    {
        set => _xmlTilePath = value;
    }

    /// <summary>
    /// Gets and Sets the length of the side of a single Tile. If the TileWidth
    /// and TileHeight are set to different values, the greatest of these
    /// is returned.
    /// </summary>
    public int TileSide
    {
        get => _tileWidth > _tileHeight ? _tileWidth : _tileHeight;
        set
        {
            if (value <= 0)
                throw new ArgumentException("Value cannot be 0!");

            _tileWidth = _tileHeight = value;
        }
    }

    /// <summary>
    /// Gets and Sets the width of a Tile.
    /// </summary>
    public int TileWidth
    {
        get => _tileWidth;
        set
        {
            if (value <= 0)
                throw new ArgumentException("Value cannot be 0!");

            _tileWidth = value;
        }
    }

    /// <summary>
    /// Gets and Sets the height of a Tile.
    /// </summary>
    public int TileHeight
    {
        get => _tileHeight;
        set
        {
            if (value <= 0)
                throw new ArgumentException("Value cannot be 0!");

            _tileHeight = value;
        }
    }

    /// <summary>
    /// The width of the tile map, measured in units.
    /// </summary>
    public int Width
    {
        get => _width;
        set => _width = value;
    }

    /// <summary>
    /// The height of the tile map, measured in units.
    /// </summary>
    public int Height
    {
        get => _height;
        set => _height = value;
    }

    /// <summary>
    /// Creates a new TileEngine
    /// </summary>
    /// <param name="tileLibrary">TileLibrary to use.</param>
    /// <param name="width">Width of the tile map in number of tiles.</param>
    /// <param name="height">Height of the tile map in number of tiles.</param>
    public TileEngine(ITileLibrary tileLibrary, int width, int height)
    {
        _tileLibrary = tileLibrary;

        _width = width;
        _height = height;

        _tileWidth = _tileHeight = DefaultTileSide;

        InitializeTileMap(_width, _height);
    }

    /// <summary>
    /// Initializes the values of the tile map to DEFAULT_TILE_VALUE.
    /// </summary>
    /// <param name="width">Width of the tile map in units.</param>
    /// <param name="height">Height of the tile map in units.</param>
    private void InitializeTileMap(int width, int height)
    {
        _tileMap = new int[width, height];

        // Set the new dimensions of the tilemap
        _width = width;
        _height = height;

        // Set all values in the tile map to DEFAULT_TILE_VALUE
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                _tileMap[x, y] = DefaultTileValue;
            }
        }
    }

    /// <summary>
    /// Conforms the tile map to a new width.
    /// </summary>
    /// <param name="width">New width of the map.</param>
    private void ChangeWidth(int width)
    {
        // Make sure a tile map exist
        if (_tileMap == null)
            return;

        // Create a new temporary map
        int[,] newMap = new int[width, _height];

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Copy all old values over to the new map
                if (x < _width)
                    newMap[x, y] = _tileMap[x, y];
                // The new tile cells are filled with the default value
                else
                    newMap[x, y] = DefaultTileValue;
            }
        }

        // Set the new width
        _width = width;

        // Store a reference to the new map
        _tileMap = newMap;
    }

    /// <summary>
    /// Conforms the tile map to a new width.
    /// </summary>
    /// <param name="width">New width of the map.</param>
    private void ChangeHeight(int height)
    {
        // Make sure a tile map exist
        if (_tileMap == null)
            return;

        // Create a new temporary map
        int[,] newMap = new int[_width, height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Copy all old values over to the new map
                if (y < _height)
                    newMap[x, y] = _tileMap[x, y];
                // The new tile cells are filled with the default value
                else
                    newMap[x, y] = DefaultTileValue;
            }
        }

        // Set the new width
        _height = height;

        // Store a reference to the new map
        _tileMap = newMap;
    }

    /// <summary>
    /// Loads the Tiles and the tile map to be used from an Xml file.
    /// </summary>
    /// <param name="spriteLibrary">SpriteLibrary containing the Sprites to
    /// use for the loaded Tiles.</param>
    public void LoadXml()
    {
        LoadXml(_xmlTilePath);
    }

    /// <summary>
    /// Loads the Tiles and the tile map to be used from an Xml file.
    /// </summary>
    /// <param name="reader">XmlReader connected to the file to read from.</param>
    public void LoadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            // TileMap node ?
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "TileMap")
            {
                _width = int.Parse(reader.GetAttribute(0));
                _height = int.Parse(reader.GetAttribute(1));

                InitializeTileMap(_width, _height);
            }

            // (Map) Tile node ?
            if (reader.NodeType == XmlNodeType.Element &&
                reader.LocalName == "Tile")
            {
                int x = int.Parse(reader.GetAttribute(0));
                int y = int.Parse(reader.GetAttribute(1));
                int tileKey = int.Parse(reader.GetAttribute(2));

                _tileMap[x, y] = tileKey;
            }

            // End node?
            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == "TileEngine")
            {
                return;
            }
        }
    }

    /// <summary>
    /// Loads the Tiles and the tile map to be used from an Xml file.
    /// </summary>
    /// <param name="path">Path to the xml file.</param>
    public void LoadXml(String path)
    {
        if (path == null)
            throw new ArgumentNullException("Path to Xml file cannot be null!");

        if (path.Length < 5 || path.Substring(path.Length - 3, 3).ToLower() != "xml")
            throw new ArgumentException("File is not of type '.xml'");

        XmlReaderSettings settings = new XmlReaderSettings();
        settings.CloseInput = true;
        settings.IgnoreComments = true;
        settings.IgnoreWhitespace = true;
        settings.CheckCharacters = true;

        using XmlReader reader = XmlReader.Create(path, settings);
        LoadXml(reader);
    }

    /// <summary>
    /// Checks whether a collision has occured between a ICollisionObject and
    /// a Tile, and takes correct measurements if so has happened. For effiency
    /// only the tiles overlapping with the object are examined.
    /// </summary>
    /// <param name="collisionObjects">The objects to check.</param>
    public void CheckCollisions(ICollisionObject[] collisionObjects)
    {
        // See if the object can take damage
        // Only examine tiles overlapping with the object
        // Control whether a collision has occured based on the objects velocity
        // and the collisionThreshold

        // Call the object's OnCollision method
        // Damage the object if the colliding tile says so
        // Correct the position of the colliding object

        // Set the object's velocity to zero on collision

        // Loop through the passed in collision objects
        foreach (ICollisionObject collObj in collisionObjects)
        {
            CheckCollision(collObj);
        }
    }

    /// <summary>
    /// Checks whether a collision has occured between a ICollisionObject and
    /// a Tile, and takes correct measurements if so has happened. For effiency
    /// only the tiles overlapping with the object are examined.
    /// </summary>
    /// <param name="collObject">The object to check.</param>
    public void CheckCollision(ICollisionObject collObject)
    {
        // Get the positions of the Tiles to check
        int xMin = (int)((collObject.Position.X - collObject.Origin.X + collObject.CollisionBox.X) / TileWidth);
        int xMax = (int)((collObject.Position.X - collObject.Origin.X + collObject.CollisionBox.X +
                          collObject.CollisionBox.Width) / TileWidth);

        int yMin = (int)((collObject.Position.Y - collObject.Origin.Y + collObject.CollisionBox.Y) / TileHeight);
        int yMax = (int)((collObject.Position.Y - collObject.Origin.Y + collObject.CollisionBox.Y +
                          collObject.CollisionBox.Height) / TileHeight);

        // Loop through the Tiles
        Tile tile = null;

        // Make sure all numbers are within the bounds of the map
        if (xMin < 0) xMin = 0;
        if (xMax >= _tileMap.GetLength(0)) xMax = _tileMap.GetLength(0) - 1;

        if (yMin < 0) yMin = 0;
        if (yMax >= _tileMap.GetLength(1)) yMax = _tileMap.GetLength(1) - 1;

        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                // No Tile at position?
                if (_tileMap[x, y] == DefaultTileValue)
                    continue;

                tile = _tileLibrary.GetTile(_tileMap[x, y]);
                //tile = this.tiles[this.tileMap[x, y]];

                // Don't check the tile if it can't give damage or
                // be collided with
                if (tile.CollisionSides == 0 && !tile.GivesDamage)
                    continue;

                // Check the tiles' collision sides for collisions
                Vector2 position = collObject.Position - collObject.Origin;
                Vector2 velocity = collObject.Velocity;
                Rectangle collBox = collObject.CollisionBox;
                float rightMargin = collObject.Width - collBox.X - collBox.Width;
                float leftMargin = collBox.X;
                float topMargin = collBox.Y;
                float bottomMargin = collObject.Height - collBox.Y - collBox.Height;

                // Collide sides
                BoxSide collisions = 0;

                if (velocity.X > 0 &&
                    position.X + collBox.X + collBox.Width >= (x * _tileWidth + tile.CollisionBox.X) &&
                    position.X + collBox.X + collBox.Width <=
                    (x * _tileWidth + tile.CollisionBox.X + _collisionThreshold))
                    collisions |= BoxSide.Left;

                if (velocity.Y > 0 &&
                    position.Y + collBox.Y + collBox.Height >= (y * _tileHeight + tile.CollisionBox.Y) &&
                    position.Y + collBox.Y + collBox.Height <=
                    (y * _tileHeight + tile.CollisionBox.Y + _collisionThreshold))
                    collisions |= BoxSide.Top;

                if (velocity.X < 0 &&
                    position.X + collBox.X <= (x * _tileWidth + tile.CollisionBox.X + tile.CollisionBox.Width) &&
                    position.X + collBox.X >= (x * _tileWidth + tile.CollisionBox.X + tile.CollisionBox.Width -
                                               _collisionThreshold))
                    collisions |= BoxSide.Right;

                if (velocity.Y < 0 &&
                    position.Y + collBox.Y <= (y * _tileHeight + tile.CollisionBox.Y + tile.CollisionBox.Height) &&
                    position.Y + collBox.Y >= (y * _tileHeight + tile.CollisionBox.Y + tile.CollisionBox.Height -
                                               _collisionThreshold))
                    collisions |= BoxSide.Bottom;


                // Check that the object is within the area where
                // it can collide with the side

                // Left
                if ((collisions & BoxSide.Left) != 0)
                {
                    // Is the object within the collision area
                    // Object top less than tile bottom and
                    // object bottom greater than tile top
                    if ((tile.CollisionSides & BoxSide.Left) != 0 &&
                        position.Y + collBox.Y < y * _tileHeight + tile.CollisionBox.Y + tile.CollisionBox.Height &&
                        position.Y + collBox.Y + collBox.Height > y * _tileHeight + tile.CollisionBox.Y)
                    {
                        position.X = x * _tileWidth + tile.CollisionBox.X - collBox.X - collBox.Width;
                        velocity.X = 0;
                    }
                    else
                        collisions -= BoxSide.Left;
                }

                // Top
                if ((collisions & BoxSide.Top) != 0)
                {
                    // Is the object within the collision threshold
                    if ((tile.CollisionSides & BoxSide.Top) != 0 &&
                        position.X + collBox.X < x * _tileWidth + tile.CollisionBox.X + tile.CollisionBox.Width &&
                        position.X + collBox.X + collBox.Width > x * _tileWidth + tile.CollisionBox.X)
                    {
                        position.Y = y * _tileWidth + tile.CollisionBox.Y - collBox.Y - collBox.Height;
                        velocity.Y = 0;
                    }
                    else
                        collisions -= BoxSide.Top;
                }

                // Right
                if ((collisions & BoxSide.Right) != 0)
                {
                    // Is the object within the collision threshold
                    if ((tile.CollisionSides & BoxSide.Right) != 0 &&
                        position.Y + collBox.Y < y * _tileHeight + tile.CollisionBox.Y + tile.CollisionBox.Height &&
                        position.Y + collBox.Y + collBox.Height > y * _tileHeight + tile.CollisionBox.Y)
                    {
                        position.X = x * _tileWidth + tile.CollisionBox.X + tile.CollisionBox.Width - collBox.X;
                        velocity.X = 0;
                    }
                    else
                        collisions -= BoxSide.Right;
                }

                // Bottom
                if ((collisions & BoxSide.Bottom) != 0)
                {
                    // Is the object within the collision threshold
                    if ((tile.CollisionSides & BoxSide.Bottom) != 0 &&
                        position.X + collBox.X < x * _tileWidth + tile.CollisionBox.X + tile.CollisionBox.Width &&
                        position.X + collBox.X + collBox.Width > x * _tileWidth + tile.CollisionBox.X)
                    {
                        position.Y = y * _tileWidth + tile.CollisionBox.Y + tile.CollisionBox.Height - collBox.Y;
                        velocity.Y = 0;
                    }
                    else
                        collisions -= BoxSide.Bottom;
                }

                // Were there any collisions?
                if (collisions != 0)
                {
                    // See which sides of the object that collided
                    BoxSide objectSides = 0;
                    if ((collisions & BoxSide.Left) != 0) objectSides |= BoxSide.Right;
                    if ((collisions & BoxSide.Right) != 0) objectSides |= BoxSide.Left;
                    if ((collisions & BoxSide.Top) != 0) objectSides |= BoxSide.Bottom;
                    if ((collisions & BoxSide.Bottom) != 0) objectSides |= BoxSide.Top;

                    // Allert object of the collision
                    if (collObject.CanCollide)
                        collObject.OnCollision(tile, objectSides, position + collObject.Origin, velocity);

                    // Damage the object if it can take damage
                    if (collObject.CanTakeDamage && tile.GivesDamage)
                        collObject.TakeDamage(tile.DamageLevel);
                }
            }
        }
    }

    /// <summary>
    /// Draws the currently visible tiles on screen.
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to use for drawing.</param>
    /// <param name="positionOffset">The positional offset to use for
    /// drawing the correct tiles.</param>
    public void Draw(SpriteBatch spriteBatch, Vector2 positionOffset)
    {
        // Calculate the unit position if the top left corner
        int xBase = (int)(positionOffset.X / _tileWidth);
        int yBase = (int)(positionOffset.Y / _tileHeight);

        Tile tile;
        Vector2 position = new Vector2();

        for (int yi = yBase; yi < _height; yi++)
        {
            for (int xi = xBase; xi < _width; xi++)
            {
                if (_tileMap[xi, yi] == DefaultTileValue)
                    continue;

                // Get tile and sprite
                tile = _tileLibrary.GetTile(_tileMap[xi, yi]);
                // Calculate position
                position.X = xi * _tileWidth - positionOffset.X;
                position.Y = yi * _tileHeight - positionOffset.Y;

                // Draw the tile
                //spriteBatch.Draw(this.spriteLibrary.Textures[sprite.TextureKey],
                spriteBatch.Draw(tile.Texture,
                    position,
                    tile.SourceRectangle,
                    Color.White);
            }
        }
    }
}