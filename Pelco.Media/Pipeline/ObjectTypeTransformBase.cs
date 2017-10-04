using System;

namespace Pelco.PDK.Media.Pipeline
{
    public abstract class ObjectTypeTransformBase<SRC, TARGET> : ObjectTypeSource<TARGET>, IObjectTypeSink<SRC>, ITransform
    {
        public abstract bool HandleObject(SRC obj);

        public virtual bool WriteBuffer(ByteBuffer buffer)
        {
            throw new InvalidOperationException();
        }
    }
}
