using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pelco.Media.RTSP.SDP
{
    public enum TransportProtocol
    {
        UDP,
        RTP_AVP,
        RTP_SAVP,
        UNKNOWN,
    }

    public enum MediaType
    {
        AUDIO,
        VIDEO,
        TEXT,
        APPLICATION,
        UNKNOWN,
    }

    public class MediaDescription
    {
        private static readonly string CRLF = "\r\n";
        private static readonly Regex REGEX = new Regex(@"^m\s*=\s*(audio|video|text|application|message)\s+(\d+)\s+(udp|RTP\s*/\s*AVP|RTP\s*/\s*SAVP)\s+(.+)", RegexOptions.Compiled);

        public MediaDescription()
        {
            Bandwidths = new List<BandwidthInfo>();
            Attributes = new List<Attribute>();
            MediaFormats = new List<uint>();
        }

        #region Properties

        public MediaType Media { get; set; }

        public uint Port { get; set; }

        public TransportProtocol Protocol { get; set; }

        public string MediaTitle { get; set; }

        public ConnectionInfo Connection { get; set; }

        public List<BandwidthInfo> Bandwidths { get; private set; }

        public List<uint> MediaFormats { get; private set; }

        public List<Attribute> Attributes { get; private set; }

        #endregion

        /// <summary>
        /// Returns the assocciated rtpmap attributes as a list of <see cref="SdpRtpMap"/>
        /// instances.
        /// </summary>
        /// <exception cref="SdpParseException">If a rtpmap attribuet is malformed.</exception>
        /// <returns></returns>
        public ImmutableList<SdpRtpMap> GetRtpMaps()
        {
            var builder = ImmutableList.CreateBuilder<SdpRtpMap>();

            Attributes.Where(a => "rtpmap" == a.Name).ToList().ForEach(a =>
            {
                builder.Add(SdpRtpMap.Parse(a.Value));
            });

            return builder.ToImmutable();
        }

        /// <summary>
        /// <see cref="object.ToString"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder("m=").Append(Media.ToString().ToLower())
                                            .Append(' ')
                                            .Append(Port)
                                            .Append(' ')
                                            .Append(FromTransportProtocol(Protocol));

            MediaFormats.ForEach(fmt =>
            {
                sb.Append(' ').Append(fmt);
            });
            sb.Append(CRLF);

            if (MediaTitle != null)
            {
                sb.Append("i=").Append(MediaTitle).Append(CRLF);
            }

            if (Connection != null)
            {
                sb.Append(Connection.ToString()).Append(CRLF);
            }

            Bandwidths.ForEach(bw =>
            {
                sb.Append(bw.ToString()).Append(CRLF);
            });

            Attributes.ForEach(attr =>
            {
                sb.Append(attr.ToString()).Append(CRLF);
            });

            return sb.ToString();
        }

        public static Builder CreateBuilder()
        {
            return new Builder();
        }

        public static MediaDescription Parse(string line)
        {
            var match = REGEX.Match(line);

            if (!match.Success)
            {
                throw new SdpParseException($"Unable to parse malformed Media Description '{line}'");
            }

            uint port;
            if (!uint.TryParse(match.Groups[2].Value.Trim(), out port))
            {
                throw new SdpParseException($"Unable to parse Media Description port '{match.Groups[2].Value}'");
            }

            var md = new MediaDescription()
            {
                Media = (MediaType)Enum.Parse(typeof(MediaType), match.Groups[1].Value.Trim().ToUpper()),
                Port = port,
                Protocol = ToTransportProtocol(match.Groups[3].Value.Trim())
            };

            ParseMediaFormats(match.Groups[4].Value, md);

            return md;
        }

        private string FromTransportProtocol(TransportProtocol proto)
        {
            switch (proto)
            {
                case TransportProtocol.UDP     : return "udp";
                case TransportProtocol.RTP_AVP : return "RTP/AVP";
                case TransportProtocol.RTP_SAVP: return "RTP/SAVP";
                default                        : return "unknown";
            }
        }

        private static TransportProtocol ToTransportProtocol(string str)
        {
            switch (Regex.Replace(str, @"\s+", ""))
            {
                case "udp"     : return TransportProtocol.UDP;
                case "RTP/AVP" : return TransportProtocol.RTP_AVP;
                case "RTP/SAVP": return TransportProtocol.RTP_SAVP;
                default        : return TransportProtocol.UNKNOWN;
            }
        }

        private static void ParseMediaFormats(string str, MediaDescription md)
        {
            str = str.Trim();

            Regex.Split(str, @"\s+").Where(s => s != string.Empty).ToList().ForEach(fmt =>
            {
                uint format;
                if (!uint.TryParse(fmt.Trim(), out format))
                {
                    throw new SdpParseException($"Unable to parse Media Description fmt '{fmt}'");
                }

                md.MediaFormats.Add(format);
            });
        }

        public sealed class Builder
        {
            private uint _port;
            private string _title;
            private MediaType _type;
            private List<uint> _fmts;
            private TransportProtocol _proto;
            private ConnectionInfo _connection;
            private List<Attribute> _attributes;
            private List<BandwidthInfo> _bandwidths;

            public Builder()
            {
                _fmts = new List<uint>();
                _attributes = new List<Attribute>();
                _bandwidths = new List<BandwidthInfo>();

                Clear();
            }

            public Builder Clear()
            {
                _port = 0;
                _connection = null;
                _type = RTSP.SDP.MediaType.UNKNOWN;
                _proto = TransportProtocol.UNKNOWN;

                _fmts.Clear();
                _attributes.Clear();
                _bandwidths.Clear();

                return this;
            }

            public Builder AddFormat(uint fmt)
            {
                _fmts.Add(fmt);

                return this;
            }

            public Builder AddBandwidth(BandwidthInfo bw)
            {
                _bandwidths.Add(bw);

                return this;
            }

            public Builder AddAttribute(Attribute attribute)
            {
                _attributes.Add(attribute);

                return this;
            }

            public Builder Port(uint port)
            {
                _port = port;

                return this;
            }

            public Builder MediaType(MediaType type)
            {
                _type = type;

                return this;
            }

            public Builder Protocol(TransportProtocol proto)
            {
                _proto = proto;

                return this;
            }

            public Builder Title(string title)
            {
                _title = title;

                return this;
            }

            public Builder Connection(ConnectionInfo conn)
            {
                _connection = conn;

                return this;
            }

            public MediaDescription Build()
            {
                var md = new MediaDescription()
                {
                    Media = _type,
                    Port = _port,
                    Protocol = _proto,
                    MediaTitle = _title,
                    Connection = _connection,
                };

                md.MediaFormats.AddRange(_fmts);
                md.Bandwidths.AddRange(_bandwidths);
                md.Attributes.AddRange(_attributes);

                return md;
            }
        }
    }
}
