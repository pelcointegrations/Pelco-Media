using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Pelco.PDK.Media.RTSP.SDP
{
    public class ConnectionInfo
    {
        private static readonly Regex REGEX = new Regex(@"^c\s*=\s*IN\s+(IP4|IP6)\s+(.+)", RegexOptions.Compiled);

        #region Properties

        public NetworkType NetType { get; set; }

        public AddressType AddrType { get; set; }

        public string Address { get; set; }

        public short TTL { get; set; }

        #endregion

        public override string ToString()
        {
            var sb = new StringBuilder("c=").Append(NetworkType.IN)
                                          .Append(' ')
                                          .Append(AddrType)
                                          .Append(' ')
                                          .Append(Address);
           
            if (TTL > 0)
            {
                sb.Append('/').Append(TTL);
            }

            return sb.ToString();
        }

        public static ConnectionInfo Parse(string line)
        {
            var match = REGEX.Match(line);

            if (!match.Success)
            {
                throw new SdpParseException($"Unable to parse malformed Connection Data '{line}'");
            }

            var builder = ConnectionInfo.CreateBuilder()
                                        .AddrType((AddressType)Enum.Parse(typeof(AddressType), match.Groups[1].Value));

            var connectionAddr = match.Groups[2].Value;
            int index = connectionAddr.LastIndexOf('/');
            if (index == -1)
            {
                return builder.Address(connectionAddr.Trim()).Build();
            }

            builder.Address(connectionAddr.Substring(0, index).Trim());

            short ttl;
            var str = connectionAddr.Substring(index + 1).Trim();
            if (!short.TryParse(str, out ttl))
            {
                throw new SdpParseException($"Unable to parse Connection Info's address TTL '{str}'");
            }

            return builder.TTL(ttl).Build();
        }

       public static Builder CreateBuilder()
        {
            return new Builder();
        }

        public sealed class Builder
        {
            private short _ttl;
            private string _address;
            private NetworkType _netType;
            private AddressType _addrType;

            public Builder()
            {
                Clear();
            }

            public Builder Clear()
            {
                _ttl = 0;
                _address = string.Empty;
                _netType = NetworkType.UNKNOWN;
                _addrType = AddressType.UNKNOWN;

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

            public Builder Address(string addr)
            {
                _address = addr;

                return this;
            }

            public Builder TTL(short ttl)
            {
                _ttl = ttl;

                return this;
            }

            public ConnectionInfo Build()
            {
                return new ConnectionInfo()
                {
                    NetType = _netType,
                    AddrType = _addrType,
                    Address = _address,
                    TTL = _ttl
                };
            }
        }
    }

    /// <summary>
    /// SDP Network type as defined in RFC 4566 seciont 5.7
    /// </summary>
    public enum NetworkType
    {
        /// <summary>
        /// Internet network type
        /// </summary>
        IN,

        /// <summary>
        ///  Unknown network type.
        /// </summary>
        UNKNOWN,
    }

    /// <summary>
    /// SDP Address type as defined in RFC 4566 seciont 5.7
    /// </summary>
    public enum AddressType
    {
        /// <summary>
        /// IPv4 address type
        /// </summary>
        IP4,

        /// <summary>
        /// IPv6 address type.
        /// </summary>
        IP6,

        /// <summary>
        /// Unknown address type.
        /// </summary>
        UNKNOWN,
    }
}
