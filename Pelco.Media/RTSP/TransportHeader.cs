//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Pelco.Media.RTSP
{
    public class TransportHeader
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        internal TransportHeader(TransportType type,
                                 string ssrc,
                                 int ttl,
                                 string source,
                                 string destination,
                                 PortPair clientPorts,
                                 PortPair serverPorts,
                                 PortPair multicastPorts,
                                 PortPair interleavedChannels,
                                 ImmutableList<string> extras)
        {
            Type = type;
            SSRC = ssrc;
            TTL = ttl;
            Source = source;
            Destination = destination;
            ClientPorts = clientPorts;
            ServerPorts = serverPorts;
            MulticastPorts = multicastPorts;
            InterleavedChannels = interleavedChannels;
            Extras = extras;
        }

        #region Properties

        public TransportType Type { get; private set; }

        public string Source { get; private set; }

        public string Destination { get; private set; }

        public int TTL { get; private set; }

        public PortPair MulticastPorts { get; private set; }

        public PortPair ServerPorts { get; private set; }

        public PortPair ClientPorts { get; private set; }

        public PortPair InterleavedChannels { get; private set; }

        public ImmutableList<string> Extras { get; private set; }

        public string SSRC { get; private set; }

        #endregion

        public override string ToString()
        {
            IList<string> parts = new List<string>();

            switch (Type)
            {
                case TransportType.UdpUnicast:
                    {
                        parts.Add("RTP/AVP");
                        parts.Add("unicast");

                        if (ServerPorts != null && ServerPorts.IsSet)
                        {
                            parts.Add($"server_port={ServerPorts.RtpPort}-{ServerPorts.RtcpPort}");
                        }

                        if (ClientPorts != null && ClientPorts.IsSet)
                        {
                            parts.Add($"client_port={ClientPorts.RtpPort}-{ClientPorts.RtcpPort}");
                        }

                        if (!string.IsNullOrEmpty(SSRC))
                        {
                            parts.Add($"ssrc={SSRC}");
                        }
                    }
                    break;

                case TransportType.UdpMulticast:
                    {
                        parts.Add("RTP/AVP");
                        parts.Add("multicast");

                        if (MulticastPorts != null && MulticastPorts.IsSet)
                        {
                            parts.Add($"port={MulticastPorts.RtpPort}-{MulticastPorts.RtcpPort}");
                            parts.Add($"ttl={TTL}");
                        }
                    }
                    break;

                case TransportType.RtspInterleaved:
                    {
                        parts.Add("RTP/AVP/TCP");

                        if (InterleavedChannels != null && InterleavedChannels.IsSet)
                        {
                            parts.Add($"interleaved={InterleavedChannels.RtpPort}-{InterleavedChannels.RtcpPort}");
                        }
                    }
                    break;

                default:
                    break;
            }

            return string.Join(";", parts);
        }

        public static TransportHeader Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Unable to parse null or empty Transport header value");
            }

            LOG.Debug($"Attempting to parsing RTSP Transport header: {value}");

            TransportType discoveredType = TransportType.Unknown;
            TransportHeader.Builder builder = TransportHeader.CreateBuilder();

            string[] parts = value.Split(new char[] { ';' });
            foreach (var part in parts)
            {
                string[] keyValue = part.Split(new char[] { '=' }, 2);
                switch (keyValue.Length)
                {
                    case 1:
                        {
                            if (keyValue[0] == "RTP/AVP/TCP")
                            {
                                discoveredType = TransportType.RtspInterleaved;
                                builder.Type(discoveredType);
                            }
                            else if (keyValue[0] == "unicast" && discoveredType != TransportType.RtspInterleaved)
                            {
                                builder.Type(TransportType.UdpUnicast);
                            }
                            else if (keyValue[0] == "multicast" && discoveredType != TransportType.RtspInterleaved)
                            {
                                builder.Type(TransportType.UdpMulticast);
                            }
                            else if ((keyValue[0] != "multicast") || parts[0] == "RTP/AVP")
                            {
                                builder.AddExtra(part);
                            }
                        }
                        break;

                    case 2:
                        {
                            if (keyValue[0] == "interleaved")
                            {
                                builder.InterleavedChannels(ParsePortPair(keyValue[1]));
                            }
                            else if (keyValue[0] == "server_port")
                            {
                                builder.ServerPorts(ParsePortPair(keyValue[1]));
                            }
                            else if (keyValue[0] == "client_port")
                            {
                                builder.ClientPorts(ParsePortPair(keyValue[1]));
                            }
                            else if (keyValue[0] == "port")
                            {
                                builder.MulticastPorts(ParsePortPair(keyValue[1]));
                            }
                            else if (keyValue[0] == "ttl")
                            {
                                builder.TTL(int.Parse(keyValue[1]));
                            }
                            else if (keyValue[0] == "destination")
                            {
                                builder.Destination(keyValue[1].Trim());
                            }
                            else if (keyValue[0] == "source")
                            {
                                builder.Source(keyValue[1].Trim());
                            }
                            else if (keyValue[0] == "ssrc")
                            {
                                builder.SSRC(keyValue[1].Trim());
                            }
                            else
                            {
                                builder.AddExtra(part);
                            }
                        }
                        break;

                    default:
                        {
                            builder.AddExtra(part);
                            break;
                        }
                }
            }

            return builder.Build();
        }

        public static Builder CreateBuilder()
        {
            return new Builder();
        }

        private static PortPair ParsePortPair(string str)
        {
            var ports = str.Split(new char[] { '-' });
            return new PortPair()
            {
                RtpPort = int.Parse(ports[0]),
                RtcpPort = int.Parse(ports[1])
            };
        }

        public class Builder
        {
            private int _ttl;
            private string _ssrc;
            private string _source;
            private TransportType _type;
            private string _destination;
            private PortPair _clientPorts;
            private PortPair _serverPorts;
            private PortPair _multicastPorts;
            private PortPair _interleavedChannels;
            private ImmutableList<string>.Builder _extras;

            public Builder()
            {
                Clear();
            }

            public Builder Clear()
            {
                _ttl = 0;
                _type = TransportType.Unknown;
                _ssrc = null;
                _source = null;
                _destination = null;
                _clientPorts = null;
                _serverPorts = null;
                _multicastPorts = null;
                _interleavedChannels = null;
                _extras = ImmutableList.CreateBuilder<string>();

                return this;
            }

            public Builder Type(TransportType type)
            {
                if (type == TransportType.Unknown)
                {
                    throw new ArgumentException("Cannot set type to unknown");
                }

                _type = type;

                return this;
            }

            public Builder TTL(int ttl)
            {
                if (ttl <= 0)
                {
                    throw new ArgumentException("TTL must be >= 0");
                }

                _ttl = ttl;

                return this;
            }

            public Builder SSRC(string ssrc)
            {
                if (string.IsNullOrEmpty(ssrc))
                {
                    throw new ArgumentException("SSRC cannot be empty or null");
                }

                _ssrc = ssrc;

                return this;
            }

            public Builder Source(string source)
            {
                if (string.IsNullOrEmpty(source))
                {
                    throw new ArgumentException("Soruce cannot be empty or null");
                }

                _source = source;

                return this;
            }

            public Builder Destination(string dest)
            {
                if (string.IsNullOrEmpty(dest))
                {
                    throw new ArgumentException("Destination cannot be empty or null");
                }

                _destination = dest;

                return this;
            }

            public Builder ClientPorts(int rtp, int rtcp)
            {
                return ClientPorts(new PortPair { RtpPort = rtp, RtcpPort = rtcp });
            }

            public Builder ClientPorts(PortPair pair)
            {
                _clientPorts = pair;

                return this;
            }

            public Builder ServerPorts(int rtp, int rtcp)
            {
               return ServerPorts(new PortPair { RtpPort = rtp, RtcpPort = rtcp });
            }

            public Builder ServerPorts(PortPair pair)
            {
                _serverPorts = pair;

                return this;
            }

            public Builder MulticastPorts(int rtp, int rtcp)
            {
                return MulticastPorts(new PortPair { RtpPort = rtp, RtcpPort = rtcp });
            }

            public Builder MulticastPorts(PortPair pair)
            {
                _multicastPorts = pair;

                return this;
            }

            public Builder InterleavedChannels(int rtp, int rtcp)
            {
                return InterleavedChannels(new PortPair { RtpPort = rtp, RtcpPort = rtcp });
            }

            public Builder InterleavedChannels(PortPair pair)
            {
                _interleavedChannels = pair;

                return this;
            }

            public Builder AddExtra(string extra)
            {
                _extras.Add(extra);

                return this;
            }

            public TransportHeader Build()
            {
                return new TransportHeader(_type,
                                               _ssrc,
                                               _ttl,
                                               _source,
                                               _destination,
                                               _clientPorts,
                                               _serverPorts,
                                               _multicastPorts,
                                               _interleavedChannels,
                                               _extras.ToImmutable());
            }
        }
    }

    public class PortPair
    {
        public PortPair()
        {
            RtpPort = 0;
            RtcpPort = 0;
        }

        public PortPair(int rtpPort, int rtcpPort)
        {
            RtpPort = rtpPort;
            RtcpPort = rtcpPort;
        }

        public int RtpPort { get; internal set; }

        public int RtcpPort { get; internal set; }

        public bool IsSet
        {
            get
            {
                return RtpPort != 0 || RtcpPort != 0;
            }
        }
    }
}
