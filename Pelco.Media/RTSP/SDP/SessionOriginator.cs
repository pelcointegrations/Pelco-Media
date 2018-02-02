//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Pelco.Media.RTSP.SDP
{
    public class SessionOriginator
    {
        private static readonly Regex Regex = new Regex(@"^o\s*=\s*(.+)\s+(\d+)\s+(\d+)\s+(IN)\s+(IP4|IP6)\s+(.+)", RegexOptions.Compiled);

        public string Username { get; private set; }

        public long SessionId { get; private set; }

        public long SessionVersion { get; private set; }

        public NetworkType NetType { get; private set; }

        public AddressType AddrType { get; private set; }

        public string UnicastAddress { get; private set; }

        public override string ToString()
        {
            return new StringBuilder("o=").Append(Username)
                                          .Append(' ')
                                          .Append(SessionId)
                                          .Append(' ')
                                          .Append(SessionVersion)
                                          .Append(' ')
                                          .Append(NetType)
                                          .Append(' ')
                                          .Append(AddrType)
                                          .Append(' ')
                                          .Append(UnicastAddress)
                                          .ToString();
        }

        public static SessionOriginator Parse(string line)
        {
            var match = Regex.Match(line);

            if (!match.Success)
            {
                throw new SdpParseException($"Unable to parse origin '{line}'");
            }

            return CreateBuilder().Username(match.Groups[1].Value)
                                  .SessionId(long.Parse(match.Groups[2].Value))
                                  .SessionVersion(long.Parse(match.Groups[3].Value))
                                  .NetType(NetworkType.IN)
                                  .AddrType((AddressType)Enum.Parse(typeof(AddressType), match.Groups[5].Value))
                                  .UnicastAddress(match.Groups[6].Value)
                                  .Build();
            
        }

        public static Builder CreateBuilder()
        {
            return new Builder();
        }

        public class Builder
        {
            private long _sessionId;
            private string _username;
            private long _sessionVersion;
            private string _unicastAddress;
            private NetworkType _netType;
            private AddressType _addrType;

            public Builder()
            {
                Clear();
            }

            public Builder Clear()
            {
                _username = null;
                _sessionId = 0;
                _sessionVersion = 0;
                _unicastAddress = null;
                _netType = NetworkType.UNKNOWN;
                _addrType = AddressType.UNKNOWN;

                return this;
            }

            public Builder Username(string username)
            {
                _username = username;

                return this;
            }

            public Builder SessionId(long id)
            {
                _sessionId = id;

                return this;
            }

            public Builder SessionVersion(long version)
            {
                _sessionVersion = version;

                return this;
            }

            public Builder UnicastAddress(string addr)
            {
                _unicastAddress = addr;

                return this;
            }

            public Builder NetType(NetworkType type)
            {
                _netType = type;

                return this;
            }
            
            public Builder AddrType(AddressType type)
            {
                _addrType = type;

                return this; 
            }

            public SessionOriginator Build()
            {
                return new SessionOriginator()
                {
                    Username = _username,
                    SessionId = _sessionId,
                    SessionVersion = _sessionVersion,
                    UnicastAddress = _unicastAddress,
                    NetType = _netType,
                    AddrType = _addrType
                };
            }
        }
    }
}
