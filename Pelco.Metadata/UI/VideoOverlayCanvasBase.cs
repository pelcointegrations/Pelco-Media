using System;
using System.Windows;
using Pelco.Media.Pipeline;
using Pelco.Metadata.UI.Overlays;

namespace Pelco.Metadata.UI
{
    public class VideoOverlayCanvasBase<T> : IVideoOverlayCanvas<T>
    {
        private VideoOverlayCanvas _canvas;

        public VideoOverlayCanvasBase()
        {
            _canvas = new VideoOverlayCanvas();
        }

        public FrameworkElement GetVisualOverlay()
        {
            return _canvas;
        }

        public virtual bool HandleObject(T obj)
        {
            return true;
        }

        public bool WriteBuffer(ByteBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
        }

        public void OnOverlayWindowChange(Rect normalizedVideoWindow, double rotation)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _canvas.DrawingCanvas.OnOverlayWindowChange(normalizedVideoWindow, rotation);
            });
        }

        public void OnOverlayDigitalPtzChange(Rect normalizedPtzWindow)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _canvas.DrawingCanvas.OnOverlayDigitalPtzChange(normalizedPtzWindow);
            });
        }

        public void OnOverlayStreamAspectRatioChange(double aspectRatio)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _canvas.DrawingCanvas.OnOverlayStreamAspectRatioChange(aspectRatio);
            });
        }

        protected void DrawOverlay(OverlayDrawing drawing)
        {
            _canvas.DrawingCanvas.Draw(drawing);
        }

        protected void RemoveOverlay(string overlayId)
        {
            _canvas.DrawingCanvas.Remove(overlayId);
        }

        protected void ClearOverlays()
        {
            _canvas.DrawingCanvas.RemoveAllOverlays();
        }
    }
}
