using ArchitectureLibrary.Signals;
using GBATool.Enums;
using GBATool.Signals;
using GBATool.Utils;
using GBATool.VOs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GBATool.Views
{
    /// <summary>
    /// Interaction logic for Map.xaml
    /// </summary>
    public partial class Map : UserControl, ICleanable
    {
        private MapFunctionality _currentMapFunctionality = MapFunctionality.Select;
        private int _deltaCursorMoveX = 0;
        private int _deltaCursorMoveY = 0;

        public Map()
        {
            InitializeComponent();

            #region Signals
            SignalManager.Get<TryCaptureMouseSignal>().Listener += OnTryCaptureMouse;
            SignalManager.Get<TryReleaseMouseSignal>().Listener += OnTryReleaseMouse;
            SignalManager.Get<UseBitmapAsCursorSignal>().Listener += OnUseBitmapAsCursor;
            SignalManager.Get<CheckMapBucketToolSignal>().Listener += OnCheckMapBucketTool;
            SignalManager.Get<CheckMapSelectToolSignal>().Listener += OnCheckMapSelectTool;
            SignalManager.Get<CheckMapEraseToolSignal>().Listener += OnCheckMapEraseTool;
            SignalManager.Get<CheckMapPaintToolSignal>().Listener += OnCheckMapPaintTool;
            SignalManager.Get<CheckMapMoveToolSignal>().Listener += OnCheckMapMoveTool;
            SignalManager.Get<UseBitmapAsMoveCursorSignal>().Listener += OnUseBitmapAsMoveCursor;
            #endregion

            bankViewer.OnActivate();

            palette0.OnActivate();
            palette1.OnActivate();
            palette2.OnActivate();
            palette3.OnActivate();
            palette4.OnActivate();
            palette5.OnActivate();
            palette6.OnActivate();
            palette7.OnActivate();
            palette8.OnActivate();
            palette9.OnActivate();
            palette10.OnActivate();
            palette11.OnActivate();
            palette12.OnActivate();
            palette13.OnActivate();
            palette14.OnActivate();
            palette15.OnActivate();
        }

        public void CleanUp()
        {
            bankViewer.OnDeactivate();

            palette0.OnDeactivate();
            palette1.OnDeactivate();
            palette2.OnDeactivate();
            palette3.OnDeactivate();
            palette4.OnDeactivate();
            palette5.OnDeactivate();
            palette6.OnDeactivate();
            palette7.OnDeactivate();
            palette8.OnDeactivate();
            palette9.OnDeactivate();
            palette10.OnDeactivate();
            palette11.OnDeactivate();
            palette12.OnDeactivate();
            palette13.OnDeactivate();
            palette14.OnDeactivate();
            palette15.OnDeactivate();

            #region Signals
            SignalManager.Get<TryCaptureMouseSignal>().Listener -= OnTryCaptureMouse;
            SignalManager.Get<TryReleaseMouseSignal>().Listener -= OnTryReleaseMouse;
            SignalManager.Get<UseBitmapAsCursorSignal>().Listener -= OnUseBitmapAsCursor;
            SignalManager.Get<CheckMapBucketToolSignal>().Listener -= OnCheckMapBucketTool;
            SignalManager.Get<CheckMapSelectToolSignal>().Listener -= OnCheckMapSelectTool;
            SignalManager.Get<CheckMapEraseToolSignal>().Listener -= OnCheckMapEraseTool;
            SignalManager.Get<CheckMapPaintToolSignal>().Listener -= OnCheckMapPaintTool;
            SignalManager.Get<CheckMapMoveToolSignal>().Listener -= OnCheckMapMoveTool;
            SignalManager.Get<UseBitmapAsMoveCursorSignal>().Listener -= OnUseBitmapAsMoveCursor;
            #endregion
        }

        private static bool MouseWithinBounds(FrameworkElement control, System.Drawing.Point mousePosition)
        {
            Point controlXY = control.TransformToAncestor(Application.Current.MainWindow).Transform(new(0, 0));

            System.Drawing.Rectangle controlRect = new(
                (int)controlXY.X,
                (int)controlXY.Y,
                (int)control.ActualWidth,
                (int)control.ActualHeight);
            return controlRect.Contains(mousePosition);
        }

        private bool IsMouseInMapBounds()
        {
            Point mousePos = Mouse.GetPosition(Application.Current.MainWindow);

            System.Drawing.Point mousePosition = new((int)mousePos.X, (int)mousePos.Y);

            return MouseWithinBounds(mapCanvas, mousePosition);
        }

        private void OnCheckMapBucketTool()
        {
            if (IsMouseInMapBounds())
            {
                cursorImage.Visibility = Visibility.Visible;
                cursorMoveImage.Visibility = Visibility.Collapsed;
            }

            _currentMapFunctionality = MapFunctionality.BucketPaint;
        }

        private void OnCheckMapSelectTool()
        {
            if (IsMouseInMapBounds())
            {
                cursorImage.Visibility = Visibility.Collapsed;
                cursorMoveImage.Visibility = Visibility.Collapsed;
            }

            _currentMapFunctionality = MapFunctionality.Select;
        }

        private void OnCheckMapEraseTool()
        {
            if (IsMouseInMapBounds())
            {
                cursorImage.Visibility = Visibility.Collapsed;
                cursorMoveImage.Visibility = Visibility.Collapsed;
            }

            _currentMapFunctionality = MapFunctionality.Erase;
        }

        private void OnCheckMapPaintTool()
        {
            if (IsMouseInMapBounds())
            {
                cursorImage.Visibility = Visibility.Visible;
                cursorMoveImage.Visibility = Visibility.Collapsed;
            }

            _currentMapFunctionality = MapFunctionality.Paint;
        }

        private void OnCheckMapMoveTool()
        {
            if (IsMouseInMapBounds())
            {
                cursorImage.Visibility = Visibility.Collapsed;
                cursorMoveImage.Visibility = Visibility.Visible;
            }

            _currentMapFunctionality = MapFunctionality.Move;
        }

        private void OnUseBitmapAsCursor(MapPaintCursorVO vo)
        {
            cursorImage.Source = vo.Image;
        }

        private void OnUseBitmapAsMoveCursor(ImageSource? image, int deltaX, int deltaY)
        {
            cursorMoveImage.Source = image;
            _deltaCursorMoveX = deltaX;
            _deltaCursorMoveY = deltaY;
        }

        private void OnTryCaptureMouse(string name)
        {
            if (name != mapCanvas.Name &&
                name != imgMap.Name)
            {
                return;
            }

            if (!mapCanvas.IsMouseCaptured)
            {
                mapCanvas.CaptureMouse();
            }
        }

        private void OnTryReleaseMouse(string name)
        {
            if (name != mapCanvas.Name &&
                name != imgMap.Name)
            {
                return;
            }

            if (mapCanvas.IsMouseCaptured)
            {
                mapCanvas.ReleaseMouseCapture();
            }
        }

        private void MapCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            cursorImage.Visibility = _currentMapFunctionality switch
            {
                MapFunctionality.Select or MapFunctionality.Erase or MapFunctionality.Move => Visibility.Collapsed,
                _ => Visibility.Visible
            };

            cursorMoveImage.Visibility = _currentMapFunctionality is MapFunctionality.Move ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MapCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            cursorImage.Visibility = Visibility.Collapsed;
            cursorMoveImage.Visibility = Visibility.Collapsed;
        }

        private void MapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement parentControl && (parentControl is Canvas or Image))
            {
                Point positionInCanvas = e.GetPosition(parentControl);

                // remove the decimals
                int posX = (int)positionInCanvas.X;
                int posY = (int)positionInCanvas.Y;

                if (cursorImage.Source != null)
                {
                    Canvas.SetLeft(cursorImage, posX);
                    Canvas.SetTop(cursorImage, posY);
                }

                if (cursorMoveImage.Source != null)
                {
                    Canvas.SetLeft(cursorMoveImage, posX + _deltaCursorMoveX);
                    Canvas.SetTop(cursorMoveImage, posY + _deltaCursorMoveY);
                }
            }
        }
    }
}
