using ArchitectureLibrary.History;
using GBATool.Models;
using GBATool.ViewModels;
using System.Collections.Generic;

namespace GBATool.HistoryActions;

public class MoveMapTilesHistoryAction : IHistoryAction
{
    private readonly List<Tile> _originalTiles = [];
    private readonly List<TileObject> _deletedTiles = [];
    private readonly string _mapID = string.Empty;

    public MoveMapTilesHistoryAction(MapModel? mapModel, List<Tile> paintingTiles, List<TileObject> erasedTiles)
    {
        if (mapModel == null)
        {
            return;
        }

        _mapID = mapModel.MapID;
    }

    public void Redo()
    {

    }

    public void Undo()
    {

    }
}
