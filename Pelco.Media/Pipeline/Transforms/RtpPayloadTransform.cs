using NLog;
using Pelco.Media.RTP;
using System;

namespace Pelco.Media.Pipeline.Transforms
{
    public class RtpPayloadTransform : TransformBase
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Converts an RTP packet into an the RTP payload.  Basically it just strips off
        /// the RTP transport info and returns that actual payload data.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public override bool WriteBuffer(ByteBuffer buffer)
        {
            try
            {
                var packet = RtpPacket.Decode(buffer);
                return PushBuffer(packet.Payload);
            }
            catch (Exception e)
            {
                LOG.Error($"Unable to decode RTP packet, reason: {e.Message}");
                return false;
            }
        }
    }
}
