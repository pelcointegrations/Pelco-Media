using System;

namespace Pelco.PDK.Media.RTSP
{
    class RtspMessageParseException : Exception
    {
        public RtspMessageParseException(string msg) : base(msg)
        {

        }

        public RtspMessageParseException(string msg, Exception cause) : base(msg, cause)
        {

        }
    }
}
