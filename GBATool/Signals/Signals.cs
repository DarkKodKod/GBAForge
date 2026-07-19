using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.Models;
using GBATool.Utils;
using GBATool.ViewModels;
using GBATool.VOs;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GBATool.Signals;

#region Generics
public class BrowseFolderSuccessSignal : Signal<Control, string> { }
public class BrowseFileSuccessSignal : Signal<string, bool> { }
public class SetUpWindowPropertiesSignal : Signal<WindowVO> { }
public class FinishedLoadingProjectSignal : Signal { }
public class ProjectItemLoadedSignal : Signal<string> { }
public class ProjectConfigurationSavedSignal : Signal { }
public class ShowLoadingDialogSignal : Signal { }
public class CloseDialogSignal : Signal { }
public class WriteBuildOutputSignal : Signal<string, OutputMessageType, string> { }
public class MouseWheelSignal : Signal<MouseWheelVO> { }
public class MouseImageSelectedSignal : Signal<Image, Point> { }
public class OptionOnionSkinSignal : Signal<bool> { }
public class OptionShowCollisionsSignal : Signal<bool, string[]> { }
public class ProjectBuildCompleteSignal : Signal { }
public class DragLeaveEventSignal : Signal<DragLeaveVO> { }
public class DropEventSignal : Signal<DragLeaveVO> { }
public class DragOverEventSignal : Signal<DragLeaveVO> { }
public class MouseDownEventSignal : Signal<MouseButtonVO> { }
public class MouseUpEventSignal : Signal<MouseButtonVO> { }
public class MouseMoveEventSignal : Signal<MouseEventVO> { }
public class MouseLeaveEventSignal : Signal<MouseEventVO> { }
public class PreviewMouseMoveEventSignal : Signal<MouseEventVO> { }
public class PreviewMouseDownEventSignal : Signal<MouseEventVO> { }
public class SelectionChangedEventSignal : Signal<SelectionChangedVO> { }
#endregion

#region MainWindowViewModel
public class SizeChangedSignal : Signal<SizeChangedEventArgs, bool> { }
public class LoadConfigSuccessSignal : Signal { }
public class ExitSuccessSignal : Signal { }
public class UpdateAdornersSignal : Signal<TreeViewItem, DragEventArgs> { }
public class MouseLeftButtonDownSignal : Signal<Point> { }
public class MouseLeftButtonUpSignal : Signal { }
public class ProjectItemSelectedSignal : Signal<ProjectItem> { }
public class ProjectItemUnselectedSignal : Signal<ProjectItem> { }
public class CloseProjectSuccessSignal : Signal { }
public class OpenProjectSuccessSignal : Signal<ProjectOpenVO> { }
public class UpdateRecentProjectsSignal : Signal<string[]> { }
public class DropElementSignal : Signal<ProjectItem, ProjectItem> { }
public class LoadProjectItemSignal : Signal<ProjectItem> { }
public class UpdateFolderSignal : Signal<ProjectItem> { }
public class CreateProjectSuccessSignal : Signal<string> { }
public class InitializeAdornersSignal : Signal<TreeViewItem, DragEventArgs> { }
public class DetachAdornersSignal : Signal { }
public class GotoProjectItemSignal : Signal<string> { }
#endregion

#region File system
public class RenameFileSignal : Signal<ProjectItem> { }
#endregion

#region Edit
public class PasteElementSignal : Signal<ProjectItem, ProjectItem> { }
public class MoveElementSignal : Signal<ProjectItem, ProjectItem> { }
public class DeleteElementSignal : Signal<ProjectItem> { }
public class FindAndCreateElementSignal : Signal<ProjectItem> { }
public class CreateNewElementSignal : Signal<ProjectItem> { }
#endregion

#region TileSet
public class UpdateTileSetImageSignal : Signal { }
public class SpriteSelectCursorSignal : Signal { }
public class SpriteSize16x16Signal : Signal { }
public class SpriteSize16x32Signal : Signal { }
public class SpriteSize16x8Signal : Signal { }
public class SpriteSize32x16Signal : Signal { }
public class SpriteSize32x32Signal : Signal { }
public class SpriteSize32x64Signal : Signal { }
public class SpriteSize32x8Signal : Signal { }
public class SpriteSize64x32Signal : Signal { }
public class SpriteSize64x64Signal : Signal { }
public class SpriteSize8x16Signal : Signal { }
public class SpriteSize8x32Signal : Signal { }
public class SpriteSize8x8Signal : Signal { }
public class SpriteSizeCustomSignal : Signal { }
public class ConfirmSpriteDeletionSignal : Signal<SpriteVO> { }
public class SelectSpriteSignal : Signal<SpriteVO> { }
#endregion

#region SpriteList
public class UpdateSpriteListSignal : Signal { }
public class CleanUpSpriteListSignal : Signal { }
public class AddSpriteSignal : Signal<SpriteVO> { }
public class DeletingSpriteSignal : Signal<SpriteVO> { }
#endregion

#region Character
public class ColorPaletteControlSelectedSignal : Signal<int[]> { }
public class CleanColorPaletteControlSelectedSignal : Signal { }
public class AnimationTabNewSignal : Signal { }
public class RenamedAnimationTabSignal : Signal<string> { }
public class AnimationTabDeletedSignal : Signal<ActionTabItem> { }
public class SwapAnimationTabSignal : Signal<int, int> { }
public class SwitchCharacterFrameViewSignal : Signal<string, string, int> { }
public class UpdateCharacterImageSignal : Signal { }
public class SwitchAnimationTabSignal : Signal<string> { }
public class NewAnimationFrameSignal : Signal<string, string, int, bool> { }
public class DeleteAnimationFrameSignal : Signal<string, int> { }
public class SelectFrameElementsSignal : Signal<SpriteControlVO[], CollisionControlVO[]> { }
public class ResetFrameSpritesSelectionAreaSignal : Signal<Point> { }
public class PreviousFrameCharacterAnimationSignal : Signal<string> { }
public class PauseCharacterAnimationSignal : Signal<string> { }
public class UpdateCollisionViewSignal : Signal { }
public class AddCollisionsOnCanvasSignal : Signal<List<SpriteCollisionVO>, string> { }
public class NextFrameCharacterAnimationSignal : Signal<string> { }
public class UpdateVerticalAxisSignal : Signal<int> { }
public class NewCollisionIntoSpriteSignal : Signal<string, string, SpriteCollisionVO> { }
public class CopyCollisionsSignal : Signal<string, string> { }
public class PasteCollisionsSignal : Signal<string, string> { }
public class CollisionColorSelectedSignal : Signal<string, string, string, Color> { }
public class DeleteCollisionSignal : Signal<string, string, string> { }
public class UpdateSpriteCollisionInfoSignal : Signal<SpriteCollisionVO> { }
public class UpdateSpriteBaseSignal : Signal<int> { }
public class UpdateOriginPositionSignal : Signal<int, int> { }
public class StopCharacterAnimationSignal : Signal<string> { }
public class PlayCharacterAnimationSignal : Signal<string> { }
public class AddOrUpdateSpriteIntoCharacterFrameSignal : Signal<CharacterSprite, string> { }
public class LoadWithSpriteControlsSignal : Signal<List<SpriteControlVO>, string, string> { }
public class FillWithPreviousFrameSpriteControlsSignal : Signal<List<SpriteControlVO>, string> { }
public class DeleteElementsFromCharacterFrameSignal : Signal<string[]> { }
public class SpriteFrameHideSelectionSignal : Signal { }
public class ElementFrameShowSelectionSignal : Signal<FrameElementDragObjectVO[]> { }
public class InformationToCorrectlyDisplayTheMetaSpriteCenteredSignal : Signal<double, double, double> { }
public class UpdateSpriteVisualPropertiesSignal : Signal<string, bool, bool> { }
#endregion

#region Banks
public class FileModelVOSelectionChangedSignal : Signal<FileModelVO> { }
public class AddNewTileSetLinkSignal : Signal<BankLinkVO> { }
public class CleanupTileSetLinksSignal : Signal { }
public class CleanupBankSpritesSignal : Signal { }
public class AdjustCanvasBankSizeSignal : Signal<BitsPerPixel> { }
public class ReloadBankViewImageSignal : Signal { }
public class SelectTileSetSignal : Signal<string> { }
public class BankImageUpdatedSignal : Signal { }
public class BankSpriteDeletedSignal : Signal<SpriteModel> { }
public class ObtainTransparentColorSignal : Signal<SpriteModel> { }
public class GeneratePaletteFromBankSignal : Signal<string, IEnumerable<SpriteModel>, Color, BitsPerPixel> { }
public class SetBankModelToBankViewerSignal : Signal<BankModel?> { }
public class CharacterFrameEditorViewLoadedSignal : Signal { }
public class UpdateBankViewerParentWithImageMetadataSignal : Signal<BankImageMetaData?> { }
public class ReloadBankImageSignal : Signal { }
public class MoveDownSelectedSpriteElementSignal : Signal<int> { }
public class RemoveSpriteSelectionFromBank : Signal { }
public class MoveUpSelectedSpriteElementSignal : Signal<int> { }
public class SetColorFromColorPickerSignal : Signal<Control, Color> { }
public class ReturnTransparentColorFromBankSignal : Signal<Color> { }
public class TryCreatePaletteElementSignal : Signal<string, List<Color>> { }
public class UseBitmapAsCursorSignal : Signal<Image> { }
#endregion

#region Palettes
public class ColorPaletteSelectedSignal : Signal<Color, int, int> { }
public class PaletteColorArrayChangeSignal : Signal<int[], int> { }
#endregion

#region Maps
public class ChangeMapPaletteSignal : Signal<int, int> { }
public class TryCaptureMouseSignal : Signal<string> { }
public class TryReleaseMouseSignal : Signal<string> { }
public class ResetSelectionAreaSignal : Signal<Point> { }
public class SelectTilesSignal : Signal<TileObject[]> { }
public class MapEraseToolSignal : Signal { }
public class MapSelectToolSignal : Signal { }
public class MapPaintToolSignal : Signal { }
public class MapBucketToolSignal : Signal { }
public class MapMoveToolSignal : Signal { }
public class UncheckMapBucketToolSignal : Signal { }
public class UncheckMapSelectToolSignal : Signal { }
public class UncheckMapEraseToolSignal : Signal { }
public class UncheckMapPaintToolSignal : Signal { }
public class UncheckMapMoveToolSignal : Signal { }
#endregion
