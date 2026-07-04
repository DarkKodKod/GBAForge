using ArchitectureLibrary.Signals;
using GBATool.Commands.Banks;
using GBATool.Commands.Input;
using GBATool.Enums;
using GBATool.FileSystem;
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

    public int Index { get; set; }
    public string MapID { get; set; } = string.Empty;
    public int MapIndex { get; set; }

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
    private BckgrRegularSize _bckgrRegularSize = BckgrRegularSize.Regular32x32;
    private BckgrAffineSize _bckgrAffineSize = BckgrAffineSize.Affine16x16;
    private BindingList<TileObject> _tiles0 = [];
    private BindingList<TileObject> _tiles1 = [];
    private BindingList<TileObject> _tiles2 = [];
    private BindingList<TileObject> _tiles3 = [];
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
    private (string, int)[] _selectedTiles = [];
    private Visibility _tilesSelectedActive;
    private int _tilesSelectedWidth;
    private int _tilesSelectedHeight;
    private int _tilesSelectedOriginX;
    private int _tilesSelectedOriginY;
    private MapFunctionality _currentMapFunctionality = MapFunctionality.Select;
    private bool _isMovingFromInsideCanvas;

    public enum MapFunctionality
    {
        Select,
        Move,
        Paint,
        BucketPaint,
        Erase
    }

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
                    case BckgrRegularSize.Regular64x32:
                        CanvasHeight = 256;
                        CanvasWidth = 512;
                        break;
                    case BckgrRegularSize.Regular32x64:
                        CanvasWidth = 256;
                        CanvasHeight = 512;
                        break;
                    case BckgrRegularSize.Regular64x64:
                        CanvasHeight = CanvasWidth = 512;
                        break;
                    case BckgrRegularSize.Regular32x32:
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

    public BindingList<TileObject> Tiles0
    {
        get => _tiles0;
        set
        {
            _tiles0 = value;

            OnPropertyChanged(nameof(Tiles0));
        }
    }

    public BindingList<TileObject> Tiles1
    {
        get => _tiles1;
        set
        {
            _tiles1 = value;

            OnPropertyChanged(nameof(Tiles1));
        }
    }

    public BindingList<TileObject> Tiles2
    {
        get => _tiles2;
        set
        {
            _tiles2 = value;

            OnPropertyChanged(nameof(Tiles2));
        }
    }

    public BindingList<TileObject> Tiles3
    {
        get => _tiles3;
        set
        {
            _tiles3 = value;

            OnPropertyChanged(nameof(Tiles3));
        }
    }

    public (string, int)[] SelectedTiles
    {
        get => _selectedTiles;
        set
        {
            _selectedTiles = value;

            OnPropertyChanged(nameof(SelectedTiles));
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
            .. ProjectFiles.GetModels<PaletteModel>(),
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
        #endregion

        MapModel? model = GetModel();

        if (model == null)
        {
            return;
        }

        if (model.Tiles.Count == 0)
        {
            model.InsertNewTiles();

            ProjectItem?.FileHandler?.Save();
        }

        _doNotSave = true;

        MapType = model.MapType;
        Priority = model.Priority;
        BckgrRegularSize = model.BckgrRegularSize;
        BckgrAffineSize = model.BckgrAffineSize;

        int mapIndex = 0;
        foreach (KeyValuePair<string, Tile[]> map in model.Tiles)
        {
            switch (mapIndex)
            {
                case 0:
                    {
                        int tileIndex = 0;
                        foreach (Tile item in map.Value)
                        {
                            Tiles0.Add(new()
                            {
                                Index = tileIndex++,
                                MapID = map.Key,
                                MapIndex = item.MapIndex
                            });
                        }
                    }
                    break;
                case 1:
                    {
                        int tileIndex = 0;
                        foreach (Tile item in map.Value)
                        {
                            Tiles1.Add(new()
                            {
                                Index = tileIndex++,
                                MapID = map.Key,
                                MapIndex = item.MapIndex
                            });
                        }
                    }
                    break;
                case 2:
                    {
                        int tileIndex = 0;
                        foreach (Tile item in map.Value)
                        {
                            Tiles2.Add(new()
                            {
                                Index = tileIndex++,
                                MapID = map.Key,
                                MapIndex = item.MapIndex
                            });
                        }
                    }
                    break;
                case 3:
                    {
                        int tileIndex = 0;
                        foreach (Tile item in map.Value)
                        {
                            Tiles3.Add(new()
                            {
                                Index = tileIndex++,
                                MapID = map.Key,
                                MapIndex = item.MapIndex
                            });
                        }
                    }
                    break;
                default: break;
            }

            mapIndex++;
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
        #endregion

        base.OnDeactivate();
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

        if (vO.OriginalSource is FrameworkElement fe)
        {
            if (fe.Name != "mapCanvas")
            {
                return;
            }
        }

        Point positionInCanvas = vO.EventArgs.GetPosition(sender);

        switch (CurrentMapFunctionality)
        {
            case MapFunctionality.Select:
                MouseMoveSelection(positionInCanvas);
                break;
            default:
                break;
        }
    }

    private void MouseMoveSelection(Point currentMousePosition)
    {
        if (Util.AboutEqual(currentMousePosition.X, _initialMousePositionInCanvas.X) &&
           Util.AboutEqual(currentMousePosition.Y, _initialMousePositionInCanvas.Y))
        {
            return;
        }

        SignalManager.Get<TryCaptureMouseSignal>().Dispatch();

        UpdateMouseSelectionArea(currentMousePosition);
    }

    private void OnMouseUpEvent(MouseButtonVO vO)
    {
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

        if (vO.OriginalSource is FrameworkElement fe)
        {
            if (fe.Name != "mapCanvas")
            {
                return;
            }
        }

        SignalManager.Get<TryReleaseMouseSignal>().Dispatch();

        List<TileObject> selectedTiles = [];

        if (MouseSelectionActive == Visibility.Visible)
        {
            selectedTiles = CheckMouseAreaSelected();
        }

        Point pos = vO.MouseEvent.GetPosition(sender);

        if (pos.X < 0)
        {
            pos.X = 0;
        }
        if (pos.Y < 0)
        {
            pos.Y = 0;
        }

        SignalManager.Get<ResetSelectionAreaSignal>().Dispatch(pos);

        if (selectedTiles.Count == 0 &&
            pos.X >= 0 && pos.Y >= 0)
        {
            GetSelectedTile(pos, ref selectedTiles);
        }

        SignalManager.Get<SelectTilesSignal>().Dispatch([.. selectedTiles]);

        _isMovingFromInsideCanvas = false;
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

        if (vO.OriginalSource is not Canvas and not Rectangle)
        {
            return;
        }

        if (vO.OriginalSource is FrameworkElement fe)
        {
            if (fe.Name != "mapCanvas")
            {
                return;
            }
        }

        Point point = vO.MouseEvent.GetPosition(sender);

        _initialMousePositionInCanvas = point;

        SignalManager.Get<ResetSelectionAreaSignal>().Dispatch(_initialMousePositionInCanvas);

        SignalManager.Get<SelectTilesSignal>().Dispatch([]);

        _isMovingFromInsideCanvas = true;
    }

    private void SaveTileProperties(TileObject tile)
    {
        if (_doNotSave)
            return;

        MapModel? model = GetModel();

        if (model == null)
        {
            return;
        }

        MapUtils.InvalidateImageFromCache(tile.MapID);

        //        sprite.FlipHorizontal = IsFlippedHorizontal;
        //        sprite.FlipVertical = IsFlippedVertical;
        //        sprite.GraphicMode = GraphicMode;
        //        sprite.ObjectMode = ObjectMode;
        //        sprite.EnableMosaic = IsEnableMosaic;
        //
        //        SignalManager.Get<UpdateSpriteVisualPropertiesSignal>().Dispatch(sprite.SpriteID, sprite.FlipHorizontal, sprite.FlipVertical);

        ProjectItem?.FileHandler?.Save();
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

    private void GetSelectedTile(Point position, ref List<TileObject> selectedTiles)
    {
        int cellIndex = MapUtils.GetCellIndexFromPoint(position);

        if (!selectedTiles.Contains(Tiles0[cellIndex]))
        {
            selectedTiles.Add(Tiles0[cellIndex]);
        }
    }

    private List<TileObject> CheckMouseAreaSelected()
    {
        List<TileObject> tiles = [];

        if (MouseSelectionWidth == 0 || 
            MouseSelectionHeight == 0)
        {
            return tiles;
        }

        LimitAreaSelectionToCanvasSize();

        Rect rectangle = new(
            MouseSelectionOriginX,
            MouseSelectionOriginY,
            MouseSelectionWidth,
            MouseSelectionHeight);

        List<int> tilesInRect = MapUtils.GetCellsIndicesFromRect(rectangle);

        if (tilesInRect.Count > 0)
        {
            foreach (int cellIndex in tilesInRect)
            {
                tiles.Add(Tiles0[cellIndex]);
            }
        }

        return tiles;
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

            SelectedTiles = [];

            return;
        }

        List<(string, int)> selectedTiles = [];

        int width = 0;
        int height = 0;
        int continuousIndex = 0;

        for (int i = 0; i < tiles.Length; i++)
        {
            if (continuousIndex == 0)
            {
                width += 8;
                height += 8;
                continuousIndex = tiles[i].Index;
            }
            else if (continuousIndex + 1 == tiles[i].Index)
            {
                continuousIndex++;

                if (height == 8)
                {
                    width += 8;
                }
            }
            else
            {
                continuousIndex = tiles[i].Index;
                height += 8;
            }

            selectedTiles.Add((tiles[i].MapID, tiles[i].Index));
        }

        SelectedTile = tiles.Length == 1 ? tiles[0] : null;

        SelectedTiles = [.. selectedTiles];

        Point origin = MapUtils.GetCellPointFromIndex(tiles[0].Index, tiles[0].MapIndex);

        TilesSelectedActive = Visibility.Visible;
        TilesSelectedWidth = width;
        TilesSelectedHeight = height;
        TilesSelectedOriginX = (int)origin.X;
        TilesSelectedOriginY = (int)origin.Y;
    }
}
