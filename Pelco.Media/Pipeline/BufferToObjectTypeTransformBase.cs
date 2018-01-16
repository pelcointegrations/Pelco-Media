namespace Pelco.Media.Pipeline
{
    public abstract class BufferToObjectTypeTransformBase<T> : ObjectTypeSource<T>, ITransform
    {
        public new void Stop()
        {
            base.Stop();
        }

        public virtual void PushEvent(MediaEvent e)
        {
            UpstreamLink?.OnMediaEvent(e);
        }

        public abstract bool WriteBuffer(ByteBuffer buffer);
    }
}
