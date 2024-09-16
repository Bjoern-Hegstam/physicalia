namespace XNALibrary.Graphics.TileEngine;

/// <summary>
/// Bitflagged enum representing the sides of a box with four sides.
/// </summary>
[Flags]
public enum BoxSide
{
    Top = 0x01,
    Bottom = 0x02,
    Left = 0x04,
    Right = 0x08
}