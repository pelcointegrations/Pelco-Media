using Pelco.Media.Pipeline;
using System;
using System.Net;

namespace Pelco.Media.RTSP
{
    /// <summary>
    /// Holds contextual information related to an RTSP request.
    /// </summary>
    public class RequestContext
    {
        private RtspListener _listener;

        internal RequestContext(RtspListener listener)
        {
            _listener = listener ?? throw new ArgumentNullException("Listener cannot be null");
        }

        /// <summary>
        /// Gets the requesting IP endpoint.
        /// </summary>
        public IPEndPoint Endpoint
        {
            get
            {
                return _listener.Endpoint;
            }
        }

        /// <summary>
        /// Write data to the underlying RTSP connectionn.
        /// </summary>
        /// <param name="data">The data to write to RTCP connection</param>
        /// <returns>true if successful, false otherwise</returns>
        public bool Write(byte[] data)
        {
            return _listener.Write(data);
        }

        /// <summary>
        /// Write buffer to the underlying RTSP connectionn.
        /// </summary>
        /// <param name="data">The buffer to write to RTCP connection</param>
        /// <returns>true if successful, false otherwise</returns>
        public bool Write(ByteBuffer buffer)
        {
            return _listener.Write(buffer.Raw);
        }
    }
}
 