using ArchitectureLibrary.History;
using ArchitectureLibrary.Signals;
using GBATool.Models;
using GBATool.Signals;
using GBATool.ViewModels;
using System.Collections.Generic;

namespace GBATool.HistoryActions;

public class DeleteMapTilesHitoryAction : IHistoryAction
{
    private readonly List<TileObject> _selectedTiles = [];
    private readonly List<Tile> _originalTiles = [];
    private readonly string _mapID = string.Empty;

    public DeleteMapTilesHitoryAction(MapModel? mapModel, List<TileObject> selectedTiles)
    {
        _selectedTiles = selectedTiles;

        if (mapModel == null)
        {
            return;
        }

        _mapID = mapModel.MapID;

        Tile[] tiles = [.. mapModel.RegularMapTiles];

        foreach (TileObject tileObject in selectedTiles)
        {
            _originalTiles.Add(new()
            {
                CellIndex = tileObject.Index,
                BankID = tiles[tileObject.Index].BankID,
                TileSetID = tiles[tileObject.Index].TileSetID,
                TileSetOrigin = tiles[tileObject.Index].TileSetOrigin
            });
        }
    }

    public void Redo()
    {
        if (_selectedTiles.Count > 0)
        {
            SignalManager.Get<InvalidateMapCacheSignal>().Dispatch([_mapID]);
            SignalManager.Get<DeleteMapTilesSignal>().Dispatch(_selectedTiles);
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
