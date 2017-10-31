using NLog;
using Pelco.Metadata.UI.Overlays;
using Pelco.PDK.Metadata.UI.Overlays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Pelco.Metadata.UI
{
    /// <summary>
    /// Surface on which drawings can be created on top of the chroma key.
    /// </summary>
    public class OverlayDrawingCanvas : FrameworkElement, IPointTranslator
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private double _videoRotation;
        private Rect _normalizedWindow;
        private double _streamAspectRatio;
        private Rect _normalizedDPtzWindow;
        private Rect _normalizedDerivedWindow;
        private readonly VisualCollection _children;
        private readonly Dictionary<string, DrawingContainer> _drawings;

        /// <summary>
        /// Initializes a new instance of <see cref="OverlayDrawingCanvas"/>.
        /// </summary>
        public OverlayDrawingCanvas()
        {
            _videoRotation = 0.0;
            _streamAspectRatio = 1920.0 / 1080.0;
            _normalizedWindow = new Rect(0.0, 0.0, 1.0, 1.0);
            _normalizedDPtzWindow = new Rect(0.0, 0.0, 1.0, 1.0);
            _normalizedDerivedWindow = new Rect(0.0, 0.0, 1.0, 1.0);
            _drawings = new Dictionary<string, DrawingContainer>();
            _children = new VisualCollection(this);

            ClipToBounds = true;
        }

        protected override int VisualChildrenCount => _children.Count;

        public void OnOverlayDigitalPtzChange(Rect normalizedPtzWindow)
        {
            _normalizedDPtzWindow = normalizedPtzWindow;
            OnOverlayCanvasSizeChange();
        }

        public void OnOverlayStreamAspectRatioChange(double aspectRatio)
        {
            _streamAspectRatio = aspectRatio;
            OnOverlayCanvasSizeChange();
        }

        public void OnOverlayWindowChange(Rect normalizedVideoWindow, double rotation)
        {
            _normalizedWindow = normalizedVideoWindow;
            _videoRotation = rotation;
            OnOverlayCanvasSizeChange();
        }

        /// <summary>
        /// Remove the drawing.
        /// </summary>
        /// <param name="drawingId">ID of the drawing to remove.</param>
        /// <returns>
        /// <c>true</c> if the drawing was found and removed. 
        /// <c>false</c> otherwise.
        /// </returns>
        public bool Remove(string drawingId)
        {
            if (!_drawings.TryGetValue(drawingId, out DrawingContainer drawingContainer))
            {
                return false;
            }

            _drawings.Remove(drawingId);
            _children.Remove(drawingContainer.Visual);
            RemoveVisualChild(drawingContainer.Visual);
            RemoveLogicalChild(drawingContainer.Visual);

            return true;
        }

        /// <summary>
        /// Remove all drawings.
        /// </summary>
        public void RemoveAllOverlays()
        {
            var keys = _drawings.Keys.ToList();
            foreach (string id in keys)
            {
                Remove(id);
            }
        }

        /// <summary>
        /// Draw an overlay.
        /// </summary>
        /// <param name="drawing">The overlay to draw.</param>
        public void Draw(OverlayDrawing drawing)
        {
            if (drawing == null)
            {
                return;
            }

            try
            {
                DrawingVisual drawingVisual = new DrawingVisual();
                using (var context = drawingVisual.RenderOpen())
                {
                    drawing.Draw(context, this);
                }

                _drawings.Add(drawing.ID, new DrawingContainer(drawing, drawingVisual));
                _children.Add(drawingVisual);
            }
            catch (Exception e)
            {
                LOG.Error(e, $"Problem drawing shape in overlay, reason={e.Message}");
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }

        public void Redraw()
        {
            _children.Clear();
            foreach (var drawing in _drawings.Values)
            {
                Draw(drawing.OverlayDrawing);
            }
        }

        private void OnOverlayCanvasSizeChange()
        {
            ComputeNormalizedDerivedWindow();

            double vidX0 = this.ActualWidth * _normalizedDerivedWindow.TopLeft.X;
            double vidY0 = this.ActualHeight * _normalizedDerivedWindow.TopLeft.Y;
            double vidX1 = this.ActualWidth * _normalizedDerivedWindow.BottomRight.X;
            double vidY1 = this.ActualHeight * _normalizedDerivedWindow.BottomRight.Y;

            Clip = new RectangleGeometry(new Rect()
            {
                X = Math.Min(vidX0, vidX1),
                Y = Math.Min(vidY0, vidY1),
                Width = Math.Abs(vidX1 - vidX0),
                Height = Math.Abs(vidY1 - vidY0),
            });

            Redraw(); // Redraw the shapes so that if thigs change we re-draw correctly.
        }

        private void ComputeNormalizedDerivedWindow()
        {
            // This is probably obsolete.  I think we are always providing the _normalizedVideoWindow now.

            if ((_normalizedWindow.Width == 0.0) || (_normalizedWindow.Height == 0.0))
            {
                // We didn't get any video window information so assume the video window is centered and aspect ratio is the
                // same as the stream (ie:dptz didn't change it)

                if ((this.ActualHeight != 0.0) &&
                    (this.ActualWidth != 0.0) &&
                    (_streamAspectRatio != 0.0))
                {

                    double aspectCanvas = this.ActualWidth / this.ActualHeight;

                    if (aspectCanvas > _streamAspectRatio)
                    {
                        // Full Height, cropped width
                        _normalizedDerivedWindow.Height = 1.0;
                        _normalizedDerivedWindow.Width = _normalizedDerivedWindow.Height * _streamAspectRatio / aspectCanvas;
                    }
                    else
                    {
                        // Full Width, cropped height
                        _normalizedDerivedWindow.Width = 1.0;
                        _normalizedDerivedWindow.Height = aspectCanvas * _normalizedDerivedWindow.Width / _streamAspectRatio;
                    }

                    // Center the image
                    _normalizedDerivedWindow.X = (1.0 - _normalizedDerivedWindow.Width) / 2.0;
                    _normalizedDerivedWindow.Y = (1.0 - _normalizedDerivedWindow.Height) / 2.0;
                }
                else
                {
                    // Probably should just throw an exception but for now we will just zero everything;
                    _normalizedDerivedWindow.X = 0.0;
                    _normalizedDerivedWindow.Y = 0.0;
                    _normalizedDerivedWindow.Width = 0.0;
                    _normalizedDerivedWindow.Height = 0.0;
                }
            }
            else
            {
                _normalizedDerivedWindow = _normalizedWindow;
            }
        }

        private static void OnVideoRotationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            int newRotation = 0;
            try
            {
                newRotation = (int)e.NewValue;
            }
            catch (Exception ex)
            {
                LOG.Error(ex, $"Could not parse video rotation, reason: {ex.Message}");
            }

            var canvas = (OverlayDrawingCanvas)d;
            RotateTransform transform = new RotateTransform(newRotation,
                                                            canvas.ActualWidth / 2,
                                                            canvas.ActualHeight / 2);

            canvas.RenderTransform = transform;
        }

        public Point TranslatePoint(Point normalizedStreamPoint)
        {
            Point retval = new Point();

            if ((_normalizedDPtzWindow.Width == 0.0) ||
                (_normalizedDPtzWindow.Height == 0.0) ||
                (_streamAspectRatio == 0.0))
            {
                // just return zero point for now.  Also should test for epsilon rather that 0.0 with doubles
                return retval;
            }

            retval = normalizedStreamPoint;

            // brute force this calculation for clarity

            // first compute the dptz corner points in stream pixel space
            double dptzX0 = _streamAspectRatio * _normalizedDPtzWindow.TopLeft.X;
            double dptzY0 = (1.0) * _normalizedDPtzWindow.TopLeft.Y;  // The 1.0 is the normalized height if the width is _streamAspectRatio
            double dptzX1 = _streamAspectRatio * _normalizedDPtzWindow.BottomRight.X;
            double dptzY1 = (1.0) * _normalizedDPtzWindow.BottomRight.Y;

            // compute the input point in stream pixel space
            double x = _streamAspectRatio * normalizedStreamPoint.X;
            double y = (1.0) * normalizedStreamPoint.Y;


            // compute the Video view window points in canvas space
            double vidX0 = this.ActualWidth * _normalizedDerivedWindow.TopLeft.X;
            double vidY0 = this.ActualHeight * _normalizedDerivedWindow.TopLeft.Y;
            double vidX1 = this.ActualWidth * _normalizedDerivedWindow.BottomRight.X;
            double vidY1 = this.ActualHeight * _normalizedDerivedWindow.BottomRight.Y;

            // If we are rotated then adjust the pixel space coordinates according (only supporting 90, 180, 270 rotation)

            // Note: The video window is already rotated.  The stream and dptz isn't.
            if (_videoRotation == 90.0)
            {
                RotatePixelPt90(ref dptzX0, ref dptzY0);
                RotatePixelPt90(ref dptzX1, ref dptzY1);
                RotatePixelPt90(ref x, ref y);

                // Just need to move the origin
                Swap(ref vidX0, ref vidX1);
            }
            else if (_videoRotation == 180.0)
            {
                RotatePixelPt180(ref dptzX0, ref dptzY0);
                RotatePixelPt180(ref dptzX1, ref dptzY1);
                RotatePixelPt180(ref x, ref y);

                // Just need to move the origin
                Swap(ref vidX0, ref vidX1);
                Swap(ref vidY0, ref vidY1);
            }
            else if (_videoRotation == 270.0)
            {
                RotatePixelPt270(ref dptzX0, ref dptzY0);
                RotatePixelPt270(ref dptzX1, ref dptzY1);
                RotatePixelPt270(ref x, ref y);

                // Just need to move the origin
                Swap(ref vidY0, ref vidY1);
            }

            // linear mapping from stream coordinates to canvas coordinates
            retval.X = (vidX1 - vidX0) * (x - dptzX0) / (dptzX1 - dptzX0) + vidX0;
            retval.Y = (vidY1 - vidY0) * (y - dptzY0) / (dptzY1 - dptzY0) + vidY0;

            return retval;
        }

        public bool IsPointInBounds(Point canvasPoint)
        {
            bool retval = false;

            double x0 = _normalizedDerivedWindow.TopLeft.X * this.ActualWidth;
            double y0 = _normalizedDerivedWindow.TopLeft.Y * this.ActualHeight;

            double x1 = _normalizedDerivedWindow.BottomRight.X * this.ActualWidth;
            double y1 = _normalizedDerivedWindow.BottomRight.Y * this.ActualHeight;

            if ((canvasPoint.X >= x0) && (canvasPoint.X < x1) &&
                (canvasPoint.Y >= y0) && (canvasPoint.Y < y1))
            {
                retval = true;
            }

            return retval;
        }

        void RotatePixelPt90(ref double x, ref double y)
        {
            // x y swap and then a flip of the x axis
            Swap<double>(ref x, ref y);
            x = _streamAspectRatio - x;
        }

        void RotatePixelPt180(ref double x, ref double y)
        {
            // a x-y flip of the x and y axis
            x = _streamAspectRatio - x;
            y = 1.0 - y;
        }

        void RotatePixelPt270(ref double x, ref double y)
        {
            // a x y swap and then a flip of the y axis
            Swap<double>(ref x, ref y);
            y = 1.0 - y;
        }

        private static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        private class DrawingContainer
        {
            public DrawingContainer(OverlayDrawing drawing, Visual visual)
            {
                OverlayDrawing = drawing;
                Visual = visual;
            }

            public OverlayDrawing OverlayDrawing { get; }

            public Visual Visual { get; }
        }
    }
}
