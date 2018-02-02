//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using System;
using System.Windows.Media;

namespace Pelco.UI.VideoOverlay
{
    /// <summary>
    /// Extentions class to the <see cref="CompositionTarget"/> that helps improve performance
    /// issues. Unfortunately, WPF fires this event irregularly, sometimes much more often than
    /// once per frame (4-6 times per frame seems pretty normal in my experience), so it can be
    /// difficult to keep the timing smooth in this event, and difficult to manage performance issues.
    /// 
    /// This class will only create and event that really, truly does fire once per frame.
    /// </summary>
    internal static class CompositionTargetEx
    {
        private static TimeSpan _last = TimeSpan.Zero;
        private static event EventHandler<RenderingEventArgs> _FrameUpdating;

        public static event EventHandler<RenderingEventArgs> Rendering
        {
            add
            {
                if (_FrameUpdating == null)
                {
                    CompositionTarget.Rendering += CompositionTarget_Rendering;
                }

                _FrameUpdating += value;
            }
            remove
            {
                _FrameUpdating -= value;
                if (_FrameUpdating == null)
                {
                    CompositionTarget.Rendering -= CompositionTarget_Rendering;
                }
            }
        }

        static void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            RenderingEventArgs args = (RenderingEventArgs)e;
            if (args.RenderingTime == _last)
                return;
            _last = args.RenderingTime; _FrameUpdating(sender, args);
        }
    }
}
