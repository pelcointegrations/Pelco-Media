using System;

namespace Pelco.Media.RTSP.SDP
{
    public class SdpParseException : Exception
    {
        public SdpParseException(string msg) : base(msg)
        {
        }

        public SdpParseException(string msg, Exception cause) : base(msg, cause)
        {
        }
    }
}
