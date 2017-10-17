using Pelco.Media.RTSP.Client;
using Pelco.Media.RTSP.Server;
using Pelco.Media.Tests.Utils;
using System;
using System.Threading;

namespace Pelco.Media.Tests.Integrations
{
    public class RtspCommunicationFixture : IDisposable
    {
        private static readonly string URI_PATH = "/test";

        private int _cseq;
        private RtspServer _server;

        public RtspCommunicationFixture()
        {
            ServerPort = NetworkUnil.FindAvailableTcpPort();

            var dispatcher = new DefaultRequestDispatcher();
            dispatcher.RegisterHandler(URI_PATH, new TestRequestHandler());

            _server = new RtspServer(ServerPort, dispatcher);
            _server.Start();

            // Wait until the serer port is not available.
            while (NetworkUnil.IsTcpPortAvailable(ServerPort))
            {
                Thread.Sleep(1000);
            }

            Client = new RtspClient(ServerUriEndpoint);
        }

        public RtspClient Client { get; private set; }

        public int ServerPort { get; private set; }

        public Uri ServerUriEndpoint
        {
            get
            {
                return new Uri($"rtsp://127.0.0.1:{ServerPort}{URI_PATH}");
            }
        }

        public int NextCseq()
        {
            return Interlocked.Increment(ref _cseq);
        }

        public void Dispose()
        {
            _server.Stop();
            Client.Close();
        }
    }
}
