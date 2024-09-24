namespace XNALibrary.TileEngine;

public class TileLibrary
{
    private readonly Dictionary<TileId, Tile> _tileLibrary = new();

    public void AddTile(TileId tileId, Tile tile)
    {
        _tileLibrary.Add(tileId, tile);
    }

    public Tile GetTile(TileId id)
    {
        return _tileLibrary[id];
    }
}