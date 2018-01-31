using Pelco.Media.RTP;

namespace Pelco.Media.Pipeline.Transforms
{
    /// <summary>
    /// Demarcator used to declare a packet to belong to the next frame if
    /// the previous packet had its marker bit set. This demarcator is vulnerable
    /// to packet loss.
    /// </summary>
    public class MarkerDemarcator : IRtpDemarcator
    {
        /// <summary>
        /// <see cref="IRtpDemarcator.Check(RtpPacket)"/>
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public bool Check(RtpPacket packet)
        {
            return packet.Marker;
        }
    }
}
