using Pelco.Media.Pipeline;
using Pelco.Media.Tests.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Pelco.Media.Tests.Integrations.Handlers
{
    /// <summary>
    /// Test data source to generate fake data to send over an RTP to
    /// a client.
    /// </summary>
    class TestSource : SourceBase
    {
        public const int DATA_SIZE = 7500; //bytes
        
        private CancellationTokenSource _cancellationSource;

        public TestSource()
        {
            _cancellationSource = new CancellationTokenSource();
        }

        public int FramesSent { get; private set; }

        public bool IsRunning { get; private set; }

        public override void Start()
        {
            Task.Run(() => PushData(_cancellationSource.Token));
        }

        public override void Stop()
        {
            _cancellationSource.Cancel();
        }

        private void PushData(CancellationToken token)
        {
            IsRunning = true;

            while (!token.IsCancellationRequested)
            {
                var randData = RandomUtils.RandomBytes(DATA_SIZE);
                PushBuffer(randData);
                FramesSent += 1;
                Thread.Sleep(40); // Sleep a little before sending the next fake frame of data.
            }

            IsRunning = false;
        }
    }
}
