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
    public class RectangleOverlay : OverlayDrawing
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RectangleOverlay() : base()
        {
        }

        /// <summary>
        /// Gets and sets the rectangle's point for the upper left corner. Points are represented as
        /// normalized points within the range 0-1.
        /// </summary>
        public Point UpperLeft { get; set; }

        /// <summary>
        /// Gets and sets the rectangle's point for the bottom right corner. oints are represented as
        /// normalized points within the range 0-1.
        /// </summary>
        public Point BottomRight { get; set; }

        /// <summary>
        /// Gets and sets the color used to draw the rectangle's border
        /// </summary>
        public Color BorderColor { get; set; } = Colors.Transparent;
 
        public Color FillColor { get; set; } = Colors.Transparent;
  }
}
