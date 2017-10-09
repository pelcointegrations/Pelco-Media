using Pelco.Media.Common;
using Pelco.Media.RTSP.SDP;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;

namespace Pelco.Media.RTSP.Client
{
    public sealed class MediaTracks
    {
        private static readonly MimeType APPLICATION_SPOOFED = MimeType.CreateApplicationType("vnd.pelco.spoofed");

        public static ImmutableList<MediaTrack> FromSdp(SessionDescription sdp, Uri baseUri, MimeType filter = null)
        {
            var builder = ImmutableList.CreateBuilder<MediaTrack>();

            string sdpConnectionAddr = baseUri.Host;
            if (sdp.Connection != null && IsAddressSet(sdp.Connection.Address))
            {
                sdpConnectionAddr = sdp.Connection.Address;
            }

            sdp.MediaDescriptions.ForEach(md =>
            {
                Uri controlUri = baseUri;
                string type = md.Media.ToString().ToLower();
                string subType = "unknown";

                // Prefer the media description's connection address over the sdp connection address.
                var connectionAddr = md.Connection != null && !string.IsNullOrEmpty(md.Connection.Address)
                        ? md.Connection.Address
                        : sdpConnectionAddr;

                var rtmpmaps = md.GetRtpMaps();

                // Via spec there can be multiple types and rtpmaps, but
                // we will assume a single one since all cameras/encoders
                // appear to only provide one per Media Description.
                var rtpmap = rtmpmaps.IsEmpty ? FromPayloadType(md.MediaFormats[0]) : rtmpmaps.First();
                subType = rtpmap.EncodingName;

                var control = md.Attributes.Where(a => "control" == a.Name).DefaultIfEmpty(null).First();
                if (control != null)
                {
                    controlUri = ResolveUri(baseUri, new Uri(control.Value, UriKind.RelativeOrAbsolute));
                }

                var mimeType = MimeType.Create(type, rtpmap.EncodingName);

                if (mimeType.Is(APPLICATION_SPOOFED))
                {
                    // The MediaGateway and Vxpro us spoofing to determine the client's intentions of
                    // either playing live or recorded data.  Because of the current design they do not
                    // have a way to know the client's intentions until a play call is issued that contains
                    // and RTSP Range header with an absolute start-time.
                    //
                    // To handle metadata, in a pretty hacky way, a new media description is added to the SDP
                    // of type application/vnd.pelco.spoofed to indicate to a client that the data is bogus
                    // The client should continue processing up until the play call is made.  At this point the
                    // MediaGateway or VxPro will issues a redirect to the actual source.
                    //
                    // We will make a bogus metadata track to allow the client to continue processing.
                    builder.Add(MediaTrack.CreateBuilder()
                                          .Address(Dns.GetHostAddresses(baseUri.Host)[0])
                                          .Port(md.Port)
                                          .Type(mimeType)
                                          .Uri(controlUri)
                                          .Build());
                }
                else if ((  filter != null && mimeType.Is(filter)) || (filter == null))
                {
                    // If a filter is defined only return tracks that match the
                    // defined filter type; otherwise, return all tracks.

                    if (IsAddressSet(connectionAddr))
                    {
                        // If defined as 0.0.0.0 or ::0 then replace with the SDP connection address.
                        connectionAddr = sdpConnectionAddr;
                    }

                    var addrs = Dns.GetHostAddresses(connectionAddr);

                    builder.Add(MediaTrack.CreateBuilder()
                                          .Address(Dns.GetHostAddresses(connectionAddr)[0])
                                          .Port(md.Port)
                                          .RtpMap(rtpmap)
                                          .Type(mimeType)
                                          .Uri(controlUri)
                                          .Build());
                }
            });

            return builder.ToImmutable();
        }

        private static bool IsAddressSet(string addr)
        {
            return !string.IsNullOrEmpty(addr) && addr != "0.0.0.0" && addr != "::0";
        }

        private static Uri ResolveUri(Uri baseUri, Uri controlUri)
        {
            string seperator = "";
            string prefix = baseUri.ToString();
            string suffix = controlUri.ToString();

            // The RTSP spec indicates that a control URI of '*' is replaced with
            // the base.
            if ("*" == suffix)
            {
                return baseUri;
            }

            if (controlUri.IsAbsoluteUri)
            {
                prefix = "";
            }
            else if (!prefix.EndsWith("/") && (suffix.Length > 0) && (suffix[0] != '/'))
            {
                seperator = "/";
            }

            return new Uri($"{prefix}{seperator}{suffix}");
        }

        private static SdpRtpMap FromPayloadType(uint payloadType)
        {
            // Returns a RtpMap instance from the payload types defined
            // in https://tools.ietf.org/html/rfc3551 section 5.
            switch (payloadType)
            {
                case 0: return new SdpRtpMap(0, "PCMU", 8000, "1");
                case 3: return new SdpRtpMap(3, "GSM", 8000, "1");
                case 4: return new SdpRtpMap(4, "G723", 8000, "1");
                case 5: return new SdpRtpMap(5, "DVI4", 8000, "1");
                case 6: return new SdpRtpMap(6, "DVI4", 16000, "1");
                case 7: return new SdpRtpMap(7, "LPC", 8000, "1");
                case 8: return new SdpRtpMap(8, "PCMA", 8000, "1");
                case 9: return new SdpRtpMap(9, "G722", 8000, "1");
                case 10: return new SdpRtpMap(10, "L16", 44100, "2");
                case 11: return new SdpRtpMap(11, "L16", 44100, "1");
                case 12: return new SdpRtpMap(12, "QCELP", 8000, "1");
                case 13: return new SdpRtpMap(13, "CN", 8000, "1");
                case 14: return new SdpRtpMap(14, "MPA", 90000);
                case 15: return new SdpRtpMap(15, "G728", 8000, "1");
                case 16: return new SdpRtpMap(16, "DVI4", 11025, "1");
                case 17: return new SdpRtpMap(17, "DVI4", 22050, "1");
                case 18: return new SdpRtpMap(18, "G729", 8000, "1");
                case 25: return new SdpRtpMap(25, "CelB", 90000);
                case 26: return new SdpRtpMap(26, "JPEG", 90000);
                case 28: return new SdpRtpMap(28, "nv", 90000);
                case 31: return new SdpRtpMap(31, "H261", 90000);
                case 32: return new SdpRtpMap(32, "MPV", 90000);
                case 33: return new SdpRtpMap(33, "MP2T", 90000);
                case 34: return new SdpRtpMap(34, "H263", 90000);
                default: return new SdpRtpMap(199, "UNKNOWN", 0);
            }
        }
    }
}
