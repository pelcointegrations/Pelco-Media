using Pelco.PDK.Media.Pipeline;

namespace Pelco.PDK.Media.RTP
{
    /// <summary>
    /// Defines an RTP source. An RTP source is also paired with RTCP. Definitions
    /// must define and provide both the source for the RTP and RTCP ports.
    /// </summary>
    public interface IRtpSource
    {
        /// <summary>
        /// Gets the RTP port used to receive data. If the transport
        /// type is interleaved then this method returns the RTP channel.
        /// </summary>
        int RtpPort { get; }

        /// <summary>
        /// Gets the RTCP port used to receive data. If the transport type
        /// is interleaved then this method returns the RTCP channel.
        /// </summary>
        int RtcpPort { get; }

        /// <summary>
        /// Gets the <see cref="MediaPipeline"/> source used for providing RTP data.
        /// </summary>
        ISource RtpSource { get; }

        /// <summary>
        /// Gets the <see cref="MediaPipeline"/> source used for providing RTCP data.
        /// </summary>
        ISource RtcpSource { get; }

        /// <summary>
        /// Starts the source.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the source.
        /// </summary>
        void Stop();
    }
}
