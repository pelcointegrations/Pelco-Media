using System;

namespace Pelco.Media.Pipeline
{
    public abstract class InboundObjectHandler<T> : ObjectTypeSource<T>, IObjectTypeSink<T>, ITransform
    {
        /// <summary>
        /// <see cref="IObjectTypeSink{T}.HandleObject(T)"/>
        /// </summary>
        /// <param name="obj">The object to handle</param>
        /// <returns></returns>
        public bool HandleObject(T obj)
        {
            return (obj.GetType() is T) ? DoHandleObject(obj) : PushObject(obj);
        }

        protected abstract bool DoHandleObject(T obj);

        /// <summary>
        /// <see cref="ObjectTypeSource{T}.PushObject(T)"/>
        /// </summary>
        /// <param name="e">The event to push up stream.</param>
        public void PushEvent(MediaEvent e)
        {
            OnMediaEvent(e);
        }

        public bool WriteBuffer(ByteBuffer buffer)
        {
            throw new NotImplementedException();
        }
    }
}
