using System.Windows;
using System.Windows.Controls;

namespace Pelco.UI.VideoOverlay
{
    /// <summary>
    /// Interaction logic for VideoOverlayCanvas.xaml
    /// </summary>
    public partial class VideoOverlayCanvas : UserControl
    {
        public VideoOverlayCanvas(VideoOverlayCanvasViewModel viewModel)
        {
            DataContext = viewModel;

            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as VideoOverlayCanvasViewModel).OnCanvasLoaded(sender as FrameworkElement);
        }

        private void UserControl_UnLoaded(object sender, RoutedEventArgs e)
        {
            (DataContext as VideoOverlayCanvasViewModel).Shutdown();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            (DataContext as VideoOverlayCanvasViewModel).OnSizeChange(e.NewSize);
        }
    }
}
