using ArchitectureLibrary.Signals;
using GBATool.Signals;
using GBATool.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GBATool.Views
{
    /// <summary>
    /// Interaction logic for Map.xaml
    /// </summary>
    public partial class Map : UserControl, ICleanable
    {
        public Map()
        {
            InitializeComponent();

            #region Signals
            SignalManager.Get<TryCaptureMouseSignal>().Listener += OnTryCaptureMouse;
            SignalManager.Get<TryReleaseMouseSignal>().Listener += OnTryReleaseMouse;
            SignalManager.Get<UseBitmapAsCursorSignal>().Listener += OnUseBitmapAsCursor;
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
            #endregion
        }

        private void OnUseBitmapAsCursor(Image image)
        {
            cursorImage.Source = image.Source;
        }

        private void OnTryCaptureMouse(string name)
        {
            if (name != mapCanvas.Name)
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
            if (name != mapCanvas.Name)
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
            cursorImage.Visibility = Visibility.Visible;
        }

        private void MapCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            cursorImage.Visibility = Visibility.Collapsed;
        }

        private void MapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.OriginalSource is not Canvas canvas)
            {
                return;
            }

            if (cursorImage.Source == null)
            {
                return;
            }

            Point positionInCanvas = e.GetPosition(canvas);

            Canvas.SetLeft(cursorImage, positionInCanvas.X);
            Canvas.SetTop(cursorImage, positionInCanvas.Y);
        }
    }
}
