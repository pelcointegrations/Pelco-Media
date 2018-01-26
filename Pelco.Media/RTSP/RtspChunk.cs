namespace Pelco.Media.RTSP
{
    /// <summary>
    /// Represents a chuck of RTSP data.  In the case of a interleaved frame
    /// the chunk will hold the interleaved payload.  In the case of an Rtsp
    /// response or request it will contain the body payload.
    /// </summary>
    public class RtspChunk
    {
        /// <summary>
        /// Holds the data associated with the rtsp chunk.
        /// </summary>
        protected byte[] Data { get; set; }
    }
}
