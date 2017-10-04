using Pelco.PDK.Media.RTSP.SDP;
using System.Text.RegularExpressions;

namespace Pelco.PDK.Media.RTSP
{
    public class SdpRtpMap
    {
        private static Regex REGEX = new Regex(@"^rtpmap\s*:\s*(\d+)\s+(\w+)\s*/\s*(\d+)(/(.+))?", RegexOptions.Compiled);

        public ushort PayloadType { get; private set; }

        public string EncodingName { get; private set; }

        public uint ClockRate { get; private set; }

        public string EncodingParameters { get; private set; }

        public static SdpRtpMap Parse(string str)
        {
            var match = REGEX.Match(str);

            if (!match.Success)
            {
                throw new SdpParseException($"Unable to parse malformed rtpmap attriute '{str}'");
            }

            return null;
        }
    }
}
