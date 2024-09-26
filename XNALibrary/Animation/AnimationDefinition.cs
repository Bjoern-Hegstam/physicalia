using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNALibrary.Sprites;

namespace XNALibrary.Animation;

public record AnimationDefinition(
    AnimationDefinitionId Id,
    List<Frame> Frames,
    float FramesPerSecond,
    bool IsLoop
);

public record Frame(Texture2D Texture2D, Rectangle SourceRectangle, Point Origin);