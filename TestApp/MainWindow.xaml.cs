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
                        Uri = new Uri("rtsp://127.0.0.1:5544/stream?user_info=admin&device_id=fb1ab8c3-caf4-4d63-af2f-5d6eb7a4b2ac&data_source_id=uuid%3A1801101A-8000-0000-0802-6B6B606E0004%3Ametadata%3Ametadata&data_interface_id=0&multicast=false&transcoded=false")
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
