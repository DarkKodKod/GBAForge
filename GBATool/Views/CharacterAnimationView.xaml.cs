using ArchitectureLibrary.Signals;
using ArchitectureLibrary.Utils;
using GBATool.Models;
using GBATool.Signals;
using GBATool.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GBATool.Views;

/// <summary>
/// Interaction logic for CharacterAnimationView.xaml
/// </summary>
public partial class CharacterAnimationView : UserControl
{
    private bool _imageReady;
    private double _distanceLeft;
    private double _distanceTop;
    private double _centerX;
    private double _centerY;

    public List<CharacterFrameView> FrameViewList { get; set; } = [];

    public CharacterAnimationView()
    {
        InitializeComponent();
    }

    public void OnActivate()
    {
        _imageReady = false;

        SignalManager.Get<NewAnimationFrameSignal>().Listener += OnNewAnimationFrame;
        SignalManager.Get<DeleteAnimationFrameSignal>().Listener += OnDeleteAnimationFrame;
        SignalManager.Get<MoveFrameBackwardSignal>().Listener += OnMoveFrameBackward;
        SignalManager.Get<MoveFrameForwardSignal>().Listener += OnMoveFrameForward;
        SignalManager.Get<InformationToCorrectlyDisplayTheMetaSpriteCenteredSignal>().Listener += OnInformationToCorrectlyDisplayTheMetaSpriteCentered;
    }

    public void OnDeactivate()
    {
        SignalManager.Get<NewAnimationFrameSignal>().Listener -= OnNewAnimationFrame;
        SignalManager.Get<DeleteAnimationFrameSignal>().Listener -= OnDeleteAnimationFrame;
        SignalManager.Get<MoveFrameBackwardSignal>().Listener -= OnMoveFrameBackward;
        SignalManager.Get<MoveFrameForwardSignal>().Listener -= OnMoveFrameForward;
        SignalManager.Get<InformationToCorrectlyDisplayTheMetaSpriteCenteredSignal>().Listener -= OnInformationToCorrectlyDisplayTheMetaSpriteCentered;

        FrameViewList.Clear();
        spFrames.Children.RemoveRange(0, spFrames.Children.Count - 1);
    }

    private void OnMoveFrameBackward(string animationID, int frameIndex)
    {
        if (DataContext is not CharacterAnimationViewModel viewModel)
        {
            return;
        }

        if (viewModel.TabID != animationID)
        {
            return;
        }

        if (frameIndex > 0 && frameIndex < spFrames.Children.Count)
        {
            UIElement element = spFrames.Children[frameIndex];

            spFrames.Children.RemoveAt(frameIndex);

            spFrames.Children.Insert(frameIndex - 1, element);
        }

        if (frameIndex > 0 && frameIndex < FrameViewList.Count)
        {
            (FrameViewList[frameIndex], FrameViewList[frameIndex - 1]) = (FrameViewList[frameIndex - 1], FrameViewList[frameIndex]);
        }

        int index = 0;

        // Adjust the index for all the remaining chidren
        foreach (object item in spFrames.Children)
        {
            if (item is CharacterFrameView view)
            {
                view.FrameIndex = index++;
            }
        }
    }

    private void OnMoveFrameForward(string animationID, int frameIndex)
    {
        if (DataContext is not CharacterAnimationViewModel viewModel)
        {
            return;
        }

        if (viewModel.TabID != animationID)
        {
            return;
        }

        if (frameIndex >= 0 && frameIndex < spFrames.Children.Count - 1)
        {
            UIElement element = spFrames.Children[frameIndex];

            spFrames.Children.RemoveAt(frameIndex);

            spFrames.Children.Insert(frameIndex + 1, element);
        }

        if (frameIndex >= 0 && frameIndex < FrameViewList.Count - 1)
        {
            (FrameViewList[frameIndex], FrameViewList[frameIndex + 1]) = (FrameViewList[frameIndex + 1], FrameViewList[frameIndex]);
        }

        int index = 0;

        // Adjust the index for all the remaining chidren
        foreach (object item in spFrames.Children)
        {
            if (item is CharacterFrameView view)
            {
                view.FrameIndex = index++;
            }
        }
    }

    private void OnDeleteAnimationFrame(string animationID, int frameIndex)
    {
        if (DataContext is not CharacterAnimationViewModel viewModel)
        {
            return;
        }

        if (viewModel.TabID != animationID)
        {
            return;
        }

        spFrames.Children.RemoveAt(frameIndex);

        foreach (CharacterFrameView frame in FrameViewList)
        {
            if (frame.FrameIndex == frameIndex)
            {
                FrameViewList.Remove(frame);
                break;
            }
        }

        int index = 0;

        // Adjust the index for all the remaining chidren
        foreach (object item in spFrames.Children)
        {
            if (item is CharacterFrameView view)
            {
                view.FrameIndex = index++;
            }
        }
    }

    private void OnNewAnimationFrame(string animationID, string frameID, int newIndex, bool isHeldFrame)
    {
        if (DataContext is not CharacterAnimationViewModel viewModel)
        {
            return;
        }

        if (viewModel.TabID != animationID)
        {
            return;
        }

        var model = viewModel.CharacterModel;

        if (viewModel.FileHandler == null || model == null)
        {
            return;
        }

        CharacterFrameView frame = new(animationID, frameID, spFrames.Children.Count - 1, viewModel.FileHandler, model, isHeldFrame);

        // Insert last
        if (newIndex == -1)
        {
            FrameViewList.Add(frame);

            spFrames.Children.Insert(spFrames.Children.Count - 1, frame);
        }
        else
        {
            // Insert in between

            spFrames.Children.Insert(newIndex, frame);

            FrameViewList.Insert(newIndex, frame);

            int index = 0;

            // Adjust the index for all the remaining chidren
            foreach (object item in spFrames.Children)
            {
                if (item is CharacterFrameView view)
                {
                    view.FrameIndex = index++;
                }
            }
        }
    }

    private void OnInformationToCorrectlyDisplayTheMetaSpriteCentered(double imageMostLeft, double imageMostTop, double scale)
    {
        if (!_imageReady)
        {
            return;
        }

        UpdateImagePosition(previewImage, imageMostLeft, imageMostTop, scale);
    }

    private void UpdateImagePosition(Image image, double imageMostLeft, double imageMostTop, double scale)
    {
        imageMostLeft *= scale;
        imageMostTop *= scale;

        double top = imageMostTop - _distanceTop + _centerY;
        double left = imageMostLeft - _distanceLeft + _centerX;

        Canvas.SetLeft(image, left);
        Canvas.SetTop(image, top);
    }

    private void PreviewImage_Loaded(object sender, RoutedEventArgs e)
    {
        _imageReady = true;

        if (sender is not Image image)
        {
            return;
        }

        if (image.RenderTransform is not ScaleTransform transform)
        {
            return;
        }

        if (DataContext is not CharacterAnimationViewModel viewModel)
        {
            return;
        }

        if (string.IsNullOrEmpty(viewModel.AnimationID))
        {
            return;
        }

        if (string.IsNullOrEmpty(viewModel.FrameID))
        {
            return;
        }

        CharacterModel? model = viewModel.CharacterModel;

        if (model == null)
        {
            return;
        }

        if (!model.Animations.TryGetValue(viewModel.AnimationID, out CharacterAnimation? animation))
        {
            return;
        }

        Rectangle<double> rect = animation.GetFrameBoundingBox(viewModel.FrameID);

        double scale = transform.ScaleX;

        _centerX = (parentCanvas.ActualWidth - (rect.Width * scale)) / 2;
        _centerY = (parentCanvas.ActualHeight - (rect.Height * scale)) / 2;

        _distanceLeft = rect.X * scale;
        _distanceTop = rect.Y * scale;

        UpdateImagePosition(image, rect.X, rect.Y, scale);
    }
}