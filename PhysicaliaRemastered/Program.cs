using System;

namespace PhysicaliaRemastered;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        using var game = new PhysicaliaGame();
        game.Run();
    }
}