using ArchitectureLibrary.History;
using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.Models;
using GBATool.Signals;
using System.Collections.Generic;

namespace GBATool.HistoryActions;

public class PaintMapTilesHistoryAction : IHistoryAction
{
    private readonly List<Tile> _originalTiles = [];
    private readonly List<Tile> _bucketTiles = [];
    private readonly string _mapID = string.Empty;

    public PaintMapTilesHistoryAction(MapModel? mapModel, List<Tile> paintingTiles)
    {
        if (mapModel == null)
        {
            return;
        }

        _mapID = mapModel.MapID;

        Tile[] tiles;

        if (mapModel.MapType == MapType.Regular)
        {
            tiles = [.. mapModel.RegularMapTiles];
        }
        else
        {
            tiles = [.. mapModel.AffineMapTiles];
        }

        foreach (Tile tile in paintingTiles)
        {
            _bucketTiles.Add(new()
            {
                CellIndex = tile.CellIndex,
                BankID = tile.BankID,
                TileSetID = tile.TileSetID,
                TileSetOrigin = tile.TileSetOrigin
            });

            _originalTiles.Add(new()
            {
                CellIndex = tile.CellIndex,
                BankID = tiles[tile.CellIndex].BankID,
                TileSetID = tiles[tile.CellIndex].TileSetID,
                TileSetOrigin = tiles[tile.CellIndex].TileSetOrigin
            });
        }
    }

    public void Redo()
    {
        if (_bucketTiles.Count > 0)
        {
            SignalManager.Get<InvalidateMapCacheSignal>().Dispatch([_mapID]);
            SignalManager.Get<PaintMapTilesSignal>().Dispatch(_bucketTiles, true);
        }
    }

    public void Undo()
    {
        if (_originalTiles.Count > 0)
        {
            SignalManager.Get<InvalidateMapCacheSignal>().Dispatch([_mapID]);
            SignalManager.Get<PaintMapTilesSignal>().Dispatch(_originalTiles, true);
        }
    }
}

