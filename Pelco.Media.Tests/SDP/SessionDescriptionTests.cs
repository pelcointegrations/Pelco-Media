using Pelco.Media.RTSP.SDP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pelco.Media.Tests.SDP
{
    public class SessionDescriptionTests
    {
        [Fact]
        public void TestParsingSessionDescription()
        {
            string sdp = "v=0\r\n"
                         + "o = jdoe 2890844526 2890842807 IN IP4 10.47.16.5\r\n"
                         + "s = SDP Seminar\r\n"
                         + "i = A Seminar on the session description protocol\r\n"
                         + "u = http://www.example.com/seminars/sdp.pdf\r\n"
                         + "e = j.doe@example.com(Jane Doe)\r\n"
                         + "c = IN IP4 224.2.17.12 / 127\r\n"
                         + "t = 2873397496 2873404696\r\n"
                         + "a = recvonly\r\n"
                         + "m = audio 49170 RTP / AVP 0\r\n"
                         + "m = video 51372 RTP / AVP 99\r\n"
                         + "a = rtpmap:99 h263-1998/90000\r\n";

            SessionDescription sessiond = SessionDescription.Parse(sdp);

            Assert.Equal("SDP Seminar", sessiond.SessionName);
            Assert.Equal("A Seminar on the session description protocol", sessiond.SessionInformation);
            Assert.Equal(new Uri("http://www.example.com/seminars/sdp.pdf"), sessiond.URI);
            Assert.Equal("j.doe@example.com(Jane Doe)", sessiond.Email);

            var origin = sessiond.Origin;
            Assert.NotNull(origin);
            Assert.Equal("jdoe", origin.Username);
            Assert.Equal(2890844526, origin.SessionId);
            Assert.Equal(2890842807, origin.SessionVersion);

            var connection = sessiond.Connection;
            Assert.NotNull(connection);
            Assert.Equal(NetworkType.IN, connection.NetType);
            Assert.Equal(AddressType.IP4, connection.AddrType);
            Assert.Equal("224.2.17.12", connection.Address);
            Assert.Equal(127, connection.TTL);

            Assert.Single(sessiond.Attributes);
            Assert.Equal("recvonly", sessiond.Attributes.ElementAt(0).Name);

            Assert.Equal(2, sessiond.MediaDescriptions.Count);

            // Validate audio media description
            var md = sessiond.MediaDescriptions.ElementAt(0);
            Assert.Equal(MediaType.AUDIO, md.Media);
            Assert.Equal(49170u, md.Port);
            Assert.Equal(TransportProtocol.RTP_AVP, md.Protocol);
            Assert.Single(md.MediaFormats);
            Assert.Equal(0u, md.MediaFormats.ElementAt(0));
            Assert.Empty(md.Attributes);

            md = sessiond.MediaDescriptions.ElementAt(1);
            Assert.Equal(MediaType.VIDEO, md.Media);
            Assert.Equal(51372u, md.Port);
            Assert.Equal(TransportProtocol.RTP_AVP, md.Protocol);
            Assert.Single(md.MediaFormats);
            Assert.Equal(99u, md.MediaFormats.ElementAt(0));
            Assert.Single(md.Attributes);
            Assert.Equal("rtpmap", md.Attributes.ElementAt(0).Name);
            Assert.Equal("99 h263-1998/90000", md.Attributes.ElementAt(0).Value);
        }

        [Fact]
        public void TestOnvifSpecificationSessionDescription()
        {
            var sdp = "v=0\r\n"
                      + "o=- 2890844256 2890842807 IN IP4 172.16.2.93\r\n"
                      + "s=RTSP Session\r\n"
                      + "m=audio 0 RTP/AVP 0\r\n"
                      + "a=control:rtsp://example.com/onvif_camera/audio\r\n"
                      + "m=video 0 RTP/AVP 26\r\n"
                      + "a=control:rtsp://example.com/onvif_camera/video\r\n"
                      + "m=application 0 RTP/AVP 107\r\n"
                      + "a=control:rtsp://example.com/onvif_camera/metadata\r\n"
                      + "a=recvonly\r\n"
                      + "a=rtpmap:107 vnd.onvif.metadata/90000\r\n";

            SessionDescription sessiond = SessionDescription.Parse(sdp);

            Assert.Equal("RTSP Session", sessiond.SessionName);
            Assert.Null(sessiond.SessionInformation);
            Assert.Null(sessiond.URI);
            Assert.Null(sessiond.Email);

            var origin = sessiond.Origin;
            Assert.NotNull(origin);
            Assert.Equal("-", origin.Username);
            Assert.Equal(2890844256, origin.SessionId);
            Assert.Equal(2890842807, origin.SessionVersion);
            Assert.Equal(NetworkType.IN, origin.NetType);
            Assert.Equal(AddressType.IP4, origin.AddrType);
            Assert.Equal("172.16.2.93", origin.UnicastAddress);

            Assert.Equal(3, sessiond.MediaDescriptions.Count);

            // Validate audio media description
            var md = sessiond.MediaDescriptions.ElementAt(0);
            Assert.Equal(MediaType.AUDIO, md.Media);
            Assert.Equal(0u, md.Port);
            Assert.Equal(TransportProtocol.RTP_AVP, md.Protocol);
            Assert.Single(md.MediaFormats);
            Assert.Equal(0u, md.MediaFormats.ElementAt(0));
            Assert.Single(md.Attributes);
            Assert.Equal("control", md.Attributes.ElementAt(0).Name);
            Assert.Equal("rtsp://example.com/onvif_camera/audio", md.Attributes.ElementAt(0).Value);

            // Validate video media description
            md = sessiond.MediaDescriptions.ElementAt(1);
            Assert.Equal(MediaType.VIDEO, md.Media);
            Assert.Equal(0u, md.Port);
            Assert.Equal(TransportProtocol.RTP_AVP, md.Protocol);
            Assert.Single(md.MediaFormats);
            Assert.Equal(26u, md.MediaFormats.ElementAt(0));
            Assert.Single(md.Attributes);
            Assert.Equal("control", md.Attributes.ElementAt(0).Name);
            Assert.Equal("rtsp://example.com/onvif_camera/video", md.Attributes.ElementAt(0).Value);

            // Validate metadata media description
            md = sessiond.MediaDescriptions.ElementAt(2);
            Assert.Equal(MediaType.APPLICATION, md.Media);
            Assert.Equal(0u, md.Port);
            Assert.Equal(TransportProtocol.RTP_AVP, md.Protocol);
            Assert.Single(md.MediaFormats);
            Assert.Equal(107u, md.MediaFormats.ElementAt(0));
            Assert.Equal(3, md.Attributes.Count);
            Assert.Equal("control", md.Attributes.ElementAt(0).Name);
            Assert.Equal("rtsp://example.com/onvif_camera/metadata", md.Attributes.ElementAt(0).Value);
            Assert.Equal("recvonly", md.Attributes.ElementAt(1).Name);
            Assert.Equal("rtpmap", md.Attributes.ElementAt(2).Name);
            Assert.Equal("107 vnd.onvif.metadata/90000", md.Attributes.ElementAt(2).Value);
        }
    }
}
