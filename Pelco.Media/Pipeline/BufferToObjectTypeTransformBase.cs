namespace Pelco.Media.Pipeline
{
    public abstract class BufferToObjectTypeTransformBase<T> : ObjectTypeSource<T>, ITransform
    {
        public new void Stop()
        {
            base.Stop();
        }

        public abstract bool WriteBuffer(ByteBuffer buffer);
    }
}
