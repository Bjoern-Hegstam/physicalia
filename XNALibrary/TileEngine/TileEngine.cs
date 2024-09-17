using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Collision;

namespace XNALibrary.TileEngine;

public class TileEngine
{
    private const int DefaultTileSidePx = 32;
    private const int EmptyTile = -1;
    private const int CollisionThresholdPx = 10;

    private readonly TileLibrary _tileLibrary;

    private readonly int _tileWidthPx;
    private readonly int _tileHeightPx;

    private int[,] _tileMap;
    private int Width => _tileMap.GetLength(0);
    private int Height => _tileMap.GetLength(1);

    public TileEngine(TileLibrary tileLibrary, int width, int height)
    {
        _tileLibrary = tileLibrary;
        _tileMap = new int[width, height];

        _tileWidthPx = _tileHeightPx = DefaultTileSidePx;

        ResetTileMap();
    }

    private void ResetTileMap()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                _tileMap[x, y] = EmptyTile;
            }
        }
    }

    public void LoadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "TileMap" })
            { 
                int width = int.Parse(reader.GetAttribute(0));
                int height = int.Parse(reader.GetAttribute(1));

                _tileMap = new int[width, height];
                ResetTileMap();
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Tile" })
            {
                int x = int.Parse(reader.GetAttribute(0));
                int y = int.Parse(reader.GetAttribute(1));
                int tileKey = int.Parse(reader.GetAttribute(2));

                _tileMap[x, y] = tileKey;
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "TileEngine" })
            {
                return;
            }
        }
    }

    public void CheckCollisions(ICollisionObject[] collisionObjects)
    {
        foreach (ICollisionObject collObj in collisionObjects)
        {
            CheckCollision(collObj);
        }
    }

    public void CheckCollision(ICollisionObject collObject)
    {
        // Get the positions of the Tiles to check
        var xMin = (int)((collObject.Position.X - collObject.Origin.X + collObject.CollisionBox.X) / _tileWidthPx);
        var xMax = (int)((collObject.Position.X - collObject.Origin.X + collObject.CollisionBox.X +
                          collObject.CollisionBox.Width) / _tileWidthPx);

        var yMin = (int)((collObject.Position.Y - collObject.Origin.Y + collObject.CollisionBox.Y) / _tileHeightPx);
        var yMax = (int)((collObject.Position.Y - collObject.Origin.Y + collObject.CollisionBox.Y +
                          collObject.CollisionBox.Height) / _tileHeightPx);

        // Loop through the Tiles

        // Make sure all numbers are within the bounds of the map
        if (xMin < 0)
        {
            xMin = 0;
        }

        if (xMax >= _tileMap.GetLength(0))
        {
            xMax = _tileMap.GetLength(0) - 1;
        }

        if (yMin < 0)
        {
            yMin = 0;
        }

        if (yMax >= _tileMap.GetLength(1))
        {
            yMax = _tileMap.GetLength(1) - 1;
        }

        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                // No Tile at position?
                if (_tileMap[x, y] == EmptyTile)
                {
                    continue;
                }

                Tile tile = _tileLibrary.GetTile(_tileMap[x, y]);

                // Don't check the tile if it can't give damage or
                // be collided with
                if (tile is { GivesDamage: false } or { CollisionSides: 0 })
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

                // Were there any collisions?
                if (collisions != 0)
                {
                    // See which sides of the object that collided
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

                    // Allert object of the collision
                    if (collObject.CanCollide)
                    {
                        collObject.OnCollision(tile, objectSides, position + collObject.Origin, velocity);
                    }

                    // Damage the object if it can take damage
                    if (collObject.CanTakeDamage && tile.GivesDamage)
                    {
                        collObject.TakeDamage(tile.DamageLevel);
                    }
                }
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 positionOffset)
    {
        // Calculate the unit position if the top left corner
        var xBase = (int)(positionOffset.X / _tileWidthPx);
        var yBase = (int)(positionOffset.Y / _tileHeightPx);

        var position = new Vector2();

        for (int yi = yBase; yi < Height; yi++)
        {
            for (int xi = xBase; xi < Width; xi++)
            {
                if (_tileMap[xi, yi] == EmptyTile)
                {
                    continue;
                }

                Tile tile = _tileLibrary.GetTile(_tileMap[xi, yi]);
                
                position.X = xi * _tileWidthPx - positionOffset.X;
                position.Y = yi * _tileHeightPx - positionOffset.Y;

                // Draw the tile
                spriteBatch.Draw(tile.Texture,
                    position,
                    tile.SourceRectangle,
                    Color.White);
            }
        }
    }
}