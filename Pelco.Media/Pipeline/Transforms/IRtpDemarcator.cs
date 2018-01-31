using Pelco.Media.RTP;

namespace Pelco.Media.Pipeline.Transforms
{
    /// <summary>
    /// An IRtpDemarcator is used to determine if a packet belongs to the
    /// next from or not.
    /// </summary>
    public interface IRtpDemarcator
    {
        /// <summary>
        /// Performs check to determine if the packet belongs to the
        /// next frame or not.
        /// </summary>
        /// <param name="packet">The packet to check</param>
        /// <returns>true if it does false otherwise</returns>
        bool Check(RtpPacket packet);
    }
}
