using FacialDetectionCommon;
using Pelco.Metadata.UI;
using Pelco.Metadata.UI.Overlays;
using System.Windows;
using System.Windows.Media;

namespace TestApp
{
    public class DrawingCanvas : VideoOverlayCanvasBase<FacialDiscovery>
    {
        public override bool HandleObject(FacialDiscovery discovered)
        {
            Application.Current.Dispatcher.Invoke(() =>
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
            });

            return true;
        }
    }
}
