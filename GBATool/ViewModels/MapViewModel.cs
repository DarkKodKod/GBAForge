using ArchitectureLibrary.History.Signals;
using ArchitectureLibrary.Signals;
using GBATool.Commands.Banks;
using GBATool.Commands.Input;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.HistoryActions;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GBATool.ViewModels;

public class TileObject : INotifyPropertyChanged
{
    private bool _isFlippedHorizontal;
    private bool _isFlippedVertical;
    private int _paletteIndex;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propname)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
    }

    public int Index { get; init; }

    public int PaletetteIndex
    {
        get => _paletteIndex;
        set
        {
            if (_paletteIndex != value)
            {
                _paletteIndex = value;
            }

            OnPropertyChanged(nameof(PaletetteIndex));
        }
    }

    public bool IsFlippedHorizontal
    {
        get => _isFlippedHorizontal;
        set
        {
            if (_isFlippedHorizontal != value)
            {
                _isFlippedHorizontal = value;
            }

            OnPropertyChanged(nameof(IsFlippedHorizontal));
        }
    }

    public bool IsFlippedVertical
    {
        get => _isFlippedVertical;
        set
        {
            if (_isFlippedVertical != value)
            {
                _isFlippedVertical = value;
            }

            OnPropertyChanged(nameof(IsFlippedVertical));
        }
    }
}

public class BankIndex : INotifyPropertyChanged
{
    private int _index;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propname)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
    }

    public int PaletteIndex { get; init; }

    public int Index
    {
        get => _index;
        set
        {
            _index = value;

            OnPropertyChanged(nameof(Index));

            SignalManager.Get<ChangeMapPaletteSignal>().Dispatch(value, PaletteIndex);
        }
    }
}

public class MapViewModel : ItemViewModel
{
    private Visibility _gridVisibility = Visibility.Visible;
    private ImageSource? _mapImage = null;
    private FileModelVO[]? _banks;
    private int _selectedBank = -1;
    private bool _doNotSave;
    private int _canvasHeght = 256;
    private int _canvasWidth = 256;
    private float _scale = 3.0f;
    private MapType _mapType = MapType.Regular;
    private Priority _priority = Priority.Highest;
    private BckgrRegularSize _bckgrRegularSize = BckgrRegularSize.Small;
    private BckgrAffineSize _bckgrAffineSize = BckgrAffineSize.Affine16x16;
    private BindingList<TileObject> _tiles = [];
    private bool _enableMosaic;
    private bool _affineWrapping;
    private BindingList<BankIndex> _paletteIDs = [];
    private ScreenBaseBlock _screenBaseBlock = ScreenBaseBlock.Block0;
    private CharacterBaseBlock _characterBaseBlock = CharacterBaseBlock.Block0;
    private string _bankID = string.Empty;
    private List<FileModelVO> _palettes = [];
    private TileObject? _selectedTile = null;
    private Visibility _mouseSelectionActive;
    private int _mouseSelectionOriginX;
    private int _mouseSelectionOriginY;
    private int _mouseSelectionWidth;
    private int _mouseSelectionHeight;
    private Point _initialMousePositionInCanvas;
    private Visibility _tilesSelectedActive;
    private int _tilesSelectedWidth;
    private int _tilesSelectedHeight;
    private int _tilesSelectedOriginX;
    private int _tilesSelectedOriginY;
    private MapFunctionality _currentMapFunctionality = MapFunctionality.Select;
    private bool _isMovingFromInsideCanvas;

    public MapModel? GetModel()
    {
        return ProjectItem?.FileHandler?.FileModel is MapModel model ? model : null;
    }

    #region Commands
    public MouseButtonEventCommand<MouseDownEventSignal> MouseDownEventCommand { get; } = new();
    public MouseButtonEventCommand<MouseUpEventSignal> MouseUpEventCommand { get; } = new();
    public MouseEventCommand<MouseMoveEventSignal> MouseMoveEventCommand { get; } = new();
    public FileModelVOSelectionChangedCommand FileModelVOSelectionChangedCommand { get; } = new();
    #endregion

    #region get/set
    public MapPaintCursorVO? CurrentCursor { get; set; } = null;

    public TileObject? SelectedTile
    {
        get => _selectedTile;
        set
        {
            _selectedTile = value;

            OnPropertyChanged(nameof(SelectedTile));
        }
    }

    public MapFunctionality CurrentMapFunctionality
    {
        get => _currentMapFunctionality;
        set
        {
            _currentMapFunctionality = value;

            OnPropertyChanged(nameof(CurrentMapFunctionality));
        }
    }

    public FileModelVO[]? Banks
    {
        get => _banks;
        set
        {
            _banks = value;

            OnPropertyChanged(nameof(Banks));
        }
    }

    public int SelectedBank
    {
        get => _selectedBank;
        set
        {
            _selectedBank = value;

            OnPropertyChanged(nameof(SelectedBank));
        }
    }

    public Visibility GridVisibility
    {
        get => _gridVisibility;
        set
        {
            _gridVisibility = value;

            OnPropertyChanged(nameof(GridVisibility));
        }
    }

    public ImageSource? MapImage
    {
        get => _mapImage;
        set
        {
            _mapImage = value;

            OnPropertyChanged(nameof(MapImage));
        }
    }

    public int CanvasHeight
    {
        get => _canvasHeght;
        set
        {
            _canvasHeght = value;

            OnPropertyChanged(nameof(CanvasHeight));
        }
    }

    public int CanvasWidth
    {
        get => _canvasWidth;
        set
        {
            _canvasWidth = value;

            OnPropertyChanged(nameof(CanvasWidth));
        }
    }

    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;

            OnPropertyChanged(nameof(Scale));
        }
    }

    public MapType MapType
    {
        get => _mapType;
        set
        {
            if (_mapType != value)
            {
                _mapType = value;

                UpdateAndSaveMapType(value);
            }

            OnPropertyChanged(nameof(MapType));
        }
    }
    public Priority Priority
    {
        get => _priority;
        set
        {
            if (_priority != value)
            {
                _priority = value;

                UpdateAndSavePriority(value);
            }

            OnPropertyChanged(nameof(Priority));
        }
    }

    public BckgrRegularSize BckgrRegularSize
    {
        get => _bckgrRegularSize;
        set
        {
            if (_bckgrRegularSize != value)
            {
                switch (BckgrRegularSize)
                {
                    case BckgrRegularSize.Wide:
                        CanvasHeight = 256;
                        CanvasWidth = 512;
                        break;
                    case BckgrRegularSize.Tall:
                        CanvasWidth = 256;
                        CanvasHeight = 512;
                        break;
                    case BckgrRegularSize.Big:
                        CanvasHeight = CanvasWidth = 512;
                        break;
                    case BckgrRegularSize.Small:
                    default:
                        CanvasHeight = CanvasWidth = 256;
                        break;
                }

                UpdateAndSaveBackgroundRegularSize(value);

                _bckgrRegularSize = value;
            }

            OnPropertyChanged(nameof(BckgrRegularSize));
        }
    }

    public BckgrAffineSize BckgrAffineSize
    {
        get => _bckgrAffineSize;
        set
        {
            if (_bckgrAffineSize != value)
            {
                _bckgrAffineSize = value;

                UpdateAndSaveBackgroundAffineSize(value);
            }

            OnPropertyChanged(nameof(BckgrAffineSize));
        }
    }

    public Visibility MouseSelectionActive
    {
        get => _mouseSelectionActive;
        set
        {
            _mouseSelectionActive = value;

            OnPropertyChanged(nameof(MouseSelectionActive));
        }
    }

    public int MouseSelectionOriginX
    {
        get => _mouseSelectionOriginX;
        set
        {
            _mouseSelectionOriginX = value;

            OnPropertyChanged(nameof(MouseSelectionOriginX));
        }
    }

    public Visibility TilesSelectedActive
    {
        get => _tilesSelectedActive;
        set
        {
            _tilesSelectedActive = value;

            OnPropertyChanged(nameof(TilesSelectedActive));
        }
    }
    public int TilesSelectedWidth
    {
        get => _tilesSelectedWidth;
        set
        {
            _tilesSelectedWidth = value;

            OnPropertyChanged(nameof(TilesSelectedWidth));
        }
    }
    public int TilesSelectedHeight
    {
        get => _tilesSelectedHeight;
        set
        {
            _tilesSelectedHeight = value;

            OnPropertyChanged(nameof(TilesSelectedHeight));
        }
    }
    public int TilesSelectedOriginX
    {
        get => _tilesSelectedOriginX;
        set
        {
            _tilesSelectedOriginX = value;

            OnPropertyChanged(nameof(TilesSelectedOriginX));
        }
    }
    public int TilesSelectedOriginY
    {
        get => _tilesSelectedOriginY;
        set
        {
            _tilesSelectedOriginY = value;

            OnPropertyChanged(nameof(TilesSelectedOriginY));
        }
    }

    public int MouseSelectionOriginY
    {
        get => _mouseSelectionOriginY;
        set
        {
            _mouseSelectionOriginY = value;

            OnPropertyChanged(nameof(MouseSelectionOriginY));
        }
    }

    public int MouseSelectionWidth
    {
        get => _mouseSelectionWidth;
        set
        {
            _mouseSelectionWidth = value;

            OnPropertyChanged(nameof(MouseSelectionWidth));
        }
    }

    public int MouseSelectionHeight
    {
        get => _mouseSelectionHeight;
        set
        {
            _mouseSelectionHeight = value;

            OnPropertyChanged(nameof(MouseSelectionHeight));
        }
    }

    public BindingList<TileObject> Tiles
    {
        get => _tiles;
        set
        {
            _tiles = value;

            OnPropertyChanged(nameof(Tiles));
        }
    }

    public bool EnableMosaic
    {
        get => _enableMosaic;
        set
        {
            if (_enableMosaic != value)
            {
                _enableMosaic = value;

                UpdateAndSaveEnableMosaic(value);
            }

            OnPropertyChanged(nameof(EnableMosaic));
        }
    }

    public bool AffineWrapping
    {
        get => _affineWrapping;
        set
        {
            if (_affineWrapping != value)
            {
                _affineWrapping = value;

                UpdateAndSaveAffineWrapping(value);
            }

            OnPropertyChanged(nameof(AffineWrapping));
        }
    }

    public ScreenBaseBlock ScreenBaseBlock
    {
        get => _screenBaseBlock;
        set
        {
            if (_screenBaseBlock != value)
            {
                _screenBaseBlock = value;

                UpdateAndSaveScreenBaseBlock(value);
            }

            OnPropertyChanged(nameof(ScreenBaseBlock));
        }
    }

    public CharacterBaseBlock CharacterBaseBlock
    {
        get => _characterBaseBlock;
        set
        {
            if (_characterBaseBlock != value)
            {
                _characterBaseBlock = value;

                UpdateAndSaveCharacterBaseBlock(value);
            }

            OnPropertyChanged(nameof(CharacterBaseBlock));
        }
    }

    public string BankID
    {
        get => _bankID;
        set
        {
            if (_bankID != value)
            {
                _bankID = value;

                UpdateAndSaveBankID(value);
            }

            OnPropertyChanged(nameof(BankID));
        }
    }

    public List<FileModelVO> Palettes
    {
        get => _palettes;
        set
        {
            _palettes = value;

            OnPropertyChanged(nameof(Palettes));
        }
    }

    public BindingList<BankIndex> PaletteIDs
    {
        get => _paletteIDs;
        set
        {
            if (_paletteIDs != value)
            {
                _paletteIDs = value;

                OnPropertyChanged(nameof(PaletteIDs));
            }
        }
    }
    #endregion

    public MapViewModel()
    {
        FileModelVO[] filemodelVo = [.. ProjectFiles.GetModels<BankModel>()];

        IEnumerable<FileModelVO> banks = filemodelVo;

        Banks = new FileModelVO[banks.Count()];

        int index = 0;

        foreach (FileModelVO item in banks)
        {
            item.Index = index;

            Banks[index] = item;

            index++;
        }

        List<FileModelVO> list =
        [
            new()
            {
                Index = -1,
                Name = "None",
                Model = null
            },
            .. ProjectFiles.GetModels<PaletteModel>()
        ];

        Palettes = [.. list];

        PaletteIDs.ListChanged += (s, e) =>
        {
            OnPropertyChanged("Item[]");
        };

        for (int i = 0; i < 16; i++)
        {
            PaletteIDs.Add(new() { PaletteIndex = i });
        }
    }

    public override void OnActivate()
    {
        base.OnActivate();

        MouseSelectionActive = Visibility.Collapsed;
        TilesSelectedActive = Visibility.Collapsed;

        #region Signals
        SignalManager.Get<FileModelVOSelectionChangedSignal>().Listener += OnFileModelVOSelectionChanged;
        SignalManager.Get<MouseDownEventSignal>().Listener += OnMouseDownEvent;
        SignalManager.Get<MouseUpEventSignal>().Listener += OnMouseUpEvent;
        SignalManager.Get<MouseMoveEventSignal>().Listener += OnMouseMoveEvent;
        SignalManager.Get<ChangeMapPaletteSignal>().Listener += OnChangeMapPalette;
        SignalManager.Get<ResetSelectionAreaSignal>().Listener += OnResetSelectionArea;
        SignalManager.Get<SelectTilesSignal>().Listener += OnSelectTiles;
        SignalManager.Get<CheckMapBucketToolSignal>().Listener += OnCheckMapBucketTool;
        SignalManager.Get<CheckMapSelectToolSignal>().Listener += OnCheckMapSelectTool;
        SignalManager.Get<CheckMapEraseToolSignal>().Listener += OnCheckMapEraseTool;
        SignalManager.Get<CheckMapPaintToolSignal>().Listener += OnCheckMapPaintTool;
        SignalManager.Get<CheckMapMoveToolSignal>().Listener += OnCheckMapMoveTool;
        SignalManager.Get<UseBitmapAsCursorSignal>().Listener += OnUseBitmapAsCursor;
        SignalManager.Get<DeleteMapTilesSignal>().Listener += OnDeleteMapTiles;
        SignalManager.Get<InvalidateMapCacheSignal>().Listener += OnInvalidateMapCache;
        SignalManager.Get<PaintMapTilesSignal>().Listener += OnPaintMapTiles;
        #endregion

        MapModel? model = GetModel();

        if (model == null)
        {
            return;
        }

        if (model.RegularMapTiles.Count == 0)
        {
            model.CreateNewRegularMap();

            ProjectItem?.FileHandler?.Save();
        }

        _doNotSave = true;

        MapType = model.MapType;
        Priority = model.Priority;
        BckgrRegularSize = model.BckgrRegularSize;
        BckgrAffineSize = model.BckgrAffineSize;

        foreach (Tile tile in model.RegularMapTiles)
        {
            Tiles.Add(new()
            {
                Index = tile.CellIndex
            });
        }

        EnableMosaic = model.EnableMosaic;
        AffineWrapping = model.AffineWrapping;
        ScreenBaseBlock = model.ScreenBaseBlock;
        CharacterBaseBlock = model.CharacterBaseBlock;
        BankID = model.BankID;

        for (int i = 0; i < model.PaletteIDs.Length; ++i)
        {
            string paletteID = model.PaletteIDs[i];

            int index = Palettes.FindIndex(o => o.Model?.GUID == paletteID);

            if (index > 0)
            {
                index--;
            }

            // index -1 is valid because the PaletteIDs array accepts -1 as the first empty element.
            PaletteIDs[i].Index = index;
        }

        SelectBank(BankID);

        LoadMapImage();

        _doNotSave = false;
    }

    public override void OnDeactivate()
    {
        MouseSelectionActive = Visibility.Collapsed;
        TilesSelectedActive = Visibility.Collapsed;

        #region Signals
        SignalManager.Get<FileModelVOSelectionChangedSignal>().Listener -= OnFileModelVOSelectionChanged;
        SignalManager.Get<MouseDownEventSignal>().Listener -= OnMouseDownEvent;
        SignalManager.Get<MouseUpEventSignal>().Listener -= OnMouseUpEvent;
        SignalManager.Get<MouseMoveEventSignal>().Listener -= OnMouseMoveEvent;
        SignalManager.Get<ChangeMapPaletteSignal>().Listener -= OnChangeMapPalette;
        SignalManager.Get<ResetSelectionAreaSignal>().Listener -= OnResetSelectionArea;
        SignalManager.Get<SelectTilesSignal>().Listener -= OnSelectTiles;
        SignalManager.Get<CheckMapBucketToolSignal>().Listener -= OnCheckMapBucketTool;
        SignalManager.Get<CheckMapSelectToolSignal>().Listener -= OnCheckMapSelectTool;
        SignalManager.Get<CheckMapEraseToolSignal>().Listener -= OnCheckMapEraseTool;
        SignalManager.Get<CheckMapPaintToolSignal>().Listener -= OnCheckMapPaintTool;
        SignalManager.Get<CheckMapMoveToolSignal>().Listener -= OnCheckMapMoveTool;
        SignalManager.Get<UseBitmapAsCursorSignal>().Listener -= OnUseBitmapAsCursor;
        SignalManager.Get<DeleteMapTilesSignal>().Listener -= OnDeleteMapTiles;
        SignalManager.Get<InvalidateMapCacheSignal>().Listener -= OnInvalidateMapCache;
        SignalManager.Get<PaintMapTilesSignal>().Listener -= OnPaintMapTiles;
        #endregion

        base.OnDeactivate();
    }

    private void OnCheckMapBucketTool()
    {
        CurrentMapFunctionality = MapFunctionality.BucketPaint;
    }

    private void OnCheckMapSelectTool()
    {
        CurrentMapFunctionality = MapFunctionality.Select;
    }

    private void OnCheckMapEraseTool()
    {
        CurrentMapFunctionality = MapFunctionality.Erase;
    }

    private void OnCheckMapPaintTool()
    {
        CurrentMapFunctionality = MapFunctionality.Paint;
    }

    private void OnCheckMapMoveTool()
    {
        CurrentMapFunctionality = MapFunctionality.Move;
    }

    private void OnResetSelectionArea(Point position)
    {
        MouseSelectionActive = Visibility.Collapsed;
        MouseSelectionOriginX = (int)position.X;
        MouseSelectionOriginY = (int)position.Y;
        MouseSelectionWidth = 0;
        MouseSelectionHeight = 0;
    }

    private void SelectBank(string bankID)
    {
        for (int i = 0; i < Banks?.Length; i++)
        {
            if (Banks[i].Model is not BankModel bank)
            {
                continue;
            }

            if (bank.GUID == bankID)
            {
                SelectedBank = i;
                return;
            }
        }
    }

    private void OnFileModelVOSelectionChanged(FileModelVO fileModel)
    {
        if (fileModel.Model is BankModel)
        {
            SignalManager.Get<CleanUpSpriteListSignal>().Dispatch();

            if (Banks == null || Banks.Length == 0)
            {
                return;
            }

            if (Banks[SelectedBank].Model is not BankModel model)
            {
                return;
            }

            BankID = model.GUID;

            SignalManager.Get<SetBankModelToBankViewerSignal>().Dispatch(model);
            SignalManager.Get<RemoveSpriteSelectionFromBank>().Dispatch();
        }
    }

    private void OnChangeMapPalette(int newPaletteIndex, int paletteObjectIndex)
    {
        if (!Enumerable.Range(0, 15).Contains(paletteObjectIndex))
        {
            return;
        }

        MapModel? model = GetModel();

        if (model == null)
        {
            return;
        }

        FileModelVO? palette = null;

        if (newPaletteIndex == -1)
        {
            palette = new FileModelVO() { Model = new PaletteModel() };
        }
        else
        {
            foreach (FileModelVO item in Palettes)
            {
                if (item.Index == newPaletteIndex)
                {
                    palette = item;
                    break;
                }
            }
        }

        if (palette?.Model is not PaletteModel paletteModel)
        {
            return;
        }

        model.PaletteIDs[paletteObjectIndex] = paletteModel.GUID ?? string.Empty;

        SignalManager.Get<PaletteColorArrayChangeSignal>().Dispatch(paletteModel.Colors, paletteObjectIndex);

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSaveBankID(string value)
    {
        MapModel? model = GetModel();

        if (model == null)
            return;

        model.BankID = value;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSaveCharacterBaseBlock(CharacterBaseBlock value)
    {
        MapModel? model = GetModel();

        if (model == null)
            return;

        model.CharacterBaseBlock = value;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSaveScreenBaseBlock(ScreenBaseBlock value)
    {
        MapModel? model = GetModel();

        if (model == null)
            return;

        model.ScreenBaseBlock = value;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSaveAffineWrapping(bool value)
    {
        MapModel? model = GetModel();

        if (model == null)
            return;

        model.AffineWrapping = value;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSaveEnableMosaic(bool enableMosaic)
    {
        MapModel? model = GetModel();

        if (model == null)
            return;

        model.EnableMosaic = enableMosaic;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSaveBackgroundAffineSize(BckgrAffineSize value)
    {
        MapModel? model = GetModel();

        if (model == null)
            return;

        model.BckgrAffineSize = value;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSaveBackgroundRegularSize(BckgrRegularSize value)
    {
        MapModel? model = GetModel();

        if (model == null)
            return;

        model.BckgrRegularSize = value;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSavePriority(Priority value)
    {
        MapModel? model = GetModel();

        if (model == null)
            return;

        model.Priority = value;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void UpdateAndSaveMapType(MapType value)
    {
        MapModel? model = GetModel();

        if (model == null)
            return;

        model.MapType = value;

        if (!_doNotSave)
        {
            ProjectItem?.FileHandler?.Save();
        }
    }

    private void OnMouseMoveEvent(MouseEventVO vO)
    {
        if (!_isMovingFromInsideCanvas)
        {
            return;
        }

        if (vO.Sender is not IInputElement sender)
        {
            return;
        }

        if (vO.EventArgs.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        string sourceName = string.Empty;

        if (vO.OriginalSource is FrameworkElement fe)
        {
            if (fe.Name != "mapCanvas" &&
                fe.Name != "imgMap")
            {
                return;
            }

            sourceName = fe.Name;
        }

        if (CurrentMapFunctionality != MapFunctionality.Select)
        {
            SignalManager.Get<ResetSelectionAreaSignal>().Dispatch(_initialMousePositionInCanvas);
            SignalManager.Get<SelectTilesSignal>().Dispatch([]);
        }

        Point positionInCanvas = vO.EventArgs.GetPosition(sender);

        if (CurrentMapFunctionality == MapFunctionality.Paint)
        {
            MapModel? model = GetModel();

            if (model == null)
            {
                return;
            }

            SignalManager.Get<ResetSelectionAreaSignal>().Dispatch(positionInCanvas);

            List<TileObject> selectedTiles = [GetSelectedVisualTile(positionInCanvas, model)];

            PaintTiles(selectedTiles, model);
        }
        else
        {
            MouseMoveSelection(positionInCanvas, sourceName);
        }
    }

    private void MouseMoveSelection(Point currentMousePosition, string sourceName)
    {
        if (Util.AboutEqual(currentMousePosition.X, _initialMousePositionInCanvas.X) &&
           Util.AboutEqual(currentMousePosition.Y, _initialMousePositionInCanvas.Y))
        {
            return;
        }

        SignalManager.Get<TryCaptureMouseSignal>().Dispatch(sourceName);

        UpdateMouseSelectionArea(currentMousePosition);
    }

    private void OnMouseUpEvent(MouseButtonVO vO)
    {
        string sourceName = string.Empty;

        if (vO.OriginalSource is FrameworkElement fe)
        {
            if (fe.Name != "mapCanvas" &&
                fe.Name != "imgMap")
            {
                return;
            }

            sourceName = fe.Name;
        }

        if (!_isMovingFromInsideCanvas)
        {
            return;
        }

        if (vO.Sender is not IInputElement sender)
        {
            return;
        }

        if (vO.OriginalSource is not Canvas and not Image and not Rectangle)
        {
            return;
        }

        MapModel? model = GetModel();

        if (model == null)
        {
            return;
        }

        SignalManager.Get<TryReleaseMouseSignal>().Dispatch(sourceName);

        Point pos = vO.MouseEvent.GetPosition(sender);

        if (pos.X < 0)
        {
            pos.X = 0;
        }
        if (pos.Y < 0)
        {
            pos.Y = 0;
        }

        List<TileObject> selectedTiles;
        bool clickedOnTile = false;

        if (MouseSelectionActive == Visibility.Visible)
        {
            Rect rectangle = new(MouseSelectionOriginX, MouseSelectionOriginY, MouseSelectionWidth, MouseSelectionHeight);

            selectedTiles = CheckAreaSelected(rectangle);
        }
        else
        {
            selectedTiles = [GetSelectedVisualTile(pos, model)];
            clickedOnTile = true;
        }

        SignalManager.Get<ResetSelectionAreaSignal>().Dispatch(pos);

        switch (CurrentMapFunctionality)
        {
            default:
            case MapFunctionality.Select:
                SelectTiles(selectedTiles);
                break;
            case MapFunctionality.Move:
                MoveTiles(selectedTiles);
                break;
            case MapFunctionality.Paint:
                if (clickedOnTile)
                {
                    PaintTiles(selectedTiles, model);
                }
                break;
            case MapFunctionality.BucketPaint:
                BucketPaint(selectedTiles, clickedOnTile, model);
                break;
            case MapFunctionality.Erase:
                EraseTiles(selectedTiles, model.MapID);
                break;
        }

        if (clickedOnTile && CurrentMapFunctionality != MapFunctionality.Select)
        {
            SignalManager.Get<ResetSelectionAreaSignal>().Dispatch(_initialMousePositionInCanvas);
            SignalManager.Get<SelectTilesSignal>().Dispatch([]);
        }

        _isMovingFromInsideCanvas = false;
    }

    private void OnUseBitmapAsCursor(MapPaintCursorVO vo)
    {
        CurrentCursor = vo;
    }

    private void PaintTiles(List<TileObject> selectedTiles, MapModel model)
    {
        if (selectedTiles.Count == 0)
        {
            return;
        }

        if (CurrentCursor == null)
        {
            return;
        }

        List<Tile> paintingTiles = [];

        // Paint one tile at a time

        VisualMapTileVO[,] array2DOfTiles = CurrentCursor.VisualMapTiles;

        Tile[] originalTiles;

        int mapSizeWidth;

        if (model.MapType == MapType.Regular)
        {
            mapSizeWidth = MapUtils.GetRegularMapSizeWidth(model.BckgrRegularSize);
            originalTiles = [.. model.RegularMapTiles];
        }
        else
        {
            mapSizeWidth = MapUtils.GetAffineMapSize(model.BckgrAffineSize);
            originalTiles = [.. model.AffineMapTiles];
        }

        for (int row = 0; row < array2DOfTiles.GetLength(0); row++)
        {
            for (int col = 0; col < array2DOfTiles.GetLength(1); col++)
            {
                VisualMapTileVO val = array2DOfTiles[row, col];

                int tileIndex = selectedTiles[0].Index + (row * mapSizeWidth) + col;

                if (val != VisualMapTileVO.Empty)
                {
                    if (originalTiles[tileIndex].BankID == CurrentCursor.BankID &&
                        originalTiles[tileIndex].TileSetID == val.TileSetID &&
                        originalTiles[tileIndex].TileSetOrigin.Equals(val.Point))
                    {
                        continue;
                    }

                    paintingTiles.Add(new()
                    {
                        CellIndex = tileIndex,
                        BankID = CurrentCursor.BankID,
                        TileSetID = val.TileSetID,
                        TileSetOrigin = val.Point
                    });
                }
                else
                {
                    if (string.IsNullOrEmpty(originalTiles[tileIndex].BankID) &&
                        string.IsNullOrEmpty(originalTiles[tileIndex].TileSetID))
                    {
                        continue;
                    }

                    paintingTiles.Add(new()
                    {
                        CellIndex = tileIndex,
                        BankID = string.Empty,
                        TileSetID = string.Empty,
                        TileSetOrigin = default
                    });
                }
            }
        }

        if (paintingTiles.Count > 0)
        {
            SignalManager.Get<InvalidateMapCacheSignal>().Dispatch([model.MapID]);
            SignalManager.Get<RegisterHistoryActionSignal>().Dispatch(new PaintMapTilesHistoryAction(model, paintingTiles));
            SignalManager.Get<PaintMapTilesSignal>().Dispatch(paintingTiles);
        }
    }

    private void LoadMapImage()
    {
        WriteableBitmap? mapBitmap = null;

        MapModel? model = GetModel();

        if (model != null)
        {
            mapBitmap = MapUtils.GetFrameImageFromCache(model);
        }

        MapImage = mapBitmap;
    }

    private static void SelectTiles(List<TileObject> selectedTiles)
    {
        if (selectedTiles.Count == 0)
        {
            return;
        }

        SignalManager.Get<SelectTilesSignal>().Dispatch([.. selectedTiles]);
    }

    private static void MoveTiles(List<TileObject> selectedTiles)
    {
        if (selectedTiles.Count == 0)
        {
            return;
        }
    }

    private void BucketPaint(List<TileObject> selectedTiles, bool clickedOnTile, MapModel mapModel)
    {
        if (selectedTiles.Count == 0)
        {
            return;
        }

        if (CurrentCursor == null)
        {
            return;
        }

        Tile? startingTile = null;
        bool ignorePreviousValueOnMap = false;

        // Pick the tile value where the click was
        if (clickedOnTile)
        {
            startingTile = mapModel.RegularMapTiles.First((t) => t.CellIndex == selectedTiles[0].Index);
        }

        List<Tile> paintingTiles = [];

        if (clickedOnTile && TilesSelectedActive == Visibility.Collapsed && startingTile != null)
        {
            // Use flood fill to know what tiles do I want to change            

            selectedTiles = GetFloodFillTiles(mapModel.RegularMapTiles, startingTile, mapModel);

            selectedTiles = [.. selectedTiles.OrderBy(t => t.Index)];
        }
        else if (TilesSelectedActive == Visibility.Visible)
        {
            // Use the selected rectangle to fill the tiles in

            Rect rectangle = new(TilesSelectedOriginX, TilesSelectedOriginY, TilesSelectedWidth, TilesSelectedHeight);

            selectedTiles = CheckAreaSelected(rectangle);
        }
        else
        {
            // use the original "selectedTiles" and replace everything with in it
            ignorePreviousValueOnMap = true;
        }

        VisualMapTileVO[,] array2DOfTiles = CurrentCursor.VisualMapTiles;

        int cursorRowsCount = array2DOfTiles.GetLength(0);
        int cursorColsCount = array2DOfTiles.GetLength(1);
        int cursorColIndex = 0;
        int cursorRowIndex = 0;
        int previousIndex = -1;
        int currentIndex;
        int currentRow = 0;

        int mapSizeWidthInPixels;
        int mapSizeHeightInPixels;

        if (mapModel.MapType == MapType.Regular)
        {
            mapSizeWidthInPixels = MapUtils.GetRegularMapSizeWidthInPixels(mapModel.BckgrRegularSize);
            mapSizeHeightInPixels = MapUtils.GetRegularMapSizeHeightInPixels(mapModel.BckgrRegularSize);
        }
        else
        {
            mapSizeWidthInPixels = MapUtils.GetAffineMapSizeInPixels(mapModel.BckgrAffineSize);
            mapSizeHeightInPixels = mapSizeWidthInPixels;
        }

        // capture all the tiles for the entire canvas
        List<TileObject> entireCanvasTiles = CheckAreaSelected(new Rect(0, 0, mapSizeWidthInPixels, mapSizeHeightInPixels));

        foreach (TileObject tileObject in entireCanvasTiles)
        {
            bool isPartOfArea = selectedTiles.Exists(t => t.Index == tileObject.Index);

            currentIndex = tileObject.Index;

            int row = currentIndex / (mapSizeWidthInPixels / MapUtils.CellSize);

            if (previousIndex >= 0)
            {
                bool didReachedEndOfRow = row != currentRow;

                if (didReachedEndOfRow)
                {
                    cursorRowIndex++;
                    cursorColIndex = 0;

                    if (cursorRowIndex == cursorRowsCount)
                    {
                        cursorRowIndex = 0;
                    }

                    currentRow = row;
                }

                bool didReachedEndCursor = cursorColIndex == cursorColsCount;

                if (didReachedEndCursor)
                {
                    cursorColIndex = 0;
                }
            }

            if (isPartOfArea)
            {
                VisualMapTileVO val = array2DOfTiles[cursorRowIndex, cursorColIndex];

                if (ignorePreviousValueOnMap ||
                    startingTile == null ||
                    mapModel.RegularMapTiles[currentIndex].Equals(startingTile))
                {
                    paintingTiles.Add(new()
                    {
                        CellIndex = currentIndex,
                        BankID = CurrentCursor.BankID,
                        TileSetID = val.TileSetID,
                        TileSetOrigin = val.Point
                    });
                }
            }

            previousIndex = currentIndex;
            cursorColIndex++;
        }

        if (paintingTiles.Count > 0)
        {
            SignalManager.Get<InvalidateMapCacheSignal>().Dispatch([mapModel.MapID]);
            SignalManager.Get<RegisterHistoryActionSignal>().Dispatch(new BucketMapTilesHistoryAction(mapModel, paintingTiles));
            SignalManager.Get<PaintMapTilesSignal>().Dispatch(paintingTiles);
        }
    }

    private void EraseTiles(List<TileObject> selectedTiles, string mapID)
    {
        if (selectedTiles.Count == 0)
        {
            return;
        }

        MapModel? mapModel = GetModel();

        if (mapModel == null)
        {
            return;
        }

        SignalManager.Get<InvalidateMapCacheSignal>().Dispatch([mapID]);
        SignalManager.Get<RegisterHistoryActionSignal>().Dispatch(new DeleteMapTilesHitoryAction(mapModel, selectedTiles));
        SignalManager.Get<DeleteMapTilesSignal>().Dispatch(selectedTiles);
    }

    private void OnInvalidateMapCache(List<string> mapIDs)
    {
        foreach (string mapID in mapIDs)
        {
            MapUtils.InvalidateImageFromCache(mapID);
        }
    }

    private void OnPaintMapTiles(List<Tile> tilesToPaint)
    {
        MapModel? mapModel = GetModel();

        if (mapModel == null)
        {
            return;
        }

        // Get the list of tiles that are going to be modified
        Tile[] tiles = [.. mapModel.RegularMapTiles];

        foreach (Tile tile in tilesToPaint)
        {
            tiles[tile.CellIndex].TileSetID = tile.TileSetID;
            tiles[tile.CellIndex].TileSetOrigin = tile.TileSetOrigin;
            tiles[tile.CellIndex].BankID = tile.BankID;
        }

        ProjectItem?.FileHandler?.Save();

        LoadMapImage();
    }

    private void OnDeleteMapTiles(List<TileObject> selectedTiles)
    {
        MapModel? mapModel = GetModel();

        if (mapModel == null)
        {
            return;
        }

        // Get the list of tiles that are going to be modified
        Tile[] tiles = [.. mapModel.RegularMapTiles];

        foreach (TileObject tileObject in selectedTiles)
        {
            tiles[tileObject.Index].TileSetID = string.Empty;
            tiles[tileObject.Index].TileSetOrigin = default;
            tiles[tileObject.Index].BankID = string.Empty;
        }

        ProjectItem?.FileHandler?.Save();

        LoadMapImage();
    }

    private void OnMouseDownEvent(MouseButtonVO vO)
    {
        if (!IsActive)
        {
            return;
        }

        if (vO.Sender is not IInputElement sender)
        {
            return;
        }

        if (vO.OriginalSource is not Canvas and not Rectangle and not Image)
        {
            return;
        }

        if (vO.OriginalSource is FrameworkElement fe)
        {
            if (fe.Name != "mapCanvas" &&
                fe.Name != "imgMap")
            {
                return;
            }
        }

        Point point = vO.MouseEvent.GetPosition(sender);

        _initialMousePositionInCanvas = point;

        _isMovingFromInsideCanvas = true;
    }

    private void UpdateMouseSelectionArea(Point positionInCanvas)
    {
        if (MouseSelectionActive == Visibility.Collapsed)
        {
            MouseSelectionActive = Visibility.Visible;
        }

        if (_initialMousePositionInCanvas.X < positionInCanvas.X)
        {
            MouseSelectionOriginX = (int)_initialMousePositionInCanvas.X;
            MouseSelectionWidth = (int)(positionInCanvas.X - _initialMousePositionInCanvas.X);
        }
        else
        {
            MouseSelectionOriginX = (int)positionInCanvas.X;
            MouseSelectionWidth = (int)(_initialMousePositionInCanvas.X - positionInCanvas.X);
        }

        if (_initialMousePositionInCanvas.Y < positionInCanvas.Y)
        {
            MouseSelectionOriginY = (int)(_initialMousePositionInCanvas.Y);
            MouseSelectionHeight = (int)(positionInCanvas.Y - _initialMousePositionInCanvas.Y);
        }
        else
        {
            MouseSelectionOriginY = (int)(positionInCanvas.Y);
            MouseSelectionHeight = (int)(_initialMousePositionInCanvas.Y - positionInCanvas.Y);
        }
    }

    private TileObject GetSelectedVisualTile(Point position, MapModel model)
    {
        int cellIndex = MapUtils.GetCellIndexFromPoint(position, model);

        return Tiles[cellIndex];
    }

    private List<TileObject> CheckAreaSelected(Rect rectangle)
    {
        List<TileObject> tiles = [];

        MapModel? model = GetModel();

        if (model == null)
        {
            return tiles;
        }

        if (rectangle.Width == 0 ||
            rectangle.Height == 0)
        {
            return tiles;
        }

        LimitAreaSelectionToCanvasSize();
        LimitRectangleToCanvasSize(ref rectangle);

        List<int> tilesInRect = MapUtils.GetCellsIndicesFromRect(rectangle, model);

        if (tilesInRect.Count > 0)
        {
            foreach (int cellIndex in tilesInRect)
            {
                tiles.Add(Tiles[cellIndex]);
            }
        }

        return tiles;
    }

    private void LimitRectangleToCanvasSize(ref Rect rectangle)
    {
        if (rectangle.X < 0)
        {
            rectangle.Width += rectangle.X;
            rectangle.X = 0;
        }

        if (rectangle.Y < 0)
        {
            rectangle.Height += rectangle.Y;
            rectangle.Y = 0;
        }

        if ((rectangle.Width + rectangle.X) > CanvasWidth)
        {
            rectangle.Width = CanvasWidth - rectangle.X;
        }

        if ((rectangle.Height + rectangle.Y) > CanvasHeight)
        {
            rectangle.Height = CanvasHeight - rectangle.Y;
        }
    }

    private void LimitAreaSelectionToCanvasSize()
    {
        if (MouseSelectionOriginX < 0)
        {
            MouseSelectionWidth += MouseSelectionOriginX;
            MouseSelectionOriginX = 0;
        }

        if (MouseSelectionOriginY < 0)
        {
            MouseSelectionHeight += MouseSelectionOriginY;
            MouseSelectionOriginY = 0;
        }

        if ((MouseSelectionWidth + MouseSelectionOriginX) > CanvasWidth)
        {
            MouseSelectionWidth = CanvasWidth - MouseSelectionOriginX;
        }

        if ((MouseSelectionHeight + MouseSelectionOriginY) > CanvasHeight)
        {
            MouseSelectionHeight = CanvasHeight - MouseSelectionOriginY;
        }
    }

    private void OnSelectTiles(TileObject[] tiles)
    {
        if (tiles.Length == 0)
        {
            TilesSelectedActive = Visibility.Collapsed;

            return;
        }

        MapModel? model = GetModel();

        if (model == null)
        {
            return;
        }

        int width = 0;
        int height = 0;
        int continuousIndex = -1;
        int accumulativeWidth = 0;
        int widthInPixels;

        if (model.MapType == MapType.Regular)
        {
            widthInPixels = MapUtils.GetRegularMapSizeWidthInPixels(model.BckgrRegularSize);
        }
        else
        {
            widthInPixels = MapUtils.GetAffineMapSizeInPixels(model.BckgrAffineSize);
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            if (continuousIndex == -1)
            {
                width += MapUtils.CellSize;
                height += MapUtils.CellSize;
                continuousIndex = tiles[i].Index;

                accumulativeWidth = MapUtils.CellSize;
            }
            else if (continuousIndex + 1 == tiles[i].Index &&
                accumulativeWidth < widthInPixels)
            {
                continuousIndex++;

                accumulativeWidth += MapUtils.CellSize;

                // This is to check the most left boundary, so while
                // the hight is still 8 pixels we count for the total width
                if (height == MapUtils.CellSize)
                {
                    width += MapUtils.CellSize;
                }
            }
            else
            {
                continuousIndex = tiles[i].Index;
                height += MapUtils.CellSize;

                accumulativeWidth = MapUtils.CellSize;
            }
        }

        SelectedTile = tiles.Length == 1 ? tiles[0] : null;

        Point origin = MapUtils.GetCellPointFromIndex(tiles[0].Index, model);

        TilesSelectedActive = Visibility.Visible;
        TilesSelectedWidth = width;
        TilesSelectedHeight = height;
        TilesSelectedOriginX = (int)origin.X;
        TilesSelectedOriginY = (int)origin.Y;
    }

    public List<TileObject> GetFloodFillTiles(List<Tile> map, Tile startingTile, MapModel model)
    {
        List<TileObject> tiles = [];
        Queue<(int x, int y, int index)> queue = [];
        HashSet<int> visited = [];

        int mapWidth;
        int mapHeight;

        if (model.MapType == MapType.Regular)
        {
            mapWidth = MapUtils.GetRegularMapSizeWidth(model.BckgrRegularSize);
            mapHeight = MapUtils.GetRegularMapSizeHeight(model.BckgrRegularSize);
        }
        else
        {
            mapWidth = MapUtils.GetAffineMapSize(model.BckgrAffineSize);
            mapHeight = mapWidth;
        }

        int x = startingTile.CellIndex % mapWidth;
        int y = startingTile.CellIndex / mapWidth;

        queue.Enqueue((x, y, startingTile.CellIndex));
        visited.Add(startingTile.CellIndex);

        // 4-directional offsets
        int[] dx = [-1, 1, 0, 0];
        int[] dy = [0, 0, -1, 1];

        while (queue.Count > 0)
        {
            (int currX, int currY, int cellIndex) = queue.Dequeue();

            tiles.Add(Tiles[cellIndex]);

            for (int i = 0; i < 4; i++)
            {
                int newX = currX + dx[i];
                int newY = currY + dy[i];

                if (newX >= 0 && newX < mapWidth &&
                    newY >= 0 && newY < mapHeight)
                {
                    int index = (newY * mapWidth) + newX;

                    if (!visited.Contains(index) && map[index].Equals(startingTile))
                    {
                        queue.Enqueue((newX, newY, index));
                        visited.Add(index);
                    }
                }
            }
        }

        return tiles;
    }
}
