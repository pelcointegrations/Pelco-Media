using Pelco.Media.Pipeline;

namespace Pelco.Media.RTSP
{
    /// <summary>
    /// Pipeline event used to indicate that the playback buffer is either high or low.
    /// VxS uess the tcp window to perform an implicit pause. If the
    /// buffer is full then this event will be used to tell the RtspListener
    /// to stop reading to allow the tcp window to fill.
    /// </summary>
    public class PlaybackBufferStateEvent : MediaEvent
    {
        public const string TOPIC = "playback/buffer/state";

        /// <summary>
        /// Inidicates the state of the playback buffer.
        /// </summary>
        public enum State
        {
            Low,
            High
        }

        public PlaybackBufferStateEvent()
        {
            Topic = TOPIC;
        }

        /// <summary>
        /// Gets and sets the flag indicating that the buffer is full.
        /// </summary>
        public State BufferState { get; set; }
    }
}
