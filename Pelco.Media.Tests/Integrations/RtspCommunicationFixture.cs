using Pelco.Media.RTSP.Client;
using Pelco.Media.RTSP.Server;
using Pelco.Media.Tests.Utils;
using System;
using System.Threading;

namespace Pelco.Media.Tests.Integrations
{
    public class RtspCommunicationFixture : IDisposable
    {
        private int _cseq;
        private string _path;
        private RtspServer _server;

        public RtspCommunicationFixture()
        {
            Initialized = false;
        }

        public void Initialize(string path, IRequestHandler handler)
        {
            if (!Initialized)
            {
                ServerPort = NetworkUnil.FindAvailableTcpPort();

                var dispatcher = new DefaultRequestDispatcher();
                dispatcher.RegisterHandler(path, handler);

                _path = path;
                _server = new RtspServer(ServerPort, dispatcher);
                _server.Start();

                // Wait until the serer port is not available.
                while (NetworkUnil.IsTcpPortAvailable(ServerPort))
                {
                    Thread.Sleep(1000);
                }

                Client = new RtspClient(ServerUriEndpoint);

                Initialized = true;
            }
        }

        public bool Initialized { get; private set; }

        public RtspClient Client { get; private set; }

        public int ServerPort { get; private set; }

        public Uri ServerUriEndpoint
        {
            get
            {
                return new Uri($"rtsp://127.0.0.1:{ServerPort}{_path}");
            }
        }

        public int NextCseq()
        {
            return Interlocked.Increment(ref _cseq);
        }

        public void Dispose()
        {
            _server?.Stop();
            Client?.Close();
        }
    }
}
