using Pelco.Media.Pipeline;
using Pelco.Media.RTP;
using System;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using Xunit;

namespace Pelco.Media.Tests.RTP
{
    public class RtpPacketTests
    {
        /*
         * This packet was taken from wire shark. The packet data is provided below
         * 
         * Version : 2
         * Padding : No
         * Extension : No
         * Marker : No
         * CSRC Count : 0
         * Payload Type : 97 (dynamic)
         * Sequence Number: 40179
         * Timestamp : 3616193167
         * SSRC : 1849968209
         * Payload : NAL Unit data (SEI) type 6 Header
         * Size : 12 bytes
         * Payload Size : 51 bytes
         */
        private static readonly string RTP_HEX =
            "80619CF3D78ABA8F6E44465106052c1505a8a8af944bb4bd9e6571836625ae50" +
            "4c434f000002140002028000030c53dbd30b00010c4bcf0168000280808000";

        private byte[] _packetBytes;

        public RtpPacketTests()
        {
            var hex = SoapHexBinary.Parse(RTP_HEX);
            _packetBytes = hex.Value;
        }

        [Fact]
        public void TestDecode()
        {
            var buffer = new ByteBuffer(_packetBytes, 0, _packetBytes.Length);
            var packet = RtpPacket.Decode(buffer);

            Assert.True(packet.Version.Is(RtpVersion.V2));
            Assert.False(packet.HasExtensionHeader);
            Assert.False(packet.Marker);
            Assert.True(packet.CsrcIds.IsEmpty);
            Assert.Equal(97, packet.PayloadType);
            Assert.Equal(40179, packet.SequenceNumber);
            Assert.Equal(3616193167, packet.Timestamp);
            Assert.Equal((uint)1849968209, packet.SSRC);
            Assert.Equal(51, packet.Payload.Length);
        }

        [Fact]
        public void TestEncode()
        {
            var buffer = new ByteBuffer(_packetBytes, 0, _packetBytes.Length);
            var packet = RtpPacket.Decode(buffer);
            var encoded = packet.Encode();
            var toTest = RtpPacket.Decode(encoded);

            Assert.True(toTest.Version.Is(RtpVersion.V2));
            Assert.False(toTest.HasExtensionHeader);
            Assert.False(toTest.Marker);
            Assert.True(toTest.CsrcIds.IsEmpty);
            Assert.Equal(97, toTest.PayloadType);
            Assert.Equal(40179, toTest.SequenceNumber);
            Assert.Equal(3616193167, toTest.Timestamp);
            Assert.Equal((uint)1849968209, toTest.SSRC);
            Assert.Equal(51, toTest.Payload.Length);
        }

        [Fact]
        public void TestHeaderExtension()
        {
            var buffer = new ByteBuffer(_packetBytes, 0, _packetBytes.Length);
            var packet = RtpPacket.Decode(buffer);
            var date = new DateTime(2017, 10, 17, 4, 33, 17, 32, DateTimeKind.Local);

            var headerBuf = new OnvifRtpHeader()
            {
                Time = date,
                CbitSet = true,
                DbitSet = true,
                EbitSet = true,
                CSeq = 3
            }.Encode();

            packet.HasExtensionHeader = true;
            packet.ExtensionHeaderData = OnvifRtpHeader.PROFILE_ID;
            packet.ExtensionData = headerBuf;

            var encoded = packet.Encode();
            var toTest = RtpPacket.Decode(encoded);

            Assert.True(toTest.Version.Is(RtpVersion.V2));
            Assert.True(toTest.HasExtensionHeader);
            Assert.False(toTest.Marker);
            Assert.True(toTest.CsrcIds.IsEmpty);
            Assert.Equal(97, toTest.PayloadType);
            Assert.Equal(40179, toTest.SequenceNumber);
            Assert.Equal(3616193167, toTest.Timestamp);
            Assert.Equal((uint)1849968209, toTest.SSRC);
            Assert.Equal(51, toTest.Payload.Length);
            Assert.Equal(OnvifRtpHeader.PROFILE_ID, packet.ExtensionHeaderData);

            var header = OnvifRtpHeader.Decode(toTest.ExtensionData);

            Assert.Equal(date, header.Time.ToLocalTime());
            Assert.True(header.CbitSet);
            Assert.True(header.DbitSet);
            Assert.True(header.EbitSet);
            Assert.Equal(3, header.CSeq);
        }
    }
}
