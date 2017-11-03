using System;

namespace Pelco.Media.Pipeline
{
    public abstract class ObjectTypeToBufferTransformBase<T> : TransformBase, IObjectTypeSink<T>
    {
        public abstract bool HandleObject(T obj);
    }
}
