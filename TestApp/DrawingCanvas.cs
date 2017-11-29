using FacialDetectionCommon;
using NLog;
using Pelco.UI.VideoOverlay;
using Pelco.UI.VideoOverlay.Overlays;
using System;
using System.Windows;
using System.Windows.Media;

namespace TestApp
{
    public class DrawingCanvas : VideoOverlayCanvasBase<FacialDiscovery>
    {
        public static Logger LOG = LogManager.GetCurrentClassLogger();

        public override bool HandleObject(FacialDiscovery discovered)
        {
            if (discovered != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    ClearOverlays();
                    foreach (var face in discovered.faces.Items)
                    {
                        DrawOverlay(new RectangleOverlay()
                        {
                            BorderColor = Colors.Red,
                            UpperLeft = new Point(face.UpperLeftx, face.UpperLefty),
                            BottomRight = new Point(face.BottomRightx, face.BottomRighty)
                        });
                    }
                }));
            }
            return true;
        }
    }
}
