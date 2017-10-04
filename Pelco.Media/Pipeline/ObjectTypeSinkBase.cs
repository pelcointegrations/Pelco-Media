using System;

namespace Pelco.PDK.Media.Pipeline
{
    public abstract class ObjectTypeSinkBase<T> : IObjectTypeSink<T>
    {
        public abstract bool HandleObject(T obj);

        public void Stop()
        {
        }

        public bool WriteBuffer(ByteBuffer buffer)
        {
            throw new InvalidOperationException();
        }
    }
}
