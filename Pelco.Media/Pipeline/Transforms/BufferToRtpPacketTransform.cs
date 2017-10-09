using NLog;
using Pelco.Media.Pipeline;
using Pelco.Media.RTP;
using System;

namespace Pelco.PDK.Media.Pipeline.Transforms
{
    public class BufferToRtpPacketTransform : BufferToObjectTypeTransformBase<RtpPacket>
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        public override bool WriteBuffer(ByteBuffer buffer)
        {
            try
            {
                var packet = RtpPacket.Decode(buffer);
                return PushObject(packet);
            }
            catch (Exception e)
            {
                LOG.Error($"Unable to decode buffer into RTP packet, reason: {e.Message}");
            }

            return true;
        }
    }
}
