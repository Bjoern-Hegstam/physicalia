namespace XNALibrary.Graphics;

public record struct TextureId(string Id)
{
    public string AssetName => "GameData/Texture/" + Id;
}