namespace XNALibrary.Graphics;

public readonly struct TextureId(string id)
{
    public string Id => id;

    public string AssetName => "GameData/Texture/" + id;
}