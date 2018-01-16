namespace Pelco.Media.Pipeline
{
    public class SourceBase : ISource
    {
        private readonly object _flushing_lock = new object();

        private volatile bool _isFlushing;
        private ISink _downstreamLink;
        private ISource _upstreamLink;

        public ISink DownstreamLink
        {
            private get
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
                lock (_flushing_lock)
                {
                    return _isFlushing;
                }
            }

            set
            {
                lock (_flushing_lock)
                {
                    _isFlushing = false;
                }

            }
        }

        /// <summary>
        /// Pushes the buffer downstream if a DownstreamLink is set.
        /// </summary>
        /// <param name="stream">The buffer to push</param>
        /// <returns>True if the buffer is accepted and processed down stream; otherwsie, False</returns>
        protected bool PushBuffer(ByteBuffer buffer)
        {
            lock (_flushing_lock)
            {
                if (!Flushing && DownstreamLink != null)
                {
                    buffer.MarkReadOnly(); // Ensure it is readonly before sending down stream.
                    return DownstreamLink.WriteBuffer(buffer);
                }

                return true;
            }
        }

        public virtual void Start()
        {
        }

        public virtual void Stop()
        {
        }

        public virtual void OnMediaEvent(MediaEvent e)
        {
            UpstreamLink?.OnMediaEvent(e);
        }
    }
}
