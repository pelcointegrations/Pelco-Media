using NLog;
using Pelco.Media.Pipeline;
using Pelco.Media.RTSP;
using Pelco.Metadata;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Pelco.UI.VideoOverlay
{
    public class PlaybackBufferTransform<T> : IObjectTypeSink<T>, ITransform where T : SynchronizedObject
    {
        private static readonly object StateLock = new object(); 
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private T _failedObj;
        private BlockingCollection<T> _queue;
        private ManualResetEvent _pushDataEvent;
        private CancellationTokenSource _cancellationSource;
        private PlaybackBufferStateEvent.State _currentBufState;

        public PlaybackBufferTransform()
        {
            _failedObj = default(T);
            _pushDataEvent = new ManualResetEvent(false);
            _cancellationSource = new CancellationTokenSource();
            _queue = new BlockingCollection<T>(new ConcurrentQueue<T>());
        }

        private readonly object FlushingLock = new object();

        private volatile bool _isFlushing;
        private ISink _downstreamLink;
        private ISource _upstreamLink;

        public ISink DownstreamLink
        {
            get
            {
                return _downstreamLink;
            }

            set
            {
                _downstreamLink = value;
            }
        }

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


        public bool Flushing
        {
            get
            {
                return _isFlushing;
            }

            set
            {
                lock (FlushingLock)
                {
                    _isFlushing = value;

                    if (_isFlushing)
                    {
                        ClearQueue();
                    }
                }
            }
        }

        public virtual void Start()
        {
            var token = _cancellationSource.Token;

            Task.Run(() => PushDataDownstream(token), token);

            _pushDataEvent.Set(); // Set so we can send data initially
        }

        public virtual void Stop()
        {
            _cancellationSource.Cancel();
            ClearQueue();
        }


        public bool HandleObject(T obj)
        {
            if (_queue.Count >= 1000)
            {
                LOG.Debug("Playback buffer is full, sending playback buffer high event");
                _currentBufState = PlaybackBufferStateEvent.State.High;

                PushEvent(new PlaybackBufferStateEvent()
                {
                    BufferState = _currentBufState
                });
            }

            _queue.Add(obj);

            return true;
        }

        protected virtual bool PushObject(T obj)
        {
            lock (FlushingLock)
            {
                if (!Flushing && DownstreamLink != null)
                {
                    if (DownstreamLink is IObjectTypeSink<T>)
                    {
                        return ((IObjectTypeSink<T>)DownstreamLink).HandleObject(obj);
                    }
                }

                return true;
            }
        }

        public bool WriteBuffer(ByteBuffer buffer)
        {
            throw new InvalidOperationException();
        }

        public void OnMediaEvent(MediaEvent e)
        {
            if (e is PlaybackBufferStateEvent)
            {
                var evt = e as PlaybackBufferStateEvent;
                if (evt.BufferState == PlaybackBufferStateEvent.State.Low)
                {
                    _pushDataEvent.Set();
                }
            }           
        }

        public void PushEvent(MediaEvent e)
        {
            UpstreamLink?.OnMediaEvent(e);
        }

        private void PushDataDownstream(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_pushDataEvent.WaitOne())
                {
                    if (_failedObj != null)
                    {
                        if (PushObject(_failedObj))
                        {
                            _failedObj = default(T);
                        }
                    }
                    else
                    {
                        var obj = _queue.Take();
                        if (!PushObject(obj))
                        {
                            _failedObj = obj;
                            _pushDataEvent.Reset();
                        }
                    }

                    CheckIfQueueIsLow();
                }
            }
        }

        private void ClearQueue()
        {
            while (_queue.Count > 0)
            {
                T item;
                _queue.TryTake(out item);
            }
        }

        private void CheckIfQueueIsLow()
        {
            lock (StateLock)
            {
                if ((_currentBufState != PlaybackBufferStateEvent.State.Low) && (_queue.Count <= 400))
                {
                    LOG.Debug($"Playback buffer is low, sending buffer low event.");
                    _currentBufState = PlaybackBufferStateEvent.State.Low;

                    PushEvent(new PlaybackBufferStateEvent()
                    {
                        BufferState = _currentBufState
                    });
                }
                else if (_queue.Count > 400)
                {
                    _currentBufState = PlaybackBufferStateEvent.State.High;
                }
            }
        }
    }
}
