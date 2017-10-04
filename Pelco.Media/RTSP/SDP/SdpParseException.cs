using System;

namespace Pelco.PDK.Media.RTSP.SDP
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
