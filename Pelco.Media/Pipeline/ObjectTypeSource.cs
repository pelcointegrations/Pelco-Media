namespace Pelco.Media.Pipeline
{
    public class ObjectTypeSource<T> : ISource
    {
        private readonly object FlushingLock = new object();

        private volatile bool _isFlushing;
        private ISink _downstreamLink;

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
                }
            }
        }

        public virtual void Start()
        {
        }

        public virtual void Stop()
        {
            
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
    }
}
