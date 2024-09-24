namespace XNALibrary.Sprites;

public class SpriteLibrary
{
    private readonly Dictionary<SpriteId, Sprite> _sprites = new();

    public void AddSprite(SpriteId spriteId, Sprite sprite)
    {
        _sprites.Add(spriteId, sprite);
    }

    public Sprite GetSprite(SpriteId id)
    {
        return _sprites[id];
    }
}