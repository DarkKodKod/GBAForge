using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using GBATool.Commands.Input;
using GBATool.Commands.Utils;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBATool.Views;

/// <summary>
/// Interaction logic for BankViewerView.xaml
/// </summary>
public partial class BankViewerView : UserControl, INotifyPropertyChanged
{
    private float _scale = 2.0f;
    private ImageSource? _bankImage;
    private Visibility _spriteRectVisibility = Visibility.Collapsed;
    private double _spriteRectLeft;
    private double _spriteRectWidth;
    private double _spriteRectHeight;
    private double _spriteRectTop;
    private Visibility _spriteRectVisibility2 = Visibility.Collapsed;
    private double _spriteRectLeft2;
    private double _spriteRectWidth2;
    private double _spriteRectHeight2;
    private double _spriteRectTop2;
    private Visibility _spriteRectVisibility3 = Visibility.Collapsed;
    private double _spriteRectLeft3;
    private double _spriteRectWidth3;
    private double _spriteRectHeight3;
    private double _spriteRectTop3;
    private SpriteModel? _selectedSpriteFromBank = null;
    private int _canvasHeght = 256;
    private int _canvasWidth = 256;
    private SpriteModel? _cacheSelectedSpriteFromBank = null;
    private BankImageMetaData? _metaData = null;
    private BankModel? _bankModel = null;
    private SpriteModel? _selectedSprite = null;
    private Visibility _mouseSelectionActive;
    private int _mouseSelectionOriginX;
    private int _mouseSelectionOriginY;
    private int _mouseSelectionWidth;
    private int _mouseSelectionHeight;
    private Point? _initialMousePositionInCanvas = null;

    public event PropertyChangedEventHandler? PropertyChanged;

    public static readonly DependencyProperty Force2DViewProperty = DependencyProperty.Register(
        "Force2DView",
        typeof(bool),
        typeof(BankViewerView),
        new PropertyMetadata(default(bool)));

    public static readonly DependencyProperty IndividualSelectionProperty = DependencyProperty.Register(
        "IndividualSelection",
        typeof(bool),
        typeof(BankViewerView),
        new PropertyMetadata(default(bool)));

    #region Commands
    public ImageMouseDownCommand ImageMouseDownCommand { get; } = new();
    public MouseEventCommand<MouseMoveEventSignal> MouseMoveCommand { get; } = new();
    public MouseButtonEventCommand<MouseUpEventSignal> MouseUpCommand { get; } = new();
    #endregion

    #region get/set
    public BankImageMetaData? MetaData { get => _metaData; }

    public Canvas Canvas { get => canvas; }

    public SpriteModel? SelectedSprite
    {
        get => _selectedSprite;
        set
        {
            _selectedSprite = value;

            OnPropertyChanged(nameof(SelectedSprite));
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

    public bool IndividualSelection
    {
        get => (bool)GetValue(IndividualSelectionProperty);
        set => SetValue(IndividualSelectionProperty, value);
    }

    public bool Force2DView
    {
        get => (bool)GetValue(Force2DViewProperty);
        set => SetValue(Force2DViewProperty, value);
    }

    public SpriteModel? SelectedSpriteFromBank
    {
        get => _selectedSpriteFromBank;
        set
        {
            HideRectangleSelection();

            _selectedSpriteFromBank = value;

            if (value is not null)
            {
                _cacheSelectedSpriteFromBank = value;

                ShowSelectionOverSprite(_selectedSpriteFromBank);
            }

            OnPropertyChanged(nameof(SelectedSpriteFromBank));
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

    public ImageSource? BankImage
    {
        get => _bankImage;
        set
        {
            _bankImage = value;

            OnPropertyChanged(nameof(BankImage));
        }
    }

    public Visibility SpriteRectVisibility
    {
        get { return _spriteRectVisibility; }
        set
        {
            _spriteRectVisibility = value;

            OnPropertyChanged(nameof(SpriteRectVisibility));
        }
    }

    public double SpriteRectLeft
    {
        get { return _spriteRectLeft; }
        set
        {
            _spriteRectLeft = value;

            OnPropertyChanged(nameof(SpriteRectLeft));
        }
    }

    public double SpriteRectWidth
    {
        get { return _spriteRectWidth; }
        set
        {
            _spriteRectWidth = value;

            OnPropertyChanged(nameof(SpriteRectWidth));
        }
    }

    public double SpriteRectHeight
    {
        get { return _spriteRectHeight; }
        set
        {
            _spriteRectHeight = value;

            OnPropertyChanged(nameof(SpriteRectHeight));
        }
    }

    public double SpriteRectTop
    {
        get { return _spriteRectTop; }
        set
        {
            _spriteRectTop = value;

            OnPropertyChanged(nameof(SpriteRectTop));
        }
    }

    public Visibility SpriteRectVisibility2
    {
        get { return _spriteRectVisibility2; }
        set
        {
            _spriteRectVisibility2 = value;

            OnPropertyChanged(nameof(SpriteRectVisibility2));
        }
    }

    public double SpriteRectLeft2
    {
        get { return _spriteRectLeft2; }
        set
        {
            _spriteRectLeft2 = value;

            OnPropertyChanged(nameof(SpriteRectLeft2));
        }
    }

    public double SpriteRectWidth2
    {
        get { return _spriteRectWidth2; }
        set
        {
            _spriteRectWidth2 = value;

            OnPropertyChanged(nameof(SpriteRectWidth2));
        }
    }

    public double SpriteRectHeight2
    {
        get { return _spriteRectHeight2; }
        set
        {
            _spriteRectHeight2 = value;

            OnPropertyChanged(nameof(SpriteRectHeight2));
        }
    }

    public double SpriteRectTop2
    {
        get { return _spriteRectTop2; }
        set
        {
            _spriteRectTop2 = value;

            OnPropertyChanged(nameof(SpriteRectTop2));
        }
    }

    public Visibility SpriteRectVisibility3
    {
        get { return _spriteRectVisibility3; }
        set
        {
            _spriteRectVisibility3 = value;

            OnPropertyChanged(nameof(SpriteRectVisibility3));
        }
    }

    public double SpriteRectLeft3
    {
        get { return _spriteRectLeft3; }
        set
        {
            _spriteRectLeft3 = value;

            OnPropertyChanged(nameof(SpriteRectLeft3));
        }
    }

    public double SpriteRectWidth3
    {
        get { return _spriteRectWidth3; }
        set
        {
            _spriteRectWidth3 = value;

            OnPropertyChanged(nameof(SpriteRectWidth3));
        }
    }

    public double SpriteRectHeight3
    {
        get { return _spriteRectHeight3; }
        set
        {
            _spriteRectHeight3 = value;

            OnPropertyChanged(nameof(SpriteRectHeight3));
        }
    }

    public double SpriteRectTop3
    {
        get { return _spriteRectTop3; }
        set
        {
            _spriteRectTop3 = value;

            OnPropertyChanged(nameof(SpriteRectTop3));
        }
    }
    #endregion

    public BankViewerView()
    {
        InitializeComponent();
    }

    public void OnActivate()
    {
        #region Signals
        SignalManager.Get<MouseImageSelectedSignal>().Listener += OnMouseImageSelected;
        SignalManager.Get<BankSpriteDeletedSignal>().Listener += OnBankSpriteDeleted;
        SignalManager.Get<ReloadBankImageSignal>().Listener += OnReloadBankImage;
        SignalManager.Get<BankImageUpdatedSignal>().Listener += OnBankImageUpdated;
        SignalManager.Get<ObtainTransparentColorSignal>().Listener += OnObtainTransparentColor;
        SignalManager.Get<GeneratePaletteFromBankSignal>().Listener += OnGeneratePaletteFromBank;
        SignalManager.Get<SetBankModelToBankViewerSignal>().Listener += OnSetBankModelToBankViewer;
        SignalManager.Get<CleanUpSpriteListSignal>().Listener += OnCleanUpSpriteList;
        SignalManager.Get<SelectSpriteSignal>().Listener += OnSelectSprite;
        SignalManager.Get<AdjustCanvasBankSizeSignal>().Listener += OnAdjustCanvasBankSize;
        SignalManager.Get<ReloadBankViewImageSignal>().Listener += OnReloadBankViewImage;
        SignalManager.Get<RemoveSpriteSelectionFromBank>().Listener += OnRemoveSpriteSelectionFromBank;
        SignalManager.Get<MouseMoveEventSignal>().Listener += OnMouseMove;
        SignalManager.Get<MouseUpEventSignal>().Listener += OnMouseUp;
        SignalManager.Get<TryCaptureMouseSignal>().Listener += OnTryCaptureMouse;
        SignalManager.Get<TryReleaseMouseSignal>().Listener += OnTryReleaseMouse;
        #endregion

        MouseSelectionActive = Visibility.Collapsed;

        SelectedSprite = null;
    }

    public void OnDeactivate()
    {
        #region Signals
        SignalManager.Get<MouseImageSelectedSignal>().Listener -= OnMouseImageSelected;
        SignalManager.Get<BankSpriteDeletedSignal>().Listener -= OnBankSpriteDeleted;
        SignalManager.Get<ReloadBankImageSignal>().Listener -= OnReloadBankImage;
        SignalManager.Get<BankImageUpdatedSignal>().Listener -= OnBankImageUpdated;
        SignalManager.Get<SelectSpriteSignal>().Listener -= OnSelectSprite;
        SignalManager.Get<ObtainTransparentColorSignal>().Listener -= OnObtainTransparentColor;
        SignalManager.Get<GeneratePaletteFromBankSignal>().Listener -= OnGeneratePaletteFromBank;
        SignalManager.Get<SetBankModelToBankViewerSignal>().Listener -= OnSetBankModelToBankViewer;
        SignalManager.Get<CleanUpSpriteListSignal>().Listener -= OnCleanUpSpriteList;
        SignalManager.Get<AdjustCanvasBankSizeSignal>().Listener -= OnAdjustCanvasBankSize;
        SignalManager.Get<ReloadBankViewImageSignal>().Listener -= OnReloadBankViewImage;
        SignalManager.Get<RemoveSpriteSelectionFromBank>().Listener -= OnRemoveSpriteSelectionFromBank;
        SignalManager.Get<MouseMoveEventSignal>().Listener -= OnMouseMove;
        SignalManager.Get<MouseUpEventSignal>().Listener -= OnMouseUp;
        SignalManager.Get<TryCaptureMouseSignal>().Listener -= OnTryCaptureMouse;
        SignalManager.Get<TryReleaseMouseSignal>().Listener -= OnTryReleaseMouse;
        #endregion

        MouseSelectionActive = Visibility.Collapsed;
    }

    private void OnRemoveSpriteSelectionFromBank()
    {
        _cacheSelectedSpriteFromBank = null;
        SelectedSpriteFromBank = null;
    }

    private void OnSelectSprite(SpriteVO sprite)
    {
        if (sprite == null)
            return;

        if (string.IsNullOrEmpty(sprite.TileSetID))
            return;

        TileSetModel? model = ProjectFiles.GetModel<TileSetModel>(sprite.TileSetID);

        if (model == null)
        {
            return;
        }

        foreach (SpriteModel item in model.Sprites)
        {
            if (item.ID == sprite.SpriteID)
            {
                SelectedSprite = item;
                break;
            }
        }
    }

    private void OnCleanUpSpriteList()
    {
        SelectedSprite = null;
    }

    private void OnSetBankModelToBankViewer(BankModel? bankModel)
    {
        if (bankModel == null)
        {
            return;
        }

        if (_bankModel?.GUID == bankModel.GUID)
        {
            return;
        }

        _bankModel = bankModel;

        AdjustCanvasHeight(_bankModel.BitsPerPixel);

        LoadImage(bankModel);
    }

    private void OnReloadBankViewImage()
    {
        ReloadImage();
    }

    private void HideRectangleSelection()
    {
        SpriteRectVisibility = Visibility.Collapsed;
        SpriteRectVisibility2 = Visibility.Collapsed;
        SpriteRectVisibility3 = Visibility.Collapsed;
    }

    private void OnTryCaptureMouse(string name)
    {
        if (name != "imgBank")
        {
            return;
        }

        if (!imgBank.IsMouseCaptured)
        {
            imgBank.CaptureMouse();
        }
    }

    private void OnTryReleaseMouse(string name)
    {
        if (name != "imgBank")
        {
            return;
        }

        if (imgBank.IsMouseCaptured)
        {
            imgBank.ReleaseMouseCapture();
        }
    }

    private void OnMouseUp(MouseButtonVO vo)
    {
        string sourceName = string.Empty;

        if (vo.OriginalSource is FrameworkElement fe)
        {
            if (fe.Name != "imgBank")
            {
                return;
            }

            sourceName = fe.Name;
        }

        if (!IndividualSelection)
        {
            return;
        }

        _initialMousePositionInCanvas = null;

        if (vo.Sender == null)
        {
            return;
        }

        if (_metaData == null)
        {
            return;
        }

        SignalManager.Get<TryReleaseMouseSignal>().Dispatch(sourceName);

        Image? image = Util.FindAncestor<Image>((DependencyObject)vo.Sender);

        if (image == null)
        {
            return;
        }

        SelectArbitrarySizeOfTiles(image);

        CleanUpSelectionRect();
    }

    private void CleanUpSelectionRect()
    {
        MouseSelectionActive = Visibility.Collapsed;
        MouseSelectionOriginX = 0;
        MouseSelectionOriginY = 0;
        MouseSelectionWidth = 0;
        MouseSelectionHeight = 0;
    }

    private void SelectArbitrarySizeOfTiles(Image? image)
    {
        if (_metaData == null)
        {
            return;
        }

        int x = (int)Math.Floor((double)MouseSelectionOriginX / BankUtils.SizeOfCellInPixels) * BankUtils.SizeOfCellInPixels;
        int y = (int)Math.Floor((double)MouseSelectionOriginY / BankUtils.SizeOfCellInPixels) * BankUtils.SizeOfCellInPixels;
        int w = (int)Math.Ceiling((double)MouseSelectionWidth / BankUtils.SizeOfCellInPixels) * BankUtils.SizeOfCellInPixels;
        int h = (int)Math.Ceiling((double)MouseSelectionHeight / BankUtils.SizeOfCellInPixels) * BankUtils.SizeOfCellInPixels;

        if (x == 0 && y == 0 && w == 0 && h == 0)
        {
            // select just the single tile
            Point pos = Mouse.GetPosition(image);

            x = (int)Math.Floor(pos.X / BankUtils.SizeOfCellInPixels) * BankUtils.SizeOfCellInPixels;
            y = (int)Math.Floor(pos.Y / BankUtils.SizeOfCellInPixels) * BankUtils.SizeOfCellInPixels;
            w = MapUtils.CellSize;
            h = MapUtils.CellSize;
        }

        SpriteRectLeft = x;
        SpriteRectTop = y;
        SpriteRectWidth = w;
        SpriteRectHeight = h;
        SpriteRectVisibility = Visibility.Visible;

        WriteableBitmap cropped = _metaData.image.Crop(x, y, w, h);

        using (cropped.GetBitmapContext())
        {
            Image imageCtrl = new()
            {
                Source = cropped
            };

            SignalManager.Get<UseBitmapAsCursorSignal>().Dispatch(imageCtrl);
        }
    }

    private void OnMouseMove(MouseEventVO vo)
    {
        if (!IndividualSelection)
        {
            return;
        }

        if (vo.Sender == null)
        {
            return;
        }

        if (vo.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        if (_metaData == null)
        {
            return;
        }

        string sourceName = string.Empty;

        if (vo.OriginalSource is FrameworkElement fe)
        {
            if (fe.Name != "imgBank")
            {
                return;
            }

            sourceName = fe.Name;
        }

        Image? image = Util.FindAncestor<Image>((DependencyObject)vo.Sender);

        if (image == null)
        {
            return;
        }

        Point pos = vo.EventArgs.GetPosition(image);

        if (_initialMousePositionInCanvas == null)
        {
            _initialMousePositionInCanvas = pos;

            MouseSelectionActive = Visibility.Collapsed;
            MouseSelectionOriginX = (int)pos.X;
            MouseSelectionOriginY = (int)pos.Y;
            MouseSelectionWidth = 0;
            MouseSelectionHeight = 0;
        }

        UpdateMouseSelectionArea(pos);

        SignalManager.Get<TryCaptureMouseSignal>().Dispatch(sourceName);
    }

    private void UpdateMouseSelectionArea(Point currentPositionInCanvas)
    {
        if (_initialMousePositionInCanvas == null)
        {
            return;
        }

        Point point = _initialMousePositionInCanvas ?? new Point();

        if (Util.AboutEqual(currentPositionInCanvas.X, point.X) &&
           Util.AboutEqual(currentPositionInCanvas.Y, point.Y))
        {
            return;
        }

        if (MouseSelectionActive == Visibility.Collapsed)
        {
            MouseSelectionActive = Visibility.Visible;
        }

        if (point.X < currentPositionInCanvas.X)
        {
            MouseSelectionOriginX = (int)point.X;
            MouseSelectionWidth = (int)(currentPositionInCanvas.X - point.X);
        }
        else
        {
            MouseSelectionOriginX = (int)currentPositionInCanvas.X;
            MouseSelectionWidth = (int)(point.X - currentPositionInCanvas.X);
        }

        if (point.Y < currentPositionInCanvas.Y)
        {
            MouseSelectionOriginY = (int)(point.Y);
            MouseSelectionHeight = (int)(currentPositionInCanvas.Y - point.Y);
        }
        else
        {
            MouseSelectionOriginY = (int)(currentPositionInCanvas.Y);
            MouseSelectionHeight = (int)(point.Y - currentPositionInCanvas.Y);
        }
    }

    private void OnMouseImageSelected(Image image, Point point)
    {
        if (image.Name != "imgBank")
        {
            return;
        }

        SelectedSpriteFromBank = null;
        _cacheSelectedSpriteFromBank = null;

        if (!IndividualSelection)
        {
            SelectSpriteFromBankImage(point);
        }
    }

    private void OnBankSpriteDeleted(SpriteModel spriteToDelete)
    {
        if (_bankModel == null)
        {
            return;
        }

        bool ret = _bankModel.RemoveSprite(spriteToDelete);

        if (ret)
        {
            _cacheSelectedSpriteFromBank = null;

            ReloadImage();
        }
    }

    private void OnAdjustCanvasBankSize(BitsPerPixel bitPerPixel)
    {
        AdjustCanvasHeight(bitPerPixel);
    }

    private void AdjustCanvasHeight(BitsPerPixel bitPerPixel)
    {
        if (bitPerPixel == BitsPerPixel.f8bpp)
        {
            CanvasHeight = 128;
        }
        else
        {
            CanvasHeight = 256;
        }
    }

    private static void ReloadImage()
    {
        SignalManager.Get<CleanupBankSpritesSignal>().Dispatch();
        SignalManager.Get<CleanupTileSetLinksSignal>().Dispatch();
        SignalManager.Get<BankImageUpdatedSignal>().Dispatch();
    }

    private void OnReloadBankImage()
    {
        ReloadImage();
    }

    private void LoadImage(BankModel model)
    {
        SignalManager.Get<CleanupTileSetLinksSignal>().Dispatch();

        _metaData = BankUtils.CreateImage(model, Force2DView, CanvasWidth, CanvasHeight);

        if (_metaData == null)
        {
            return;
        }

        BankImage = _metaData.image;

        SignalManager.Get<UpdateBankViewerParentWithImageMetadataSignal>().Dispatch(_metaData);
    }

    private void OnBankImageUpdated()
    {
        if (_bankModel == null)
        {
            return;
        }

        LoadImage(_bankModel);

        SelectedSpriteFromBank = null;

        if (_cacheSelectedSpriteFromBank != null)
        {
            SelectedSpriteFromBank = _cacheSelectedSpriteFromBank;
        }
    }

    protected virtual void OnPropertyChanged(string propname)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
    }

    private bool Is1DImage()
    {
        if (Force2DView)
        {
            return false;
        }

        if (_bankModel == null)
        {
            return false;
        }

        ProjectModel projectModel = ModelManager.Get<ProjectModel>();

        bool is1DImage = _bankModel.IsBackground || projectModel.SpritePatternFormat == SpritePattern.Format1D;

        return is1DImage;
    }

    private void ShowSelectionOverSprite(SpriteModel? spriteModel)
    {
        if (_metaData == null || string.IsNullOrEmpty(spriteModel?.ID))
        {
            return;
        }

        if (Is1DImage())
        {
            int canvasWidthInCells = CanvasWidth / BankUtils.SizeOfCellInPixels;
            int firstIndex = 0;
            int spriteTilesTotal = 0;

            foreach (SpriteModel sprite in _metaData.bankSprites)
            {
                if (sprite.Shape == SpriteShape.Custom || sprite.Size == SpriteSize.Custom)
                {
                    spriteTilesTotal = (sprite.Width / 8) * (sprite.Height / 8);
                }
                else
                {
                    spriteTilesTotal = SpriteUtils.Count8x8Tiles(sprite.Shape, sprite.Size);
                }

                if (sprite.ID == spriteModel?.ID)
                {
                    break;
                }

                firstIndex += spriteTilesTotal;
            }

            ShowRectOverSprite1D(spriteTilesTotal, firstIndex, canvasWidthInCells);
        }
        else
        {
            // 2d
            SpriteRectVisibility = Visibility.Visible;

            int width = 0;
            int height = 0;

            if (spriteModel.Shape == SpriteShape.Custom || spriteModel.Size == SpriteSize.Custom)
            {
                width = spriteModel.Width;
                height = spriteModel.Height;
            }
            else
            {
                SpriteUtils.ConvertToWidthHeight(spriteModel.Shape, spriteModel.Size, ref width, ref height);
            }

            // Going through the list again to find the first occurrence of this item
            (int firstIndex, string _, string _) = _metaData.SpriteIndices.Find(item => item.Item2 == spriteModel.ID);

            int canvasWidthInCells = CanvasWidth / BankUtils.SizeOfCellInPixels;
            int left = (firstIndex % canvasWidthInCells) * BankUtils.SizeOfCellInPixels;
            int top = (firstIndex / BankUtils.MaxTextureCellsWidth) * BankUtils.SizeOfCellInPixels;

            SpriteRectLeft = left;
            SpriteRectWidth = width;
            SpriteRectHeight = height;
            SpriteRectTop = top;
        }
    }

    private void SelectSpriteFromBankImage(Point point)
    {
        if (_metaData == null)
        {
            return;
        }

        int x = (int)Math.Floor(point.X / BankUtils.SizeOfCellInPixels) * BankUtils.SizeOfCellInPixels;
        int y = (int)Math.Floor(point.Y / BankUtils.SizeOfCellInPixels) * BankUtils.SizeOfCellInPixels;

        int canvasWidthInCells = CanvasWidth / BankUtils.SizeOfCellInPixels;
        int lengthHeight = CanvasHeight / BankUtils.SizeOfCellInPixels;
        int yPos = (y / BankUtils.SizeOfCellInPixels);
        int xPos = (x / BankUtils.SizeOfCellInPixels);

        int cellIndex = (canvasWidthInCells * yPos) + xPos;

        (int _, string spriteID, string tileSetId) = _metaData.SpriteIndices.Find(item => item.Item1 == cellIndex);

        if (string.IsNullOrEmpty(spriteID))
        {
            return;
        }

        // Going through the list again to find the first occurrence of this item
        (int firstIndex, string _, string _) = _metaData.SpriteIndices.Find(item => item.Item2 == spriteID);

        TileSetModel? tileSetModel = ProjectFiles.GetModel<TileSetModel>(tileSetId);

        if (tileSetModel == null)
        {
            return;
        }

        SpriteModel? sprite = tileSetModel.Sprites.Find((item) => item.ID == spriteID);

        if (string.IsNullOrEmpty(sprite?.ID))
        {
            return;
        }

        SelectedSpriteFromBank = sprite;
    }

    private void ShowRectOverSprite1D(int spriteTilesTotal, int firstIndex, int canvasWidthInCells)
    {
        SpriteRectVisibility = Visibility.Visible;

        bool useNextRect = false;

        int left = (firstIndex % canvasWidthInCells) * BankUtils.SizeOfCellInPixels;
        int width = spriteTilesTotal * BankUtils.SizeOfCellInPixels;
        int top = (firstIndex / BankUtils.MaxTextureCellsWidth) * BankUtils.SizeOfCellInPixels;

        int whatIsLeft = 0;

        int maxCellsInPixels = BankUtils.MaxTextureCellsWidth * BankUtils.SizeOfCellInPixels;

        if (left + width > maxCellsInPixels)
        {
            int sub = maxCellsInPixels - left;

            whatIsLeft = width - sub;
            width = sub;

            useNextRect = true;
        }

        SpriteRectLeft = left;
        SpriteRectWidth = width;
        SpriteRectHeight = BankUtils.SizeOfCellInPixels;
        SpriteRectTop = top;

        if (useNextRect)
        {
            useNextRect = false;

            SpriteRectVisibility2 = Visibility.Visible;

            width = whatIsLeft;

            if (width > maxCellsInPixels)
            {
                whatIsLeft = width - maxCellsInPixels;
                width = maxCellsInPixels;

                useNextRect = true;
            }

            SpriteRectLeft2 = 0;
            SpriteRectWidth2 = width;
            SpriteRectHeight2 = BankUtils.SizeOfCellInPixels;
            SpriteRectTop2 = top + BankUtils.SizeOfCellInPixels;

            if (useNextRect)
            {
                SpriteRectVisibility3 = Visibility.Visible;

                SpriteRectLeft3 = 0;
                SpriteRectWidth3 = whatIsLeft;
                SpriteRectHeight3 = BankUtils.SizeOfCellInPixels;
                SpriteRectTop3 = top + (BankUtils.SizeOfCellInPixels * 2);
            }
        }
    }

    private void OnObtainTransparentColor(SpriteModel spriteModel)
    {
        TileSetModel? tileSetModel = ProjectFiles.GetModel<TileSetModel>(spriteModel.TileSetID);

        if (tileSetModel == null)
        {
            return;
        }

        SpriteModel? sm = tileSetModel.Sprites.Find(x => x.ID == spriteModel.ID);

        if (sm == null)
        {
            return;
        }

        int width = 0;
        int height = 0;

        if (spriteModel.Shape == SpriteShape.Custom || spriteModel.Size == SpriteSize.Custom)
        {
            width = spriteModel.Width;
            height = spriteModel.Height;
        }
        else
        {
            SpriteUtils.ConvertToWidthHeight(spriteModel.Shape, spriteModel.Size, ref width, ref height);
        }

        (_, WriteableBitmap? sourceBitmapCached) = TileSetUtils.GetSourceBitmapFromCache(tileSetModel);

        if (sourceBitmapCached == null)
        {
            return;
        }

        WriteableBitmap sourceBitmap = sourceBitmapCached.CloneCurrentValue();

        WriteableBitmap cropped = sourceBitmap.Crop(sm.PosX, sm.PosY, width, height);

        SignalManager.Get<ReturnTransparentColorFromBankSignal>().Dispatch(cropped.GetPixel(0, 0));
    }

    private void OnGeneratePaletteFromBank(string name, IEnumerable<SpriteModel> bankSprites, Color transparentColor, BitsPerPixel bitPerPixel)
    {
        List<Color> colorArray = PaletteUtils.GeneratePaletteColorList(bankSprites, transparentColor, bitPerPixel);

        if (colorArray.Count > 1)
        {
            if (!string.IsNullOrEmpty(name))
            {
                SignalManager.Get<TryCreatePaletteElementSignal>().Dispatch(name, colorArray);
            }
        }
    }
}
