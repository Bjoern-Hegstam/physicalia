namespace XNALibrary.TileEngine;

public class TileLibrary
{
    private readonly Dictionary<TileDefinitionId, TileDefinition> _tileLibrary = new();

    public void AddTileDefinition(TileDefinitionId tileDefinitionId, TileDefinition tile)
    {
        _tileLibrary.Add(tileDefinitionId, tile);
    }

    public TileDefinition GetTileDefinition(TileDefinitionId definitionId)
    {
        return _tileLibrary[definitionId];
    }
}