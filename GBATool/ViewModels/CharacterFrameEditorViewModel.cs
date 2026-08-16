using ArchitectureLibrary.Model;
using ArchitectureLibrary.Signals;
using ArchitectureLibrary.ViewModel;
using GBATool.Commands.Banks;
using GBATool.Commands.Character;
using GBATool.Enums;
using GBATool.FileSystem;
using GBATool.Models;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.VOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace GBATool.ViewModels;

public class CharacterFrameEditorViewModel : ViewModel
{
    private FileModelVO[]? _banks;
    private int _selectedBank = -1;
    private string _tabId = string.Empty;
    private string _animationID = string.Empty;
    private string _frameID = string.Empty;
    private int _frameIndex;
    private FileHandler? _fileHandler;
    private BankModel? _bankModel = null;
    private string[] _selectedFrameSprites = [];
    private string[] _selectedFrameCollisions = [];
    private BankImageMetaData? _bankImageMetaData = null;
    private bool _enableOnionSkin;
    private bool _showCollisions;
    private bool _dontSave;
    private bool _isFlippedHorizontal;
    private bool _isFlippedVertical;
    private bool _isEnableMosaic;
    private bool _enableSpriteProperties;
    private double _onionSkinOpacity = 0.25;
    private ObjectMode _objectMode = ObjectMode.Normal;
    private GraphicMode _graphicMode = GraphicMode.Normal;
    private SpriteCollisionVO? _characterCollision = null;
    private List<SpriteCollisionVO> _characterCollisions = [];

    /// <summary>
    /// This variable is set from the tab that is active representing the current animation
    /// </summary>
    private string ActiveAnimationID = string.Empty;

    private static readonly List<SpriteCollisionVO> _copiedSpriteCollisions = [];

    #region Commands
    public SwitchCharacterFrameViewCommand SwitchCharacterFrameViewCommand { get; } = new();
    public FileModelVOSelectionChangedCommand FileModelVOSelectionChangedCommand { get; } = new();
    public AddNewCollisionIntoSpriteFrameCommand AddNewCollisionIntoSpriteFrameCommand { get; } = new();
    public CopyCollisionsCommand CopyCollisionsCommand { get; } = new();
    public PasteCollisionsCommand PasteCollisionsCommand { get; } = new();
    public DeleteCollisionCommand DeleteCollisionCommand { get; } = new();
    public ChangeCollisionColorCommand ChangeCollisionColorCommand { get; } = new();
    #endregion

    #region get/set
    public SpriteCollisionVO? SelectedCollision
    {
        get => _characterCollision;
        set
        {
            _characterCollision = value;
            OnPropertyChanged(nameof(SelectedCollision));
        }
    }

    public List<SpriteCollisionVO> CharacterCollisions
    {
        get => _characterCollisions;
        set
        {
            _characterCollisions = value;

            OnPropertyChanged(nameof(CharacterCollisions));
        }
    }

    public bool EnableSpriteProperties
    {
        get => _enableSpriteProperties;
        set
        {
            _enableSpriteProperties = value;

            OnPropertyChanged(nameof(EnableSpriteProperties));
        }
    }

    public ObjectMode ObjectMode
    {
        get => _objectMode;
        set
        {
            if (_objectMode != value)
            {
                _objectMode = value;

                SaveSpriteProperties();
            }

            OnPropertyChanged(nameof(ObjectMode));
        }
    }

    public GraphicMode GraphicMode
    {
        get => _graphicMode;
        set
        {
            if (_graphicMode != value)
            {
                _graphicMode = value;

                SaveSpriteProperties();
            }

            OnPropertyChanged(nameof(GraphicMode));
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

                SaveSpriteProperties();
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

                SaveSpriteProperties();
            }

            OnPropertyChanged(nameof(IsFlippedVertical));
        }
    }

    public bool IsEnableMosaic
    {
        get => _isEnableMosaic;
        set
        {
            if (_isEnableMosaic != value)
            {
                _isEnableMosaic = value;

                SaveSpriteProperties();
            }

            OnPropertyChanged(nameof(IsEnableMosaic));
        }
    }

    public string[] SelectedFrameCollisions
    {
        get => _selectedFrameCollisions;
        set
        {
            _selectedFrameCollisions = value;

            OnPropertyChanged(nameof(SelectedFrameCollisions));
        }
    }

    public string[] SelectedFrameSprites
    {
        get => _selectedFrameSprites;
        set
        {
            _selectedFrameSprites = value;

            OnPropertyChanged(nameof(SelectedFrameSprites));
        }
    }

    public double OnionSkinOpacity
    {
        get => _onionSkinOpacity;
        set
        {
            double newValue = Math.Round(value, 2);

            if (newValue != _onionSkinOpacity)
            {
                _onionSkinOpacity = newValue;

                ModelManager.Get<GBAToolConfigurationModel>().OnionSkinOpacity = newValue;
                ModelManager.Get<GBAToolConfigurationModel>().Save();

                if (EnableOnionSkin)
                {
                    // this will update the opacity
                    SignalManager.Get<OptionOnionSkinSignal>().Dispatch(EnableOnionSkin);
                }
            }

            OnPropertyChanged(nameof(OnionSkinOpacity));
        }
    }

    public bool ShowCollisions
    {
        get => _showCollisions;
        set
        {
            if (value != _showCollisions)
            {
                _showCollisions = value;

                ModelManager.Get<GBAToolConfigurationModel>().ShowCollisions = _showCollisions;
                ModelManager.Get<GBAToolConfigurationModel>().Save();

                SignalManager.Get<OptionShowCollisionsSignal>().Dispatch(_showCollisions, SelectedFrameCollisions);

                if (!_showCollisions)
                {
                    SelectedFrameCollisions = [];
                }
            }

            OnPropertyChanged(nameof(ShowCollisions));
        }
    }

    public bool EnableOnionSkin
    {
        get => _enableOnionSkin;
        set
        {
            if (value != _enableOnionSkin)
            {
                _enableOnionSkin = value;

                ModelManager.Get<GBAToolConfigurationModel>().EnableOnionSkin = value;
                ModelManager.Get<GBAToolConfigurationModel>().Save();

                SignalManager.Get<OptionOnionSkinSignal>().Dispatch(value);
            }

            OnPropertyChanged(nameof(EnableOnionSkin));
        }
    }

    public FileHandler? FileHandler
    {
        get => _fileHandler;
        set
        {
            _fileHandler = value;

            OnPropertyChanged(nameof(FileHandler));
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

    public string TabID
    {
        get => _tabId;
        set
        {
            _tabId = value;

            OnPropertyChanged(nameof(TabID));

            var model = CharacterModel;

            if (model == null)
                return;

            foreach (var item in model.Animations)
            {
                CharacterAnimation animation = item.Value;

                if (animation.ID == TabID)
                {
                    AnimationID = animation.ID;
                    break;
                }
            }
        }
    }

    public CharacterModel? CharacterModel { get; set; }

    public string AnimationID
    {
        get => _animationID;
        set
        {
            _animationID = value;

            OnPropertyChanged(nameof(AnimationID));
        }
    }

    public string FrameID
    {
        get => _frameID;
        set
        {
            _frameID = value;

            OnPropertyChanged(nameof(FrameID));
        }
    }

    public string PreviousFrameID { get; set; } = string.Empty;
    public int PreviousFrameIndex { get; set; }

    public BankImageMetaData? BankImageMetaData
    {
        get => _bankImageMetaData;
        set
        {
            _bankImageMetaData = value;

            OnPropertyChanged(nameof(BankImageMetaData));
        }
    }

    public int FrameIndex
    {
        get => _frameIndex;
        set
        {
            _frameIndex = value;

            OnPropertyChanged(nameof(FrameIndex));
        }
    }

    public BankModel? BankModel
    {
        get => _bankModel;
        set
        {
            _bankModel = value;

            OnPropertyChanged(nameof(BankModel));
        }
    }
    #endregion

    public CharacterFrameEditorViewModel()
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

        if (Util.InDesignMode())
        {
            CharacterCollisions.Add(new()
            {
                ID = "dummy",
                Width = 0,
                Height = 0,
                PosX = 0,
                PosY = 0,
                Color = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                Mask = CollisionMask.Custom,
                CustomMask = 0
            });
        }
    }

    public override void OnActivate()
    {
        base.OnActivate();

        #region Signals
        SignalManager.Get<FileModelVOSelectionChangedSignal>().Listener += OnFileModelVOSelectionChanged;
        SignalManager.Get<SelectFrameElementsSignal>().Listener += OnSelectFrameSprites;
        SignalManager.Get<AddOrUpdateSpriteIntoCharacterFrameSignal>().Listener += OnAddOrUpdateSpriteIntoCharacterFrame;
        SignalManager.Get<DeleteElementsFromCharacterFrameSignal>().Listener += OnDeleteSpriteFromCharacterFrame;
        SignalManager.Get<CharacterFrameEditorViewLoadedSignal>().Listener += OnCharacterFrameEditorViewLoaded;
        SignalManager.Get<DeleteCollisionSignal>().Listener += OnDeleteCollision;
        SignalManager.Get<NewCollisionIntoSpriteSignal>().Listener += OnNewCollisionIntoSprite;
        SignalManager.Get<CopyCollisionsSignal>().Listener += OnCopyCollisions;
        SignalManager.Get<PasteCollisionsSignal>().Listener += OnPasteCollisions;
        SignalManager.Get<CollisionColorSelectedSignal>().Listener += OnCollisionColorSelected;
        SignalManager.Get<UpdateSpriteCollisionInfoSignal>().Listener += OnUpdateSpriteCollisionInfo;
        SignalManager.Get<SwitchAnimationTabSignal>().Listener += OnSwitchAnimationTab;
        #endregion

        EnableOnionSkin = ModelManager.Get<GBAToolConfigurationModel>().EnableOnionSkin;
        OnionSkinOpacity = ModelManager.Get<GBAToolConfigurationModel>().OnionSkinOpacity;
        ShowCollisions = ModelManager.Get<GBAToolConfigurationModel>().ShowCollisions;

        SelectedFrameSprites = [];
        SelectedFrameCollisions = [];
        EnableSpriteProperties = false;
    }

    private void LoadCollisions()
    {
        if (ActiveAnimationID != AnimationID)
        {
            return;
        }

        var model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (!model.Animations.TryGetValue(AnimationID, out CharacterAnimation? animation))
        {
            return;
        }

        if (!animation.Frames.TryGetValue(FrameID, out FrameModel? frame))
        {
            return;
        }

        _dontSave = true;

        CharacterCollisions.Clear();

        foreach (KeyValuePair<string, CharacterCollision> item in frame.CollisionInfo)
        {
            SpriteCollisionVO collisionVO = new()
            {
                ID = item.Key,
                Width = item.Value.Width,
                Height = item.Value.Height,
                PosX = item.Value.PosX,
                PosY = item.Value.PosY,
                Color = new SolidColorBrush(item.Value.Color),
                Mask = (CollisionMask)item.Value.Mask,
                CustomMask = item.Value.CustomMask,
                AnimationID = AnimationID,
                FrameID = FrameID
            };

            CharacterCollisions.Add(collisionVO);
        }

        SignalManager.Get<UpdateCollisionViewSignal>().Dispatch();

        if (CharacterCollisions.Count > 0)
        {
            SignalManager.Get<AddCollisionsOnCanvasSignal>().Dispatch(CharacterCollisions, FrameID);
        }

        _dontSave = false;
    }

    private void LoadFrameSprites()
    {
        var model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (!model.Animations.TryGetValue(AnimationID, out CharacterAnimation? animation))
        {
            return;
        }

        _dontSave = true;

        SignalManager.Get<UpdateOriginPositionSignal>().Dispatch((int)model.RelativeOrigin.X, (int)model.RelativeOrigin.Y);
        SignalManager.Get<UpdateVerticalAxisSignal>().Dispatch(model.VerticalAxis);
        SignalManager.Get<UpdateSpriteBaseSignal>().Dispatch(model.SpriteBase);

        (_, List<SpriteControlVO>? previousSprites, _) = LoadSpritesFromFrame(animation, PreviousFrameID);

        if (previousSprites?.Count > 0)
        {
            SignalManager.Get<FillWithPreviousFrameSpriteControlsSignal>().Dispatch(previousSprites, AnimationID);
        }

        (BankImageMetaData? metaData, List<SpriteControlVO>? sprites, string bankID) = LoadSpritesFromFrame(animation, FrameID);

        if (metaData != null)
        {
            BankImageMetaData = metaData;

            if (sprites?.Count > 0)
            {
                SignalManager.Get<LoadWithSpriteControlsSignal>().Dispatch(sprites, FrameID, bankID);

                SelectBank(bankID);
            }
        }

        _dontSave = false;
    }

    private static (BankImageMetaData? meta, List<SpriteControlVO>? sprites, string bankID) LoadSpritesFromFrame(CharacterAnimation animation, string frameID)
    {
        if (!animation.Frames.TryGetValue(frameID, out FrameModel? frame))
        {
            return (null, null, string.Empty);
        }

        BankModel? bankModel = ProjectFiles.GetModel<BankModel>(frame.BankID);
        if (bankModel == null)
        {
            return (null, null, string.Empty);
        }

        int scaledHeight = BankUtils.MaxTextureCellsWidth * BankUtils.SizeOfCellInPixels;
        if (bankModel.BitsPerPixel == BitsPerPixel.f8bpp)
        {
            scaledHeight = (BankUtils.MaxTextureCellsWidth / 2) * BankUtils.SizeOfCellInPixels;
        }

        if (!BankModel.MetaDataCache.TryGetValue(bankModel.GUID, out BankImageMetaData? metaData))
        {
            metaData = BankUtils.CreateImage(
                bankModel,
                true,
                BankUtils.MaxTextureCellsWidth * BankUtils.SizeOfCellInPixels,
                scaledHeight);

            _ = BankModel.MetaDataCache.TryAdd(bankModel.GUID, metaData);
        }

        List<SpriteControlVO> sprites = [];

        foreach (KeyValuePair<string, CharacterSprite> item in frame.Tiles)
        {
            if (!metaData.Sprites.TryGetValue(item.Value.SpriteID, out SpriteInfo? spriteInfo))
            {
                continue;
            }

            if (spriteInfo == null || spriteInfo.BitmapSource == null)
            {
                continue;
            }

            SpriteControlVO sprite = new()
            {
                ID = item.Value.ID,
                Image = Util.GetImageFromWriteableBitmap(spriteInfo.BitmapSource),
                SpriteID = item.Value.SpriteID,
                TileSetID = item.Value.TileSetID,
                BankID = bankModel.GUID,
                Width = item.Value.Width,
                Height = item.Value.Height,
                OffsetX = spriteInfo.OffsetX,
                OffsetY = spriteInfo.OffsetY,
                PositionX = (int)item.Value.Position.X,
                PositionY = (int)item.Value.Position.Y,
                FlipHorizontal = item.Value.FlipHorizontal,
                FlipVertical = item.Value.FlipVertical
            };

            sprites.Add(sprite);
        }

        return (metaData, sprites, frame.BankID);
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

    private void OnSelectFrameSprites(SpriteControlVO[] sprites, CollisionControlVO[] collisions)
    {
        List<string> spritesIDs = [];
        List<string> collisionsIDs = [];

        for (int i = 0; i < sprites.Length; i++)
        {
            spritesIDs.Add(sprites[i].ID);
        }

        for (int i = 0; i < collisions.Length; i++)
        {
            collisionsIDs.Add(collisions[i].ID);
        }

        EnableSpriteProperties = sprites.Length == 1;

        if (EnableSpriteProperties)
        {
            UpdateSpriteProperties(sprites[0].ID);
        }

        SelectedFrameSprites = [.. spritesIDs];
        SelectedFrameCollisions = [.. collisionsIDs];
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        #region Signals
        SignalManager.Get<FileModelVOSelectionChangedSignal>().Listener -= OnFileModelVOSelectionChanged;
        SignalManager.Get<SelectFrameElementsSignal>().Listener -= OnSelectFrameSprites;
        SignalManager.Get<AddOrUpdateSpriteIntoCharacterFrameSignal>().Listener -= OnAddOrUpdateSpriteIntoCharacterFrame;
        SignalManager.Get<DeleteElementsFromCharacterFrameSignal>().Listener -= OnDeleteSpriteFromCharacterFrame;
        SignalManager.Get<CharacterFrameEditorViewLoadedSignal>().Listener -= OnCharacterFrameEditorViewLoaded;
        SignalManager.Get<DeleteCollisionSignal>().Listener -= OnDeleteCollision;
        SignalManager.Get<CopyCollisionsSignal>().Listener -= OnCopyCollisions;
        SignalManager.Get<PasteCollisionsSignal>().Listener -= OnPasteCollisions;
        SignalManager.Get<NewCollisionIntoSpriteSignal>().Listener -= OnNewCollisionIntoSprite;
        SignalManager.Get<CollisionColorSelectedSignal>().Listener -= OnCollisionColorSelected;
        SignalManager.Get<UpdateSpriteCollisionInfoSignal>().Listener -= OnUpdateSpriteCollisionInfo;
        SignalManager.Get<SwitchAnimationTabSignal>().Listener -= OnSwitchAnimationTab;
        #endregion
    }

    private void OnSwitchAnimationTab(string activeTab)
    {
        ActiveAnimationID = activeTab;
    }

    private void OnCharacterFrameEditorViewLoaded()
    {
        LoadFrameSprites();
        LoadCollisions();
    }

    private void OnCollisionColorSelected(string animationID, string frameID, string collisionID, Color newColor)
    {
        if (animationID != AnimationID || frameID != FrameID)
        {
            return;
        }

        foreach (SpriteCollisionVO item in CharacterCollisions)
        {
            if (item.ID == collisionID)
            {
                item.Color = new SolidColorBrush(Color.FromArgb(SpriteCollisionVO.Transparency, newColor.R, newColor.G, newColor.B));
                break;
            }
        }

        SignalManager.Get<UpdateCollisionViewSignal>().Dispatch();
    }

    private void OnUpdateSpriteCollisionInfo(SpriteCollisionVO collision)
    {
        if (collision.AnimationID != AnimationID || collision.FrameID != FrameID)
        {
            return;
        }

        if (_dontSave)
        {
            return;
        }

        var model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (!model.Animations.TryGetValue(collision.AnimationID, out CharacterAnimation? animation))
        {
            return;
        }

        if (!animation.Frames.TryGetValue(collision.FrameID, out FrameModel? frame))
        {
            return;
        }

        if (!frame.CollisionInfo.TryGetValue(collision.ID, out CharacterCollision? value))
        {
            return;
        }

        bool needToSave = false;

        if (value.Width != collision.Width)
        {
            value.Width = collision.Width;
            needToSave = true;
        }

        if (value.Height != collision.Height)
        {
            value.Height = collision.Height;
            needToSave = true;
        }

        if (value.PosX != collision.PosX)
        {
            value.PosX = collision.PosX;
            needToSave = true;
        }

        if (value.PosY != collision.PosY)
        {
            value.PosY = collision.PosY;
            needToSave = true;
        }

        if (value.Color != collision.Color.Color)
        {
            value.Color = collision.Color.Color;
            needToSave = true;
        }

        if (value.Mask != (int)collision.Mask)
        {
            value.Mask = (int)collision.Mask;
            needToSave = true;
        }

        if (value.CustomMask != collision.CustomMask)
        {
            value.CustomMask = collision.CustomMask;
            needToSave = true;
        }

        if (!needToSave)
        {
            return;
        }

        FileHandler?.Save();

        if (collision.ActAsVO)
        {
            foreach (SpriteCollisionVO item in CharacterCollisions)
            {
                if (item.ID == collision.ID)
                {
                    item.ActAsVO = true; // once here we dont need to send the message of something has changed again
                    item.Width = collision.Width;
                    item.Height = collision.Height;
                    item.PosX = collision.PosX;
                    item.PosY = collision.PosY;
                    item.ActAsVO = false; // go back to normal state
                    break;
                }
            }
        }
    }

    private void OnNewCollisionIntoSprite(string animationID, string frameID, SpriteCollisionVO collisionVO)
    {
        if (animationID != AnimationID || frameID != FrameID)
        {
            return;
        }

        CharacterCollisions.Add(collisionVO);

        SignalManager.Get<UpdateCollisionViewSignal>().Dispatch();

        if (_dontSave)
            return;

        var model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (!model.Animations.TryGetValue(animationID, out CharacterAnimation? animation))
        {
            return;
        }

        if (!animation.Frames.TryGetValue(frameID, out FrameModel? frame))
        {
            return;
        }

        frame.CollisionInfo.Add(collisionVO.ID,
            new()
            {
                ID = collisionVO.ID,
                Width = collisionVO.Width,
                Height = collisionVO.Height,
                PosX = collisionVO.PosX,
                PosY = collisionVO.PosY,
                Color = collisionVO.Color.Color,
                Mask = (int)collisionVO.Mask,
                CustomMask = collisionVO.CustomMask
            });

        FileHandler?.Save();
    }

    private void OnCopyCollisions(string animationID, string frameID)
    {
        if (animationID != AnimationID || frameID != FrameID)
        {
            return;
        }

        var model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (!model.Animations.TryGetValue(animationID, out CharacterAnimation? animation))
        {
            return;
        }

        if (!animation.Frames.TryGetValue(frameID, out FrameModel? _))
        {
            return;
        }

        _copiedSpriteCollisions.Clear();

        foreach (SpriteCollisionVO col in CharacterCollisions)
        {
            SpriteCollisionVO collisionVO = new()
            {
                ActAsVO = true,
                Width = col.Width,
                Height = col.Height,
                PosX = col.PosX,
                PosY = col.PosY,
                Color = new SolidColorBrush(col.Color.Color),
                Mask = col.Mask,
                CustomMask = col.CustomMask
            };

            _copiedSpriteCollisions.Add(collisionVO);
        }
    }

    private void OnPasteCollisions(string animationID, string frameID)
    {
        if (animationID != AnimationID || frameID != FrameID)
        {
            return;
        }

        var model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (!model.Animations.TryGetValue(animationID, out CharacterAnimation? animation))
        {
            return;
        }

        if (!animation.Frames.TryGetValue(frameID, out FrameModel? _))
        {
            return;
        }

        List<SpriteCollisionVO> newCollisionsVOs = [];

        foreach (SpriteCollisionVO col in _copiedSpriteCollisions)
        {
            SpriteCollisionVO collisionVO = new()
            {
                ActAsVO = true,
                ID = Guid.NewGuid().ToString(),
                Width = col.Width,
                Height = col.Height,
                PosX = col.PosX,
                PosY = col.PosY,
                Color = new SolidColorBrush(col.Color.Color),
                Mask = col.Mask,
                CustomMask = col.CustomMask,
                AnimationID = AnimationID,
                FrameID = FrameID
            };

            newCollisionsVOs.Add(collisionVO);

            OnNewCollisionIntoSprite(animationID, frameID, collisionVO);
        }

        if (newCollisionsVOs.Count > 0)
        {
            SignalManager.Get<AddCollisionsOnCanvasSignal>().Dispatch(newCollisionsVOs, FrameID);
        }
    }

    private void OnDeleteCollision(string animationID, string frameID, string collisionID)
    {
        if (animationID != AnimationID || frameID != FrameID)
        {
            return;
        }

        CharacterCollisions = [.. CharacterCollisions.Where(c => c.ID != collisionID)];

        SignalManager.Get<UpdateCollisionViewSignal>().Dispatch();

        if (_dontSave)
            return;

        var model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (!model.Animations.TryGetValue(animationID, out CharacterAnimation? animation))
        {
            return;
        }

        if (!animation.Frames.TryGetValue(frameID, out FrameModel? frame))
        {
            return;
        }

        if (!frame.CollisionInfo.ContainsKey(collisionID))
        {
            return;
        }

        frame.CollisionInfo.Remove(collisionID);

        FileHandler?.Save();
    }

    private void OnAddOrUpdateSpriteIntoCharacterFrame(CharacterSprite sprite, string bankID)
    {
        var model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (!model.Animations.TryGetValue(AnimationID, out CharacterAnimation? animation))
        {
            return;
        }

        if (!animation.Frames.TryGetValue(FrameID, out FrameModel? frame))
        {
            return;
        }

        frame.BankID = bankID;

        if (frame.Tiles.TryGetValue(sprite.ID, out _))
        {
            frame.Tiles[sprite.ID] = sprite;
        }
        else
        {
            frame.Tiles.Add(sprite.ID, sprite);
        }

        CharacterUtils.InvalidateFrameImageFromCache(frame.ID);

        if (_dontSave)
            return;

        FileHandler?.Save();
    }

    private void UpdateSpriteProperties(string spriteID)
    {
        var model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (!model.Animations.TryGetValue(AnimationID, out CharacterAnimation? animation))
        {
            return;
        }

        if (!animation.Frames.TryGetValue(FrameID, out FrameModel? frame))
        {
            return;
        }

        if (!frame.Tiles.TryGetValue(spriteID, out CharacterSprite? sprite))
        {
            return;
        }

        if (sprite == null)
        {
            return;
        }

        _dontSave = true;

        IsFlippedHorizontal = sprite.FlipHorizontal;
        IsFlippedVertical = sprite.FlipVertical;
        GraphicMode = sprite.GraphicMode;
        ObjectMode = sprite.ObjectMode;
        IsEnableMosaic = sprite.EnableMosaic;

        _dontSave = false;
    }

    private void SaveSpriteProperties()
    {
        if (_dontSave)
            return;

        var model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (!model.Animations.TryGetValue(AnimationID, out CharacterAnimation? animation))
        {
            return;
        }

        if (!animation.Frames.TryGetValue(FrameID, out FrameModel? frame))
        {
            return;
        }

        if (SelectedFrameSprites.Length > 1)
        {
            return;
        }

        if (!frame.Tiles.TryGetValue(SelectedFrameSprites[0], out CharacterSprite? sprite))
        {
            return;
        }

        if (sprite == null)
        {
            return;
        }

        CharacterUtils.InvalidateFrameImageFromCache(frame.ID);

        sprite.FlipHorizontal = IsFlippedHorizontal;
        sprite.FlipVertical = IsFlippedVertical;
        sprite.GraphicMode = GraphicMode;
        sprite.ObjectMode = ObjectMode;
        sprite.EnableMosaic = IsEnableMosaic;

        SignalManager.Get<UpdateSpriteVisualPropertiesSignal>().Dispatch(sprite.SpriteID, sprite.FlipHorizontal, sprite.FlipVertical);

        FileHandler?.Save();
    }

    private void OnDeleteSpriteFromCharacterFrame(string[] elementIDs)
    {
        var model = CharacterModel;

        if (model == null)
        {
            return;
        }

        if (!model.Animations.TryGetValue(AnimationID, out CharacterAnimation? animation))
        {
            return;
        }

        if (!animation.Frames.TryGetValue(FrameID, out FrameModel? frame))
        {
            return;
        }

        bool anyDeletion = false;

        bool areThereSprites = SelectedFrameSprites.Intersect(elementIDs).Any();

        for (int i = 0; i < elementIDs.Length; i++)
        {
            if (frame.Tiles.Remove(elementIDs[i]))
            {
                anyDeletion = true;
            }
        }

        string[] collisionsToDelete = [.. SelectedFrameCollisions.Intersect(elementIDs)];

        for (int i = 0; i < collisionsToDelete.Length; i++)
        {
            SignalManager.Get<DeleteCollisionSignal>().Dispatch(AnimationID, FrameID, collisionsToDelete[i]);
        }

        SelectedFrameCollisions = [];
        SelectedFrameSprites = [];

        if (areThereSprites)
        {
            CharacterUtils.InvalidateFrameImageFromCache(frame.ID);
        }

        if (anyDeletion)
        {
            FileHandler?.Save();
        }
    }

    private void OnFileModelVOSelectionChanged(FileModelVO fileModel)
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

        if (model == null)
        {
            return;
        }

        BankModel = model;

        SignalManager.Get<SetBankModelToBankViewerSignal>().Dispatch(model);
        SignalManager.Get<RemoveSpriteSelectionFromBank>().Dispatch();
    }
}
