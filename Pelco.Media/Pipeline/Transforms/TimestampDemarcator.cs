using Pelco.Media.RTP;

namespace Pelco.Media.Pipeline.Transforms
{
    /// <summary>
    /// Demarcator used to declare a packetto belong to the next frame if
    /// the previous packet had a different timestamp.
    /// </summary>
    public class TimestampDemarcator : IRtpDemarcator
    {
        long _lastTime = -1;

        /// <summary>
        /// <see cref="IRtpDemarcator.Check(RtpPacket)"/>
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public bool Check(RtpPacket packet)
        {
            if (_lastTime != packet.Timestamp)
            {
                var current = _lastTime;
                _lastTime = packet.Timestamp;
                return current != -1;
            }
            return false;
        }
    }
}
