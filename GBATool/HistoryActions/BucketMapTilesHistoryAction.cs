using ArchitectureLibrary.History;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;
using System.Collections.Generic;

namespace GBATool.HistoryActions;

public class BucketMapTilesHistoryAction : IHistoryAction
{
    private readonly List<Tile> _originalTiles = [];
    private readonly List<Tile> _bucketTiles = [];
    private readonly string _mapID = string.Empty;

    public BucketMapTilesHistoryAction(MapModel? mapModel, List<Tile> paintingTiles)
    {
        if (mapModel == null)
        {
            return;
        }

        _mapID = mapModel.MapID;

        Tile[] tiles = [.. mapModel.RegularMapTiles];

        foreach (Tile tileObject in paintingTiles)
        {
            _bucketTiles.Add(new()
            {
                CellIndex = tileObject.CellIndex,
                BankID = tileObject.BankID,
                TileSetID = tileObject.TileSetID,
                TileSetOrigin = tileObject.TileSetOrigin
            });

            _originalTiles.Add(new()
            {
                CellIndex = tileObject.CellIndex,
                BankID = tiles[tileObject.CellIndex].BankID,
                TileSetID = tiles[tileObject.CellIndex].TileSetID,
                TileSetOrigin = tiles[tileObject.CellIndex].TileSetOrigin
            });
        }
    }

    public void Redo()
    {
        if (_bucketTiles.Count > 0)
        {
            SignalManager.Get<InvalidateMapCacheSignal>().Dispatch([_mapID]);
            SignalManager.Get<PaintMapTilesSignal>().Dispatch(_bucketTiles);
        }
    }

    public void Undo()
    {
        if (_originalTiles.Count > 0)
        {
            SignalManager.Get<InvalidateMapCacheSignal>().Dispatch([_mapID]);
            SignalManager.Get<PaintMapTilesSignal>().Dispatch(_originalTiles);
        }
    }
}
