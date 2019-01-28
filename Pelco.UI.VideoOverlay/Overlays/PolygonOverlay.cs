//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Pelco.UI.VideoOverlay.Overlays
{
  public class PolygonOverlay : OverlayDrawing
  {
    public PolygonOverlay() : base()
    {

    }

    public List<Point> Points { get; set; } = new List<Point>();

    public Color BorderColor { get; set; } = Colors.Red;

    public Color FillColor { get; set; } = Colors.Transparent;
  }
}
