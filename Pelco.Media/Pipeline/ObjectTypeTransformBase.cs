using NLog;
using System;

namespace Pelco.Media.Pipeline
{
    public abstract class ObjectTypeTransformBase<SRC, TARGET> : ObjectTypeSource<TARGET>, IObjectTypeSink<SRC>, ITransform
    {
        public abstract bool HandleObject(SRC obj);

        public virtual void PushEvent(MediaEvent e)
        {
            OnMediaEvent(e);
        }

        public virtual bool WriteBuffer(ByteBuffer buffer)
        {
            throw new InvalidOperationException();
        }
    }
}
