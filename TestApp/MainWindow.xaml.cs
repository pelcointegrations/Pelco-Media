using FacialDetectionCommon;
using FacialRecognition.Metadata;
using Pelco.Media.Common;
using Pelco.Metadata;
using Pelco.Metadata.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PipelineCreator _pc;
        private VxMetadataPlayer _player;
        private IVideoOverlayCanvas<FacialDiscovery> _sink;

        public MainWindow()
        {
            InitializeComponent();

            _sink = new DrawingCanvas();
            _pc = new PipelineCreator(_sink);

            Grid.Children.Add(_sink.GetVisualOverlay());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    var config = new PlayerConfiguration()
                    {
                        PipelineCreator = _pc,
                        TypeFilter = MimeType.CreateApplicationType("vnd.opencv.facial_detection"),
                        Uri = new Uri("rtsp://10.220.232.48:5544/stream?user_info=admin&device_id=7fe3c767-7853-45fe-8a75-045735c11cbe&data_source_id=c91bcf36-20c7-3ad3-97f3-4bcd4c6a1eb5%3AVideoSource%3Ametadata%3Ametadata&data_interface_id=0&multicast=false&transcoded=false")
                    };

                    _player = new VxMetadataPlayer(config);

                    _player.Initialize();
                    _player.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            _player.Dispose();
        }
    }
}
