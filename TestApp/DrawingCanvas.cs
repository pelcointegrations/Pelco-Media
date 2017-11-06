using FacialDetectionCommon;
using NLog;
using Pelco.Metadata.UI;
using Pelco.Metadata.UI.Overlays;
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
                LOG.Info($"Drawing '{discovered.faces.Items.Count}' faces");
            }
            return true;
        }
    }
}
