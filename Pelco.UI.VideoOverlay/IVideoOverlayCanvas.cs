using Pelco.Media.Pipeline;
using System;
using System.Windows;

namespace Pelco.UI.VideoOverlay
{
    public interface IVideoOverlayCanvas<T> : IObjectTypeSink<T>
    {
        FrameworkElement GetVisualOverlay();

        bool IsLiveStream { get; set; }

        void UpdatePlaybackTimingInfo(DateTime? anchor, DateTime? initiation, double scale);

        void OnOverlayWindowChange(Rect normalizedVideoWindow, double rotation);

        void OnOverlayDigitalPtzChange(Rect normalizedPtzWindow);

        void OnOverlayStreamAspectRatioChange(double aspectRatio);
    }
}
