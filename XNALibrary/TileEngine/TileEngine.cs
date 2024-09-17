using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Collision;

namespace XNALibrary.TileEngine;

public class TileEngine
{
    private const int DefaultTileSidePx = 32;
    private const int EmptyTile = -1;

    private readonly TileLibrary _tileLibrary;

    private int[,] _tileMap;

    private readonly int _tileWidthPx;
    private readonly int _tileHeightPx;

    private const int CollisionThresholdPx = 10;

    public int Width => _tileMap.GetLength(0);
    public int Height => _tileMap.GetLength(1);

    public TileEngine(TileLibrary tileLibrary, int width, int height)
    {
        _tileLibrary = tileLibrary;
        _tileWidthPx = _tileHeightPx = DefaultTileSidePx;

        _tileMap = new int[width, height];
        ClearTileMap();
    }

    private void ClearTileMap()
    {
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                _tileMap[x, y] = EmptyTile;
            }
        }
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
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "TileMap" })
            {
                var width = int.Parse(reader.GetAttribute(0));
                var height = int.Parse(reader.GetAttribute(1));

                _tileMap = new int[width, height];
                ClearTileMap();
            }

            // (Map) Tile node ?
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Tile" })
            {
                int x = int.Parse(reader.GetAttribute(0));
                int y = int.Parse(reader.GetAttribute(1));
                int tileKey = int.Parse(reader.GetAttribute(2));

                _tileMap[x, y] = tileKey;
            }

            // End node?
            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "TileEngine" })
            {
                return;
            }
        }
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
        var xMin = Math.Max(
            (int)((collObject.Position.X - collObject.Origin.X + collObject.CollisionBox.X) / _tileWidthPx),
            0
        );
        var xMax = Math.Min(
            (int)((collObject.Position.X - collObject.Origin.X + collObject.CollisionBox.X +
                   collObject.CollisionBox.Width) / _tileWidthPx),
            Width - 1);

        var yMin = Math.Max(
            (int)((collObject.Position.Y - collObject.Origin.Y + collObject.CollisionBox.Y) / _tileHeightPx),
            0
        );
        var yMax = Math.Min(
            (int)((collObject.Position.Y - collObject.Origin.Y + collObject.CollisionBox.Y +
                   collObject.CollisionBox.Height) / _tileHeightPx),
            Height - 1
        );
        
        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                if (_tileMap[x, y] == EmptyTile)
                {
                    continue;
                }

                Tile tile = _tileLibrary.GetTile(_tileMap[x, y]);

                // Don't check the tile if it can't give damage or
                // be collided with
                if (tile is { CollisionSides: 0, GivesDamage: false })
                {
                    continue;
                }

                // Check the tiles' collision sides for collisions
                Vector2 position = collObject.Position - collObject.Origin;
                Vector2 velocity = collObject.Velocity;
                Rectangle collBox = collObject.CollisionBox;

                // Collide sides
                BoxSide collisions = 0;

                if (velocity.X > 0 &&
                    position.X + collBox.X + collBox.Width >= (x * _tileWidthPx + tile.CollisionBox.X) &&
                    position.X + collBox.X + collBox.Width <=
                    (x * _tileWidthPx + tile.CollisionBox.X + CollisionThresholdPx))
                {
                    collisions |= BoxSide.Left;
                }

                if (velocity.Y > 0 &&
                    position.Y + collBox.Y + collBox.Height >= (y * _tileHeightPx + tile.CollisionBox.Y) &&
                    position.Y + collBox.Y + collBox.Height <=
                    (y * _tileHeightPx + tile.CollisionBox.Y + CollisionThresholdPx))
                {
                    collisions |= BoxSide.Top;
                }

                if (velocity.X < 0 &&
                    position.X + collBox.X <= (x * _tileWidthPx + tile.CollisionBox.X + tile.CollisionBox.Width) &&
                    position.X + collBox.X >= (x * _tileWidthPx + tile.CollisionBox.X + tile.CollisionBox.Width -
                                               CollisionThresholdPx))
                {
                    collisions |= BoxSide.Right;
                }

                if (velocity.Y < 0 &&
                    position.Y + collBox.Y <= (y * _tileHeightPx + tile.CollisionBox.Y + tile.CollisionBox.Height) &&
                    position.Y + collBox.Y >= (y * _tileHeightPx + tile.CollisionBox.Y + tile.CollisionBox.Height -
                                               CollisionThresholdPx))
                {
                    collisions |= BoxSide.Bottom;
                }

                // Check that the object is within the area where
                // it can collide with the side

                // Left
                if ((collisions & BoxSide.Left) != 0)
                {
                    // Is the object within the collision area
                    // Object top less than tile bottom and
                    // object bottom greater than tile top
                    if ((tile.CollisionSides & BoxSide.Left) != 0 &&
                        position.Y + collBox.Y < y * _tileHeightPx + tile.CollisionBox.Y + tile.CollisionBox.Height &&
                        position.Y + collBox.Y + collBox.Height > y * _tileHeightPx + tile.CollisionBox.Y)
                    {
                        position.X = x * _tileWidthPx + tile.CollisionBox.X - collBox.X - collBox.Width;
                        velocity.X = 0;
                    }
                    else
                    {
                        collisions -= BoxSide.Left;
                    }
                }

                // Top
                if ((collisions & BoxSide.Top) != 0)
                {
                    // Is the object within the collision threshold
                    if ((tile.CollisionSides & BoxSide.Top) != 0 &&
                        position.X + collBox.X < x * _tileWidthPx + tile.CollisionBox.X + tile.CollisionBox.Width &&
                        position.X + collBox.X + collBox.Width > x * _tileWidthPx + tile.CollisionBox.X)
                    {
                        position.Y = y * _tileWidthPx + tile.CollisionBox.Y - collBox.Y - collBox.Height;
                        velocity.Y = 0;
                    }
                    else
                    {
                        collisions -= BoxSide.Top;
                    }
                }

                // Right
                if ((collisions & BoxSide.Right) != 0)
                {
                    // Is the object within the collision threshold
                    if ((tile.CollisionSides & BoxSide.Right) != 0 &&
                        position.Y + collBox.Y < y * _tileHeightPx + tile.CollisionBox.Y + tile.CollisionBox.Height &&
                        position.Y + collBox.Y + collBox.Height > y * _tileHeightPx + tile.CollisionBox.Y)
                    {
                        position.X = x * _tileWidthPx + tile.CollisionBox.X + tile.CollisionBox.Width - collBox.X;
                        velocity.X = 0;
                    }
                    else
                    {
                        collisions -= BoxSide.Right;
                    }
                }

                // Bottom
                if ((collisions & BoxSide.Bottom) != 0)
                {
                    // Is the object within the collision threshold
                    if ((tile.CollisionSides & BoxSide.Bottom) != 0 &&
                        position.X + collBox.X < x * _tileWidthPx + tile.CollisionBox.X + tile.CollisionBox.Width &&
                        position.X + collBox.X + collBox.Width > x * _tileWidthPx + tile.CollisionBox.X)
                    {
                        position.Y = y * _tileWidthPx + tile.CollisionBox.Y + tile.CollisionBox.Height - collBox.Y;
                        velocity.Y = 0;
                    }
                    else
                    {
                        collisions -= BoxSide.Bottom;
                    }
                }

                if (collisions != 0)
                {
                    BoxSide objectSides = 0;
                    if ((collisions & BoxSide.Left) != 0)
                    {
                        objectSides |= BoxSide.Right;
                    }

                    if ((collisions & BoxSide.Right) != 0)
                    {
                        objectSides |= BoxSide.Left;
                    }

                    if ((collisions & BoxSide.Top) != 0)
                    {
                        objectSides |= BoxSide.Bottom;
                    }

                    if ((collisions & BoxSide.Bottom) != 0)
                    {
                        objectSides |= BoxSide.Top;
                    }

                    if (collObject.CanCollide)
                    {
                        collObject.OnCollision(tile, objectSides, position + collObject.Origin, velocity);
                    }

                    if (collObject.CanTakeDamage && tile.GivesDamage)
                    {
                        collObject.TakeDamage(tile.DamageLevel);
                    }
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
        var xBase = (int)(positionOffset.X / _tileWidthPx);
        var yBase = (int)(positionOffset.Y / _tileHeightPx);

        Tile tile;
        var position = new Vector2();

        for (int yi = yBase; yi < Height; yi++)
        {
            for (int xi = xBase; xi < Width; xi++)
            {
                if (_tileMap[xi, yi] == EmptyTile)
                {
                    continue;
                }

                // Get tile and sprite
                tile = _tileLibrary.GetTile(_tileMap[xi, yi]);
                // Calculate position
                position.X = xi * _tileWidthPx - positionOffset.X;
                position.Y = yi * _tileHeightPx - positionOffset.Y;

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