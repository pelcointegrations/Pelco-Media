using NLog;
using Pelco.Media.RTP;
using System;
using System.Collections.Generic;

namespace Pelco.Media.Pipeline.Transforms
{
    public class RtpPacketizer : TransformBase
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private const int DEFAULT_MTU = 1400;

        private int _mtu;
        private uint _ssrc;
        private ushort _seqNum;
        private byte _payloadType;

        public RtpPacketizer(uint ssrc, byte payloadType, int mtu = DEFAULT_MTU)
        {
            var rand = new Random();

            _mtu = mtu;
            _ssrc = ssrc;
            _seqNum = (ushort)rand.Next(0, ushort.MaxValue);
            _payloadType = payloadType;
        }

        public override bool WriteBuffer(ByteBuffer buffer)
        {
            var slices = SliceData(buffer);

            for (int i = 0; i < slices.Count; ++i)
            {
                var slice = slices[i];
                var packet = new RtpPacket()
                {
                    Version = RtpVersion.V1,
                    Payload = slice,
                    PayloadType = _payloadType,
                    SequenceNumber = ++_seqNum,
                    SSRC = _ssrc,
                    Marker = i == (slices.Count - 1),
                };

                if (buffer.TimeReference != null)
                {
                    var onvifHeader = new OnvifRtpHeader()
                    {
                        CbitSet = true,
                        Time = buffer.TimeReference
                    };

                    packet.HasExtensionHeader = true;
                    packet.ExtensionHeaderData = OnvifRtpHeader.PROFILE_ID;
                    packet.ExtensionData = onvifHeader.Encode();
                }

                PushBuffer(packet.Encode());
            }

            return true;
        }

        protected virtual List<ByteBuffer> SliceData(ByteBuffer buffer)
        {
            List<ByteBuffer> slices = new List<ByteBuffer>(buffer.RemainingBytes / _mtu + 1);

            while (buffer.RemainingBytes > 0)
            {
                var slice = buffer.ReadSlice(Math.Min(buffer.RemainingBytes, _mtu));
                slices.Add(slice);
            }

            return slices;
        }
    }
}
