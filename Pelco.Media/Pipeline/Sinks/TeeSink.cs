using NLog;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Pelco.Media.Pipeline.Sinks
{
    public class TeeSink : ISink
    {
        private const int DEFAULT_QUEUE_SIZE = 100;

        private int _queueSize;
        private ISource _upstreamLink;
        private ConcurrentBag<ISink> _clients;

        public ISource UpstreamLink
        {
            get
            {
                return _upstreamLink;
            }

            set
            {
                _upstreamLink = value;
            }
        }

        public TeeSink(int queueSize = DEFAULT_QUEUE_SIZE)
        {
            _queueSize = queueSize;
            _clients = new ConcurrentBag<ISink>();
        }

        public void Stop()
        {
            foreach (var client in _clients)
            {
                client.Stop();
            }

            // Clear the clients
            ISink sink;
            while (!_clients.IsEmpty)
            {
                _clients.TryTake(out sink);
            }
        }

        public ISource CreateSource()
        {
            var source = new TeeOutflowSource(_queueSize);
            _clients.Add(source);

            return source;
        }

        public bool WriteBuffer(ByteBuffer buffer)
        {
            foreach (var client in _clients)
            {
                client.WriteBuffer(buffer);
            }

            return true;
        }

        public virtual void PushEvent(MediaEvent e)
        {
            UpstreamLink?.OnMediaEvent(e);
        }

        class TeeOutflowSource : TransformBase
        {
            private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

            private ManualResetEvent _stop;
            private BlockingCollection<ByteBuffer> _queue;

            public TeeOutflowSource(int queueSize)
            {
                Flushing = true;
                _stop = new ManualResetEvent(false);
                _queue = new BlockingCollection<ByteBuffer>(queueSize);
            }

            public override void Start()
            {
                _stop.Reset();

                Task.Run(() => ProcessBuffers()); // Start processing thread

                // We need to set the flushing flag to false so that the buffers
                // will be processed.
                Flushing = false;
            }

            public override void Stop()
            {
                _stop.Set();
            }

            public override bool WriteBuffer(ByteBuffer buffer)
            {
                if (!_stop.WaitOne(0))
                {
                    if (!_queue.TryAdd(buffer))
                    {
                        LOG.Warn("Dropping buffer queue is full and cannot process the buffer");
                    }
                }

                return true;
            }

            private void ProcessBuffers()
            {
                try
                {
                    while (!_stop.WaitOne(0))
                    {
                        var buffer = _queue.Take();
                        PushBuffer(buffer);
                    }

                    _queue.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    LOG.Info("Shutdown requested for TeeSink source");
                }
                catch (Exception e)
                {
                    LOG.Error($"Received exception while processing buffer, reason={e.Message}");
                }
            }
        }
    }
}
