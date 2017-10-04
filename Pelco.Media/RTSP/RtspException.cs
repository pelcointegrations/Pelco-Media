using System;

namespace Pelco.PDK.Media.RTSP
{
    public class RtspException : Exception
    {
        public RtspException() : base()
        {

        }

        public RtspException(string msg) : base(msg)
        {

        }

        public RtspException(string msg, Exception cause) : base(msg, cause)
        {

        }
    }
}
