using NodaTime;
using Pelco.Media.Pipeline;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pelco.Metadata
{
    public class SynchronizingPlaybackTransform<T> : ObjectTypeSource<T>, IObjectTypeSink<T>, ITransform where T : SynchronizedObject
    {
        private static readonly DateTime UNIX_START_TIME = new DateTime(1970, 1, 1);

        private double _scale;
        private long _anchorTime;
        private long _initiatedTime;
        private uint _frameRate = 40; // 40 ms = 25 fps.
        private ManualResetEvent _stopEvent;
        private ConcurrentQueue<T> _objects;

        public SynchronizingPlaybackTransform()
        {
            _scale = 1.0;
            _anchorTime = 0;
            _initiatedTime = 0;
            _objects = new ConcurrentQueue<T>();
            _stopEvent = new ManualResetEvent(false);
        }

        public override void Start()
        {
            base.Start();

            _stopEvent.Reset();
            Task.Run(() => ProcessQueue());
        }

        public override void Stop()
        {
            base.Stop();

            _stopEvent.Set();
        }

        public bool HandleObject(T obj)
        {
            _objects.Enqueue(obj);
            return true;
        }

        public void UpdatePlaybackInfo(DateTime? anchor, DateTime? initiation, double scale)
        {
            lock (this)
            {
                if (anchor.HasValue)
                {
                    _anchorTime = (long)(anchor.Value - UNIX_START_TIME).TotalMilliseconds;
                }

                if (initiation.HasValue)
                {
                    _initiatedTime = (long)(initiation.Value - UNIX_START_TIME).TotalMilliseconds;
                }

                _scale = scale;
            }
        }

        private void ProcessQueue()
        {
            Stopwatch sw = new Stopwatch();

            while (!_stopEvent.WaitOne())
            {
                if (sw.ElapsedMilliseconds >= _frameRate)
                {
                    // Time to check to see if we should pushing
                    // data down the pipeline.
                    
                    T obj;
                    _objects.TryPeek(out obj);
                    var playTime = GetCurrentPlaybackTime();

                    do
                    {
                        if ((playTime >= obj.TimeReference))
                        {
                            _objects.TryDequeue(out obj);
                            if (playTime < obj.TimeReference.AddMilliseconds(_frameRate))
                            {
                                PushObject(obj);
                            }
                        }

                        playTime = GetCurrentPlaybackTime();
                        _objects.TryPeek(out obj);
                    } while (playTime >= obj.TimeReference);

                    sw.Reset();
                }
            }
        }

        private DateTime GetCurrentPlaybackTime()
        {
            lock (this)
            {
                if (_anchorTime == 0 && _initiatedTime == 0)
                {
                    return DateTime.Now;
                }

                long currentAnchor = ((long)((CurrentTime.Get() - _initiatedTime) * _scale) + _anchorTime);
                Instant whereWeShouldBe = Instant.FromUnixTimeMilliseconds(currentAnchor);

                return whereWeShouldBe.ToDateTimeUtc();
            }
        }

        public bool WriteBuffer(ByteBuffer buffer)
        {
            throw new NotImplementedException();
        }
    }

    internal class CurrentTime
    {
        [DllImport("Winmm.dll", EntryPoint = "timeGetTime")]
        public static extern long Get();
    }

}
