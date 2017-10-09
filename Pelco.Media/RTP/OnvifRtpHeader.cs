using Pelco.Media.Common;
using Pelco.Media.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public  ByteBuffer Encode()
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

            return buffer;
        }
    }
}
