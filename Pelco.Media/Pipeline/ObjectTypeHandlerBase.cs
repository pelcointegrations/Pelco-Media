using NLog;
using System;

namespace Pelco.Media.Pipeline
{
    public abstract class ObjectTypeHandlerBase<T> : IObjectTypeSink<T>
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

        public void Stop()
        {
        }

        public bool WriteBuffer(ByteBuffer buffer)
        {
            throw new InvalidOperationException();
        }

        public virtual void PushEvent(MediaEvent e)
        {
            UpstreamLink?.OnMediaEvent(e);
        }

        public abstract bool HandleObject(T obj);
    }
}
