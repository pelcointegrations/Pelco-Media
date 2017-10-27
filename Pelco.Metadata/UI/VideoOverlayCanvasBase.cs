using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Pelco.Media.Pipeline;

namespace Pelco.PDK.Metadata.UI
{
    public class VideoOverlayCanvasBase<T> : IVideoOverlayCanvas<T>
    {
        private OverlayDrawingCanvas _canvas;

        public FrameworkElement GetVisualOverlay()
        {
            return _canvas;
        }

        public void OnOverlayDigitalPtzChange(Rect normalizedPtzWindow)
        {
            throw new NotImplementedException();
        }

        public void OnOverlayStreamAspectRatioChange(double aspectRatio)
        {
            throw new NotImplementedException();
        }

        public void OnOverlayWindowChange(Rect normalizedVideoWindow, double rotation)
        {
            throw new NotImplementedException();
        }

        virtual public bool HandleObject(T obj)
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
    }
}
