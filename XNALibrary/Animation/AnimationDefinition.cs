using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Animation;

public record AnimationDefinition(
    AnimationDefinitionId Id,
    Texture2D Texture,
    Rectangle StartFrame,
    int ColumnCount,
    int RowCount,
    float FrameRate,
    bool IsLoop
);