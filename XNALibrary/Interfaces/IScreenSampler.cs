using Microsoft.Xna.Framework;

namespace XNALibrary.Interfaces;

public interface IScreenSampler
{
    Vector2 Position { get; set; }

    int MaxWidth { get; set; }
    int MaxHeight { get; set; }

    int Width { get; set; }
    int Height { get; set; }

    bool IsOnScreen(Rectangle boundingBox);
    bool IsOnScreen(int x, int y, int width, int height);
}