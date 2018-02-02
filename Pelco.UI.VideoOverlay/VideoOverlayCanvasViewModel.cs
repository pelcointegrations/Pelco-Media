//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.UI.VideoOverlay.Overlays;
using NLog;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using NodaTime;

namespace Pelco.UI.VideoOverlay
{
    public class VideoOverlayCanvasViewModel : BindableBase
    {
        private static readonly object PlaybackLock = new object();
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private bool _isDirty;
        private double _scale;
        private long _anchorTime;
        private long _initiatedTime;
        private FrameworkElement _control;
        private double _streamAspectRatio;
        private double _videoWindowRotation;
        private Rect _normalizedDPTZWindow;
        private Rect _normalizedViewWindow;
        private Rect _normDerivedVideoWindow;
        private WriteableBitmap _overlayBitmap;
        private ConcurrentDictionary<string, OverlayDrawing> _overlays;

        private CancellationTokenSource _drawLoopTokenSrc;

        public VideoOverlayCanvasViewModel()
        {
            IsLiveStream = true;

            _overlays = new ConcurrentDictionary<string, OverlayDrawing>();
            _drawLoopTokenSrc = new CancellationTokenSource();
            _videoWindowRotation = 0.0;
            _streamAspectRatio = 1920.0 / 1080.0;
            _normalizedDPTZWindow = new Rect(0.0, 0.0, 1.0, 1.0); // default to whole frame (normalized to stream frame)
            _normalizedViewWindow = new Rect(0.0, 0.0, 0.0, 0.0);  // default to zero size to force us to derive from  aspect ratio
        }

        public bool IsLiveStream { get; set; }

        public WriteableBitmap OverlayBitmap
        {
            get
            {
                return _overlayBitmap;
            }

            set
            {
                lock (this)
                {
                    SetProperty(ref _overlayBitmap, value);
                }
            }
        }

        public DelegateCommand<FrameworkElement> LoadedCmd { get; private set; }

        public void AddOverlay(OverlayDrawing overlay)
        {
            RemoveOverlay(overlay.ID);
            _overlays.TryAdd(overlay.ID, overlay);

            _isDirty = true;
        }

        public void RemoveOverlay(string id)
        {
            if (IsLiveStream)
            {
                OverlayDrawing overlay;
                _overlays.TryRemove(id, out overlay);

                _isDirty = true;
            }
        }

        public void RemoveAllOverlays()
        {
            if (IsLiveStream)
            {
                _overlays.Clear();
                _isDirty = true;
            }
        }

        public void UpdatePlaybackTimingInfo(DateTime? anchor, DateTime? initiation, double scale)
        {
            lock (PlaybackLock)
            {
                if (anchor.HasValue)
                {
                    _anchorTime = Instant.FromDateTimeUtc(anchor.Value).ToUnixTimeMilliseconds();
                }

                if (initiation.HasValue)
                {
                    _initiatedTime = Instant.FromDateTimeUtc(initiation.Value).ToUnixTimeMilliseconds();
                }

                _scale = scale;
            }
        }

        public void OnStreamAspectRationChange(double aspectRatio)
        {
            lock (this)
            {
                _streamAspectRatio = aspectRatio;

                ComputeNormalizedDerivedWindow();
            }
        }

        public void OnOverlaySizeChange(Rect normalizedVideoWindow, double rotation)
        {
            lock (this)
            {
                _videoWindowRotation = rotation;
                _normalizedViewWindow = normalizedVideoWindow;
                CreateBitmap(_control.ActualWidth, _control.ActualHeight);

                ComputeNormalizedDerivedWindow();
            }
        }

        public void Shutdown()
        {
            CompositionTargetEx.Rendering -= CompositionTarget_Rendering;
            _overlays.Clear();

            LOG.Info("VideoOverlay canvas has been shutdown");
        }

        private void Draw(OverlayDrawing drawing)
        {
            if (drawing is EllipseOverlay)
            {
                DrawEllipse(drawing as EllipseOverlay);
            }
            else if (drawing is RectangleOverlay)
            {
                DrawRectangle(drawing as RectangleOverlay);
            }
        }

        private void DrawEllipse(EllipseOverlay ellipse)
        {
            var ul = ToActualPoint(ellipse.UpperLeft);
            var br = ToActualPoint(ellipse.BottomRight);
            OverlayBitmap.DrawEllipse((int)ul.X, (int)ul.Y, (int)br.X, (int)br.Y, ellipse.BorderColor);
        }

        private void DrawRectangle(RectangleOverlay overlay)
        {
            var ul = ToActualPoint(overlay.UpperLeft);
            var br = ToActualPoint(overlay.BottomRight);
            OverlayBitmap.DrawRectangle((int)ul.X, (int)ul.Y, (int)br.X, (int)br.Y, overlay.BorderColor);
        }

        private void CreateBitmap<T>(T width, T height)
        {
            if (!_control.CheckAccess())
            {
                _control.Dispatcher.BeginInvoke((Action)(() => { CreateBitmap(width, height); }));
                return;
            }

            int iWidth = Convert.ToInt32(width);
            int iHeight = Convert.ToInt32(height);
            var bitmap = BitmapFactory.New(iWidth, iHeight);
            OverlayBitmap = BitmapFactory.ConvertToPbgra32Format(bitmap);

            LOG.Info($"Video overlay bitmap resized to {iWidth}x{iHeight}");
        }

        private Point ToActualPoint(Point normalizedPoint)
        {
            var retval = new Point();

            if ((_normalizedDPTZWindow.Width == 0.0) ||
                (_normalizedDPTZWindow.Height == 0.0) ||
                (_streamAspectRatio == 0.0))
            {
                // just return zero point for now.  Also should test for epsilon rather that 0.0 with doubles
                return retval;
            }

            retval = normalizedPoint;

            // first compute the dptz corner points in stream pixel space
            double dptzX0 = _streamAspectRatio * _normalizedDPTZWindow.TopLeft.X;
            double dptzY0 = (1.0) * _normalizedDPTZWindow.TopLeft.Y;  // The 1.0 is the normalized height if the width is _streamAspectRatio
            double dptzX1 = _streamAspectRatio * _normalizedDPTZWindow.BottomRight.X;
            double dptzY1 = (1.0) * _normalizedDPTZWindow.BottomRight.Y;

            // compute the input point in stream pixel space
            double x = _streamAspectRatio * normalizedPoint.X;
            double y = (1.0) * normalizedPoint.Y;


            // compute the Video view window points in canvas space
            double vidX0 = _control.ActualWidth * _normDerivedVideoWindow.TopLeft.X;
            double vidY0 = _control.ActualHeight * _normDerivedVideoWindow.TopLeft.Y;
            double vidX1 = _control.ActualWidth * _normDerivedVideoWindow.BottomRight.X;
            double vidY1 = _control.ActualHeight * _normDerivedVideoWindow.BottomRight.Y;

            // linear mapping from stream coordinates to canvas coordinates
            retval.X = (vidX1 - vidX0) * (x - dptzX0) / (dptzX1 - dptzX0) + vidX0;
            retval.Y = (vidY1 - vidY0) * (y - dptzY0) / (dptzY1 - dptzY0) + vidY0;

            return retval;
        }

        private bool IsPointInBounds(Point point)
        {
            bool retval = false;

            double x0 = _normDerivedVideoWindow.TopLeft.X * _control.ActualWidth;
            double y0 = _normDerivedVideoWindow.TopLeft.Y * _control.ActualHeight;

            double x1 = _normDerivedVideoWindow.BottomRight.X * _control.ActualWidth;
            double y1 = _normDerivedVideoWindow.BottomRight.Y * _control.ActualHeight;

            if ((point.X >= x0) && (point.X < x1) &&
                (point.Y >= y0) && (point.Y < y1))
            {
                retval = true;
            }

            return retval;
        }

        private void ComputeNormalizedDerivedWindow()
        {
            if (_control == null)
            {
                return;
            }

            // This is probably obsolete.  I think we are always providing the _normalizedVideoWindow now.

            if ((_normalizedViewWindow.Width == 0.0) || (_normalizedViewWindow.Height == 0.0))
            {
                // We didn't get any video window information so assume the video window is centered and aspect ratio is the
                // same as the stream (ie:dptz didn't change it)

                if ((_control.ActualHeight != 0.0) &&
                    (_control.ActualWidth != 0.0) &&
                    (_streamAspectRatio != 0.0))
                {

                    double aspectCanvas = _control.ActualWidth / _control.ActualHeight;

                    if (aspectCanvas > _streamAspectRatio)
                    {
                        // Full Height, cropped width
                        _normDerivedVideoWindow.Height = 1.0;
                        _normDerivedVideoWindow.Width = _normDerivedVideoWindow.Height * _streamAspectRatio / aspectCanvas;
                    }
                    else
                    {
                        // Full Width, cropped height
                        _normDerivedVideoWindow.Width = 1.0;
                        _normDerivedVideoWindow.Height = aspectCanvas * _normDerivedVideoWindow.Width / _streamAspectRatio;
                    }

                    // Center the image
                    _normDerivedVideoWindow.X = (1.0 - _normDerivedVideoWindow.Width) / 2.0;
                    _normDerivedVideoWindow.Y = (1.0 - _normDerivedVideoWindow.Height) / 2.0;
                }
                else
                {
                    // Probably should just throw an exception but for now we will just zero everything;
                    _normDerivedVideoWindow.X = 0.0;
                    _normDerivedVideoWindow.Y = 0.0;
                    _normDerivedVideoWindow.Width = 0.0;
                    _normDerivedVideoWindow.Height = 0.0;
                }
            }
            else
            {
                _normDerivedVideoWindow = _normalizedViewWindow;
            }
        }

        internal void OnCanvasLoaded(FrameworkElement control)
        {
            if (_control != null)
            {
                // Already loaded.
                return;
            }

            _control = control;
            CreateBitmap(_control.ActualWidth, _control.ActualHeight);
            ComputeNormalizedDerivedWindow();

            // We could use a DispatchTimer for this however we can gain more simplicity and better drawing
            // performance by using the CompositionTarget.Rendering to perform our drawing. When using a DispatchTimer
            // If the interval is set to equal the interval between frames, it ought to execute once per frame.Again,
            // we are met with a sad reality.Timer events in Windows are dispatched on a fairly low priority thread.
            // The documentation guarantees that a timer event will never fire early, but makes no claims about
            // how late it may be. Indeed, in my experience the system is perfectly fine with being 100-150ms late
            // in firing the timer.This means that even if you set your time interval to 1 ms, you’ll still regularly
            // see your event fired only 7-10 times per second.  The CompositionTarget appraoch will give us a more
            // accurate framerate when drawing.
            CompositionTargetEx.Rendering += CompositionTarget_Rendering;

            LOG.Info("VideoOverlay canvas has been loaded.");
        }

        internal void OnSizeChange(Size size)
        {
            if (_control == null)
            {
                // The control has not be created yet.
                return;
            }

            CreateBitmap(size.Width, size.Height);
            ComputeNormalizedDerivedWindow();

            LOG.Info($"VideoOverlay canvas recieved size change event new size {size.Width}x{size.Height}");
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            lock (this)
            {
                    List<OverlayDrawing> toRemove = new List<OverlayDrawing>();

                if (OverlayBitmap != null && (_isDirty || !IsLiveStream))
                {
                    OverlayBitmap.Clear();
                    foreach (var overlay in _overlays)
                    {
                        if (IsLiveStream)
                        {
                            Draw(overlay.Value);
                        }
                        else
                        {
                            var ts = overlay.Value.TimeReference;
                            if (ts != null)
                            {
                                var playbackTime = GetCurrentPlaybackTime();
                                var diff = playbackTime - ts;

                                if (Math.Abs(diff.TotalMilliseconds) <= 20)
                                {
                                    Draw(overlay.Value);
                                }
                                else if (diff.TotalMilliseconds > 100)
                                {
                                    toRemove.Add(overlay.Value);
                                }
                            }
                            else
                            {
                                LOG.Info($"While processing playback overlays, detected overlay '{overlay.Value.ID}' does not have timestamp, ignoring...");
                                toRemove.Add(overlay.Value);
                            }
                        }

                        _isDirty = false;
                    }

                    if (!IsLiveStream)
                    {
                        // Remove old overlays.
                        OverlayDrawing drawing;
                        toRemove.ForEach(o => _overlays.TryRemove(o.ID, out drawing));
                    }
                }
            }
        }

        private DateTime GetCurrentPlaybackTime()
        {
            lock (PlaybackLock)
            {
                var now = SystemClock.Instance.GetCurrentInstant();

                if (_anchorTime == 0 && _initiatedTime == 0)
                {
                    return now.ToDateTimeUtc();
                }

                var current = now.ToUnixTimeTicks() / NodaConstants.TicksPerMillisecond;
                long currentAnchor = ((long)((current - _initiatedTime) * _scale) + _anchorTime);
                Instant whereWeShouldBe = Instant.FromUnixTimeMilliseconds(currentAnchor);

                return whereWeShouldBe.ToDateTimeUtc();
            }
        }
    }
}
