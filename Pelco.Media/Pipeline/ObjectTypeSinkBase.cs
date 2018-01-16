using System;

namespace Pelco.Media.Pipeline
{
    public abstract class ObjectTypeSinkBase<T> : IObjectTypeSink<T>
    {
        private ISource _upstreamLink;

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

        public abstract bool HandleObject(T obj);

        public void Stop()
        {
        }

        public virtual void PushEvent(MediaEvent e)
        {
            UpstreamLink?.OnMediaEvent(e);
        }

        public bool WriteBuffer(ByteBuffer buffer)
        {
            throw new InvalidOperationException();
        }
    }
}
