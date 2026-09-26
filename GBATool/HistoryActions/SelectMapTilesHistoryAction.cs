using ArchitectureLibrary.History;
using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.ViewModels;
using System.Collections.Generic;
using System.Windows;

namespace GBATool.HistoryActions;

public class SelectMapTilesHistoryAction : IHistoryAction
{
    private readonly List<TileObject> _selectedTiles = [];
    private readonly Visibility _isVisible = Visibility.Collapsed;
    private readonly Rect _selectionRect;

    public SelectMapTilesHistoryAction(List<TileObject> selectedTiles, Visibility isVisible, Rect selectionRect)
    {
        _isVisible = isVisible;
        _selectionRect = selectionRect;

        foreach (TileObject tileObject in selectedTiles)
        {
            _selectedTiles.Add(new()
            {
                Index = tileObject.Index,
                PaletetteIndex = tileObject.PaletetteIndex,
                IsFlippedHorizontal = tileObject.IsFlippedHorizontal,
                IsFlippedVertical = tileObject.IsFlippedVertical
            });
        }
    }

    public void Redo()
    {
        if (_selectedTiles.Count > 0)
        {
            SignalManager.Get<SelectTilesSignal>().Dispatch([.. _selectedTiles]);
        }
    }

    public void Undo()
    {
        if (_isVisible == Visibility.Visible)
        {
            SignalManager.Get<SelectTilesFromRectSignal>().Dispatch(_selectionRect);
        }
    }
}
