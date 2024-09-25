using System.IO;

namespace PhysicaliaRemastered.GameManagement;

public static class Environment
{
    public static readonly string GameDataPath = Path.Combine("Content", "GameData");
    public static readonly string LibraryPath = Path.Combine(GameDataPath, "Libraries");
    public static readonly string WorldPath = Path.Combine(GameDataPath, "Worlds");
}