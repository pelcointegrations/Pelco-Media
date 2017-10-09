using Pelco.PDK.Media.Pipeline;
using System;
using System.Collections.Immutable;

namespace Pelco.PDK.Media.RTP
{
    /// <summary>
    /// Class representation of an RTP packet as specified in RFC 3550, section 5.1.
    ///
    /// The following table shows the RTP packet format contained in this class.
    /// 
    ///                       1                   2                   3
    /// 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ 
    /// |V=2|P|X|   CC  |M|    PT       |        sequence number        |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |                           timestamp                           |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ 
    /// |              synchronization source (SSRC) identifier         |
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+ 
    /// |         contributing source (CSRC) identifiers                | 
    /// |                            ....                               |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ///                       1                   2                   3
    /// 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ 
    /// | defined by profile |      length                              | 
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ 
    /// |          header extension                                     | 
    /// | ....                                                          |
    /// </summary>
    public class RtpPacket
    {
        private static readonly Int32 BYTES_IN_WORD = 4;
        private static readonly int MIN_RTP_PACKET_SIZE = 12;

        public RtpPacket()
        {
            ExtensionHeaderData = 0;
            Marker = false;
            CsrcIds = ImmutableList.Create<uint>();
        }

        #region Properties

        public RtpVersion Version { get; internal set; }

        public byte PayloadType { get; internal set; }

        public bool HasExtensionHeader { get; internal set; }

        public ushort ExtensionHeaderData { get; internal set; }

        public bool Marker { get; internal set; }

        public ushort SequenceNumber { get; internal set; }

        public uint SSRC { get; internal set; }

        public uint Timestamp { get; internal set; }

        public ImmutableList<uint> CsrcIds { get; internal set; }

        public ByteBuffer ExtensionData { get; internal set; }

        public ByteBuffer Payload { get; internal set; }

        #endregion

        public static RtpPacket Decode(ByteBuffer buffer)
        {
            if (buffer.Length < MIN_RTP_PACKET_SIZE)
            {
                throw new ArgumentException("Buffer length is less that the the minimum RTP packet size, probably not RTP data");
            }

            var packet = new RtpPacket();

            byte b = buffer.ReadByte();
            packet.Version = RtpVersion.FromByte(b);
            packet.HasExtensionHeader = (byte)((b & 0x10) >> 4) == 1;

            bool hasPadding = (byte)((b & 0x20) >> 5) == 1;
            short csrcCount = (short)(b & 0x0F);

            b = buffer.ReadByte();
            packet.Marker = ((b & 0x80) != 0);
            packet.PayloadType = (byte)(b & 0x7F);
            packet.SequenceNumber = buffer.ReadUInt16AsHost();
            packet.Timestamp = buffer.ReadUInt32AsHost();
            packet.SSRC = buffer.ReadUInt32AsHost();

            if (csrcCount >= 1)
            {
                for (int i = 0; i < csrcCount; ++i)
                {
                    packet.CsrcIds.Add(buffer.ReadUInt32AsHost());
                }
            }

            if (packet.HasExtensionHeader)
            {
                // If there is an extension header.  Then we need to determine the profile
                // specific id, and the length of the extension header.  The extension header
                // is a 32-bit work 16-bits for the profile id, and16-bits for the length.
                packet.ExtensionHeaderData = buffer.ReadUInt16AsHost();

                // The length is defined in 32-bit words
                Int32 length = buffer.ReadUInt16AsHost() * BYTES_IN_WORD;

                packet.ExtensionData = buffer.ReadSlice(length);
            }

            // TODO(frank.lamar): Add support for stripping padding off.
            packet.Payload = buffer.ReadSlice();

            return packet;
        }

        public static ByteBuffer Encode(RtpPacket packet, int fixedBlockSize)
        {
            int packetSize = MIN_RTP_PACKET_SIZE;

            if (packet.HasExtensionHeader)
            {
                packetSize += (BYTES_IN_WORD + packet.ExtensionData.Length);
            }

            packetSize += packet.CsrcIds.Count * BYTES_IN_WORD;
            packetSize += packet.Payload.Length;

            int paddingSize = 0;
            if (fixedBlockSize > 0)
            {
                // If padding modulus is > 0 then the padding is equal to:
                // (global size of the compound RTP packet) mod (block size)
                // Block size alignment might be necessary for some encryption algorithms
                // RFC section 6.4.1
                paddingSize = fixedBlockSize - (packetSize % fixedBlockSize);
                if (paddingSize == fixedBlockSize)
                {
                    paddingSize = 0;
                }
            }
            packetSize += paddingSize;

            ByteBuffer buffer = new ByteBuffer(packetSize);

            byte b = 0x00;

            switch (packet.Version.Value())
            {
                case 1:
                    b |= 0x40;
                    break;

                case 3:
                    b |= 0xC0;
                    break;

                case 2:
                default:
                    b |= 0x80;
                    break;
            }

            if (paddingSize > 0)
            {
                b |= 0x20;
            }

            if (packet.HasExtensionHeader)
            {
                b |= 0x10;
            }

            b |= (byte)packet.CsrcIds.Count;

            buffer.WriteByte(b);

            b = 0x00;
            if (packet.Marker)
            {
                b |= 0x80;
            }

            b |= packet.PayloadType;
            buffer.WriteByte(b);

            buffer.WriteUInt16NetworkOrder(packet.SequenceNumber);
            buffer.WriteUint32NetworkOrder(packet.Timestamp);
            buffer.WriteUint32NetworkOrder(packet.SSRC);

            if (!packet.CsrcIds.IsEmpty)
            {
                foreach (var csrc in packet.CsrcIds)
                {
                    buffer.WriteUint32NetworkOrder(csrc);
                }
            }

            if (packet.HasExtensionHeader)
            {
                buffer.WriteUInt16NetworkOrder(packet.ExtensionHeaderData);
                buffer.WriteUInt16NetworkOrder((UInt16)(packet.ExtensionData.Length / BYTES_IN_WORD));
                buffer.Write(packet.ExtensionData);
            }

            buffer.Write(packet.Payload);

            return buffer;
        }
    }
}
