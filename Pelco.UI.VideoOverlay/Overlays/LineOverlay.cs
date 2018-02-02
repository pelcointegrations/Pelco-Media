//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using System.Windows;
using System.Windows.Media;

namespace Pelco.UI.VideoOverlay.Overlays
{
    public class LineOverlay : OverlayDrawing
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LineOverlay() : base()
        {
        }

        /// <summary>
        /// Gets and sets the line's start cooridiante point.  Points are represented as
        /// normalized points within the range 0-1.
        /// </summary>
        public Point StartPoint { get; set; }

        /// <summary>
        /// Gets and sets the line's end cooridinate point. Points are represented as
        /// normalized points withing the range 0-1.
        /// </summary>
        public Point EndPoint { get; set; }

        /// <summary>
        /// Gets and set the color used to draw the line
        /// </summary>
        public Color LineColor { get; set; } = Colors.Blue;
    }
}
