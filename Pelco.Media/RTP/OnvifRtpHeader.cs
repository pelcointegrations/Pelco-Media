using Pelco.Media.Common;
using Pelco.Media.Pipeline;
using System;

namespace Pelco.Media.RTP
{
    public class OnvifRtpHeader
    {
        public static readonly UInt16 PROFILE_ID = 0xABAC;

        private static readonly byte E_BIT_FLAG_MASK = 0x40;
        private static readonly byte D_BIT_FLAG_MASK = 0x20;
        private static readonly byte C_BIT_FLAG_MASK = 0x80;
        private static readonly int PACKET_SIZE_IN_BYTES = 12;

        public DateTime Time { get; set; }

        public bool CbitSet { get; set; }

        public bool DbitSet { get; set; }

        public bool EbitSet { get; set; }

        public byte CSeq { get; set; }

        public static OnvifRtpHeader Decode(ByteBuffer buffer)
        {
            var ntpTime = new NtpTime(buffer.ReadInt64AsHost());
            byte b = buffer.ReadByte();

            return new OnvifRtpHeader()
            {
                Time = ntpTime.UtcDate,
                CbitSet = (b & C_BIT_FLAG_MASK) != 0,
                DbitSet = (b & D_BIT_FLAG_MASK) != 0,
                EbitSet = (b & E_BIT_FLAG_MASK) != 0,
                CSeq = buffer.ReadByte()
            };

        }

        public ByteBuffer Encode()
        {
            var buffer = new ByteBuffer(PACKET_SIZE_IN_BYTES);

            NtpTime ntpTime = new NtpTime(Time);
            buffer.WriteUint32NetworkOrder(ntpTime.Seconds);
            buffer.WriteUint32NetworkOrder(ntpTime.Fraction);

            byte b = 0x00;

            if (CbitSet)
            {
                b |= C_BIT_FLAG_MASK;
            }

            if (DbitSet)
            {
                b |= D_BIT_FLAG_MASK;
            }

            if (EbitSet)
            {
                b |= E_BIT_FLAG_MASK;
            }

            buffer.WriteByte(b);
            buffer.WriteByte(CSeq);
            buffer.WriteInt16(0);
            buffer.SetPosition(0, ByteBuffer.PositionOrigin.BEGINNING); // Re-set so that we can read from the list
            buffer.MarkReadOnly();

            return buffer;
        }
    }
}
