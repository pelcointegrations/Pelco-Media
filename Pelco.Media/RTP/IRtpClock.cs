using Pelco.Media.Pipeline;

namespace Pelco.Media.RTP
{
    public interface IClockInstant
    {
        void Apply(RtpPacket packet);
    }

    /// <summary>
    /// Clock used to convert from absolute times to RTP times.
    /// </summary>
    public interface IRtpClock
    {
        /// <summary>
        /// Returns a <see cref="IClockInstant"/> object from the provided buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        IClockInstant Clock(ByteBuffer buffer);
    }
}
