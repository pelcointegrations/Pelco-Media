//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using System;
using System.Windows;
using Pelco.Media.Pipeline;
using Pelco.UI.VideoOverlay.Overlays;

namespace Pelco.UI.VideoOverlay
{
    public class VideoOverlayCanvasBase<T> : IVideoOverlayCanvas<T>
    {
        private VideoOverlayCanvas _canvas;
        private VideoOverlayCanvasViewModel _viewModel;

        public VideoOverlayCanvasBase()
        {
            _viewModel = new VideoOverlayCanvasViewModel();
            _canvas = new VideoOverlayCanvas(_viewModel);
        }

        public bool IsLiveStream
        {
            get
            {
                return _viewModel.IsLiveStream;
            }

            set
            {
                _viewModel.IsLiveStream = value;
            }
        }

        public ISource UpstreamLink { set; get; }

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

        public void UpdatePlaybackTimingInfo(DateTime? anchor, DateTime? initiation, double scale)
        {
            _viewModel.UpdatePlaybackTimingInfo(anchor, initiation, scale);
        }

        public void OnOverlayWindowChange(Rect normalizedVideoWindow, double rotation)
        {
            _canvas.Dispatcher.BeginInvoke((Action)(() =>
            {
                _viewModel.OnOverlaySizeChange(normalizedVideoWindow, rotation);
            }));
        }

        public void OnOverlayDigitalPtzChange(Rect normalizedPtzWindow)
        {
        }

        public void OnOverlayStreamAspectRatioChange(double aspectRatio)
        {
            _canvas.Dispatcher.BeginInvoke((Action)(() =>
            {
                _viewModel.OnStreamAspectRationChange(aspectRatio);
            }));
        }

        protected void DrawOverlay(OverlayDrawing overlay)
        {
            _viewModel.AddOverlay(overlay);
        }

        protected void RemoveOverlay(string overlayId)
        {
            _viewModel.RemoveOverlay(overlayId);
        }

        protected void ClearOverlays()
        {
            _viewModel.RemoveAllOverlays();
        }

        public void PushEvent(MediaEvent e)
        {
            UpstreamLink?.OnMediaEvent(e);
        }
    }
}
