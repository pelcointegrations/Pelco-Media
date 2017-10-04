using System;

namespace Pelco.PDK.Media.RTSP
{
    public class RtspMessageEventArgs : EventArgs
    {
        public RtspMessageEventArgs(RtspMessage message)
        {
            Message = message;
        }

        public RtspMessage Message { get; private set; }
    }
}
