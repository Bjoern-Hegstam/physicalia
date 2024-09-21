using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Collision;

namespace XNALibrary.TileEngine;

public class TileEngine(TileLibrary tileLibrary, int width, int height)
{
    private const int DefaultTileSidePx = 32;
    private const int CollisionThresholdPx = 10;

    private const int TileWidthPx = DefaultTileSidePx;
    private const int TileHeightPx = DefaultTileSidePx;

    private readonly Dictionary<Vector2, TileId> _tileMap = new();
    private int Width { get; set; } = width;
    private int Height { get; set; } = height;

    public void LoadXml(XmlReader reader)
    {
        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "TileMap" })
            {
                Width = int.Parse(reader.GetAttribute("width") ?? throw new ResourceLoadException());
                Height = int.Parse(reader.GetAttribute("height") ?? throw new ResourceLoadException());

                _tileMap.Clear();
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Tile" })
            {
                int x = int.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
                int y = int.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());
                TileId tileId = new TileId(reader.GetAttribute("tileId") ?? throw new ResourceLoadException());

                _tileMap[new Vector2(x, y)] = tileId;
            }

            if (reader is { NodeType: XmlNodeType.EndElement, LocalName: "TileEngine" })
            {
                return;
            }
        }
    }

    public void CheckCollisions(IEnumerable<ICollidable> collisionObjects)
    {
        foreach (ICollidable collObj in collisionObjects)
        {
            CheckCollision(collObj);
        }
    }

    public void CheckCollision(ICollidable collObject)
    {
        // Get the positions of the Tiles to check
        var xMin = Math.Max(
            (int)((collObject.Position.X - collObject.Origin.X + collObject.CollisionBox.X) / TileWidthPx),
            0
        );
        var xMax = Math.Min(
            (int)((collObject.Position.X - collObject.Origin.X + collObject.CollisionBox.X +
                   collObject.CollisionBox.Width) / TileWidthPx),
            Width - 1);

        var yMin = Math.Max(
            (int)((collObject.Position.Y - collObject.Origin.Y + collObject.CollisionBox.Y) / TileHeightPx),
            0
        );
        var yMax = Math.Min(
            (int)((collObject.Position.Y - collObject.Origin.Y + collObject.CollisionBox.Y +
                   collObject.CollisionBox.Height) / TileHeightPx),
            Height - 1
        );

        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                if (!_tileMap.TryGetValue(new Vector2(x, y), out TileId tileId))
                {
                    continue;
                }

                Tile tile = tileLibrary.GetTile(tileId);

                if (!tile.CanCollide)
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
                    position.X + collBox.X + collBox.Width >= x * TileWidthPx + tile.CollisionBox.X &&
                    position.X + collBox.X + collBox.Width <=
                    x * TileWidthPx + tile.CollisionBox.X + CollisionThresholdPx)
                {
                    collisions |= BoxSide.Left;
                }

                if (velocity.Y > 0 &&
                    position.Y + collBox.Y + collBox.Height >= y * TileHeightPx + tile.CollisionBox.Y &&
                    position.Y + collBox.Y + collBox.Height <=
                    y * TileHeightPx + tile.CollisionBox.Y + CollisionThresholdPx)
                {
                    collisions |= BoxSide.Top;
                }

                if (velocity.X < 0 &&
                    position.X + collBox.X <= x * TileWidthPx + tile.CollisionBox.X + tile.CollisionBox.Width &&
                    position.X + collBox.X >= x * TileWidthPx + tile.CollisionBox.X + tile.CollisionBox.Width -
                    CollisionThresholdPx)
                {
                    collisions |= BoxSide.Right;
                }

                if (velocity.Y < 0 &&
                    position.Y + collBox.Y <= y * TileHeightPx + tile.CollisionBox.Y + tile.CollisionBox.Height &&
                    position.Y + collBox.Y >= y * TileHeightPx + tile.CollisionBox.Y + tile.CollisionBox.Height -
                    CollisionThresholdPx)
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
                        position.Y + collBox.Y < y * TileHeightPx + tile.CollisionBox.Y + tile.CollisionBox.Height &&
                        position.Y + collBox.Y + collBox.Height > y * TileHeightPx + tile.CollisionBox.Y)
                    {
                        position.X = x * TileWidthPx + tile.CollisionBox.X - collBox.X - collBox.Width;
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
                        position.X + collBox.X < x * TileWidthPx + tile.CollisionBox.X + tile.CollisionBox.Width &&
                        position.X + collBox.X + collBox.Width > x * TileWidthPx + tile.CollisionBox.X)
                    {
                        position.Y = y * TileWidthPx + tile.CollisionBox.Y - collBox.Y - collBox.Height;
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
                        position.Y + collBox.Y < y * TileHeightPx + tile.CollisionBox.Y + tile.CollisionBox.Height &&
                        position.Y + collBox.Y + collBox.Height > y * TileHeightPx + tile.CollisionBox.Y)
                    {
                        position.X = x * TileWidthPx + tile.CollisionBox.X + tile.CollisionBox.Width - collBox.X;
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
                        position.X + collBox.X < x * TileWidthPx + tile.CollisionBox.X + tile.CollisionBox.Width &&
                        position.X + collBox.X + collBox.Width > x * TileWidthPx + tile.CollisionBox.X)
                    {
                        position.Y = y * TileWidthPx + tile.CollisionBox.Y + tile.CollisionBox.Height - collBox.Y;
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
                }
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 viewportPosition)
    {
        var topLeftX = (int)(viewportPosition.X / TileWidthPx);
        var topLeftY = (int)(viewportPosition.Y / TileHeightPx);

        for (int x = topLeftX; x < Width; x++)
        {
            for (int y = topLeftY; y < Height; y++)
            {
                if (!_tileMap.TryGetValue(new Vector2(x, y), out TileId tileId))
                {
                    continue;
                }

                Tile tile = tileLibrary.GetTile(tileId);
                var tilePosition = new Vector2(
                    x * TileWidthPx - (int)viewportPosition.X,
                    y * TileHeightPx - (int)viewportPosition.Y
                );

                spriteBatch.Draw(
                    tile.Texture,
                    tilePosition,
                    tile.SourceRectangle,
                    Color.White
                );
            }
        }
    }
}