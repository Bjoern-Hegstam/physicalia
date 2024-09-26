using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNALibrary.Animation;

public record AnimationDefinition(
    AnimationDefinitionId Id,
    List<Frame> Frames,
    float FramesPerSecond,
    bool IsLoop
);

public record Frame(Texture2D Texture, Rectangle SourceRectangle, Point Origin);