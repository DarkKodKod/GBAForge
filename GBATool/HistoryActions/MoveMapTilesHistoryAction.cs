using ArchitectureLibrary.History;
using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.Models;
using GBATool.Signals;
using GBATool.ViewModels;
using System.Collections.Generic;

namespace GBATool.HistoryActions;

public class MoveMapTilesHistoryAction : IHistoryAction
{
    private readonly List<Tile> _originalTiles = [];
    private readonly List<Tile> _deletedTiles = [];
    private readonly List<Tile> _bucketTiles = [];
    private readonly List<TileObject> _deletedTileObjects = [];
    private readonly string _mapID = string.Empty;

    public MoveMapTilesHistoryAction(MapModel? mapModel, List<Tile> paintingTiles, List<TileObject> erasedTiles)
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

        foreach (TileObject tileObject in erasedTiles)
        {
            _deletedTileObjects.Add(tileObject);

            _deletedTiles.Add(new()
            {
                CellIndex = tileObject.Index,
                BankID = tiles[tileObject.Index].BankID,
                TileSetID = tiles[tileObject.Index].TileSetID,
                TileSetOrigin = tiles[tileObject.Index].TileSetOrigin
            });
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
            SignalManager.Get<PaintMapTilesSignal>().Dispatch(_bucketTiles, false);
            SignalManager.Get<DeleteMapTilesSignal>().Dispatch(_deletedTileObjects, false);
            SignalManager.Get<LoadMapImageSignal>().Dispatch();
        }
    }

    public void Undo()
    {
        if (_originalTiles.Count > 0)
        {
            SignalManager.Get<InvalidateMapCacheSignal>().Dispatch([_mapID]);
            SignalManager.Get<PaintMapTilesSignal>().Dispatch(_originalTiles, false);
            SignalManager.Get<PaintMapTilesSignal>().Dispatch(_deletedTiles, false);
            SignalManager.Get<LoadMapImageSignal>().Dispatch();
        }
    }
}
