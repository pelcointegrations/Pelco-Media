using System;

namespace Pelco.PDK.Media.Pipeline
{
    public abstract class ObjectTypeHandlerBase<T> : IObjectTypeSink<T>
    {
        public void Stop()
        {
        }

        public bool WriteBuffer(ByteBuffer buffer)
        {
            throw new InvalidOperationException();
        }

        public abstract bool HandleObject(T obj);
    }
}
