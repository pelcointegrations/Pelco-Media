using System;

namespace Pelco.PDK.Media.Pipeline
{
    public abstract class ObjectTypeToBufferTransformBase<T> : SourceBase, IObjectTypeSink<T>
    {
        public abstract bool HandleObject(T obj);
        
        public bool WriteBuffer(ByteBuffer buffer)
        {
            throw new NotImplementedException();
        }
    }
}
