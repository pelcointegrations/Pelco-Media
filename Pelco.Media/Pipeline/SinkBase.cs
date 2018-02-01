namespace Pelco.Media.Pipeline
{
    public abstract class SinkBase : ISink
    {
        public ISource UpstreamLink { get; set; }

        public void PushEvent(MediaEvent e)
        {
            UpstreamLink?.OnMediaEvent(e);
        }

        public virtual void Stop()
        {
        }

        public abstract bool WriteBuffer(ByteBuffer buffer);
    }
}
