using Pelco.PDK.Media.RTSP.SDP;
using System;
using System.Text.RegularExpressions;

namespace Pelco.PDK.Media.RTSP.SDP
{
    public class SdpRtpMap
    {
        private static Regex REGEX = new Regex(@"^\s*(\d+)\s+(.+)\s*/\s*(\d+)(\s*/\s*(.+))?", RegexOptions.Compiled);

        internal SdpRtpMap()
        {

        }

        internal SdpRtpMap(ushort payloadType,
                           string encodingName,
                           uint clockRate,
                           string encodingParams = null)
        {
            PayloadType = payloadType;
            EncodingName = encodingName;
            ClockRate = clockRate;
            EncodingParameters = encodingParams;
        }

        #region Properties

        public ushort PayloadType { get; private set; }

        public string EncodingName { get; private set; }

        public uint ClockRate { get; private set; }

        public string EncodingParameters { get; private set; }

        #endregion

        public static SdpRtpMap Parse(string str)
        {
            var match = REGEX.Match(str);

            if (!match.Success)
            {
                throw new SdpParseException($"Unable to parse malformed rtpmap attriute '{str}'");
            }

            try
            {
                var builder = SdpRtpMap.CreateBuilder()
                                       .PayloadType(ushort.Parse(match.Groups[1].Value))
                                       .EncodingName(match.Groups[2].Value.Trim())
                                       .ClockRate(uint.Parse(match.Groups[3].Value));

                if (match.Groups.Count == 6)
                {
                    builder.EncodingParameters(match.Groups[5].Value.Trim());
                }

                return builder.Build();
            }
            catch (Exception e)
            {
                throw new SdpParseException($"Unable to parse rtpmap attribute '{str}'", e);
            }
        }

        public static Builder CreateBuilder()
        {
            return new Builder();
        }

        public sealed class Builder
        {
            private ushort _payloadType;
            private string _encodingName;
            private uint _clockRate;
            private string _encodingParams;

            public Builder()
            {

            }

            public Builder Clear()
            {
                _payloadType = 0;
                _encodingName = string.Empty;
                _clockRate = 0;
                _encodingParams = string.Empty;

                return this;
            }

            public Builder PayloadType(ushort payloadType)
            {
                _payloadType = payloadType;

                return this;
            }

            public Builder EncodingName(string name)
            {
                _encodingName = name;

                return this;
            }

            public Builder ClockRate(uint clockRate)
            {
                _clockRate = clockRate;

                return this;
            }

            public Builder EncodingParameters(string parameters)
            {
                _encodingParams = parameters;

                return this;
            }

            public SdpRtpMap Build()
            {
                return new SdpRtpMap()
                {
                    ClockRate = _clockRate,
                    PayloadType = _payloadType,
                    EncodingName = _encodingName,
                    EncodingParameters = _encodingParams,
                };
            }
        }
    }
}
