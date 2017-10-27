using Pelco.Media.Pipeline;
using System.Windows;

namespace Pelco.PDK.Metadata.UI
{
    public interface IVideoOverlayCanvas<T> : IObjectTypeSink<T>
    {
        FrameworkElement GetVisualOverlay();

        void OnOverlayWindowChange(Rect normalizedVideoWindow, double rotation);

        void OnOverlayDigitalPtzChange(Rect normalizedPtzWindow);

        void OnOverlayStreamAspectRatioChange(double aspectRatio);
    }
}
