using System;

namespace PhysicaliaRemastered;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        using PhysicaliaGame game = new PhysicaliaGame();
        game.Run();
    }
}