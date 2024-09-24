namespace XNALibrary.Graphics;

public record TextureId(string Id)
{
    public string AssetName => "GameData/Texture/" + Id;
}