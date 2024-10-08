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

    private readonly Dictionary<Vector2, Tile> _tileMap = new();
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
                var tileDefinitionId =
                    new TileDefinitionId(reader.GetAttribute("tileId") ?? throw new ResourceLoadException());

                var tile = new Tile
                {
                    TileDefinition = tileLibrary.GetTileDefinition(tileDefinitionId),
                    Position = new Vector2(x * TileWidthPx, y * TileHeightPx),
                };

                _tileMap[new Vector2(x, y)] = tile;
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

    public void CheckCollision(ICollidable collidable)
    {
        if (!collidable.CanCollide)
        {
            return;
        }

        Rectangle collidableWorldCollisionBox = collidable.WorldCollisionBox;

        // Get the positions of the Tiles to check
        int xMin = Math.Max(collidableWorldCollisionBox.Left / TileWidthPx, 0);
        int xMax = Math.Min(collidableWorldCollisionBox.Right / TileWidthPx, Width - 1);

        int yMin = Math.Max(collidableWorldCollisionBox.Top / TileHeightPx, 0);
        int yMax = Math.Min(collidableWorldCollisionBox.Bottom / TileHeightPx, Height - 1);

        Vector2 safePosition = collidable.Position;
        Vector2 safeVelocity = collidable.Velocity;

        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                if (!_tileMap.TryGetValue(new Vector2(x, y), out Tile? tile))
                {
                    continue;
                }

                if (!tile.CanCollide)
                {
                    continue;
                }

                Rectangle tileWorldCollisionBox = tile.WorldCollisionBox;

                // Check the tiles' collision sides for collisions
                List<BoxSide> collidingCollidableSide = [];

                if (tile.CollisionSides.Contains(BoxSide.Left) &&
                    collidable.Velocity.X > 0 &&
                    tileWorldCollisionBox.Left <= collidableWorldCollisionBox.Right &&
                    collidableWorldCollisionBox.Right <= tileWorldCollisionBox.Left + CollisionThresholdPx &&
                    collidableWorldCollisionBox.Bottom >= tileWorldCollisionBox.Top &&
                    collidableWorldCollisionBox.Top <= tileWorldCollisionBox.Bottom)
                {
                    collidingCollidableSide.Add(BoxSide.Right);
                    safePosition.X = tileWorldCollisionBox.Left - collidableWorldCollisionBox.Width -
                                     collidable.CollisionBoxDefinition.Left;
                    safeVelocity.X = 0;
                }

                if (tile.CollisionSides.Contains(BoxSide.Right) &&
                    collidable.Velocity.X < 0 &&
                    collidableWorldCollisionBox.Left <= tileWorldCollisionBox.Right &&
                    tileWorldCollisionBox.Right - CollisionThresholdPx <= collidableWorldCollisionBox.X &&
                    collidableWorldCollisionBox.Bottom >= tileWorldCollisionBox.Top &&
                    collidableWorldCollisionBox.Top <= tileWorldCollisionBox.Bottom)
                {
                    collidingCollidableSide.Add(BoxSide.Left);
                    safePosition.X = tileWorldCollisionBox.Right - collidable.CollisionBoxDefinition.Left;
                    safeVelocity.X = 0;
                }

                if (tile.CollisionSides.Contains(BoxSide.Top) &&
                    collidable.Velocity.Y > 0 &&
                    tileWorldCollisionBox.Top <= collidableWorldCollisionBox.Bottom &&
                    collidableWorldCollisionBox.Bottom <= tileWorldCollisionBox.Top + CollisionThresholdPx &&
                    collidableWorldCollisionBox.Right >= tileWorldCollisionBox.Left &&
                    collidableWorldCollisionBox.Left <= tileWorldCollisionBox.Right)
                {
                    collidingCollidableSide.Add(BoxSide.Bottom);
                    safePosition.Y = tileWorldCollisionBox.Top - collidableWorldCollisionBox.Height -
                                     collidable.CollisionBoxDefinition.Top;
                    safeVelocity.Y = 0;
                }

                if (tile.CollisionSides.Contains(BoxSide.Bottom) &&
                    collidable.Velocity.Y < 0 &&
                    collidableWorldCollisionBox.Top <= tileWorldCollisionBox.Bottom &&
                    tileWorldCollisionBox.Bottom - CollisionThresholdPx <= collidableWorldCollisionBox.Top &&
                    collidableWorldCollisionBox.Right >= tileWorldCollisionBox.Left &&
                    collidableWorldCollisionBox.Left <= tileWorldCollisionBox.Right)
                {
                    collidingCollidableSide.Add(BoxSide.Top);
                    safePosition.Y = tileWorldCollisionBox.Bottom - collidable.CollisionBoxDefinition.Top;
                    safeVelocity.Y = 0;
                }

                if (collidingCollidableSide.Count > 0)
                {
                    collidable.OnCollision(tile, collidingCollidableSide, safePosition, safeVelocity);
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
                if (!_tileMap.TryGetValue(new Vector2(x, y), out Tile? tile))
                {
                    continue;
                }

                tile.Draw(spriteBatch, viewportPosition);
            }
        }
    }
}