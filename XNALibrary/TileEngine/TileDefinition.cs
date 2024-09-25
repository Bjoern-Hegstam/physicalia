using Microsoft.Xna.Framework;
using XNALibrary.Sprites;

namespace XNALibrary.TileEngine;

public class TileDefinition
{
    public required Sprite Sprite { get; init; }

    public required Rectangle CollisionBox { get; init; }

    public required List<BoxSide> CollisionSides { get; init; }
}