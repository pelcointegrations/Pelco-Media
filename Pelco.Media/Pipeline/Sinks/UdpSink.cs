using NLog;
using System;
using System.Net;
using System.Net.Sockets;

namespace Pelco.Media.Pipeline.Sinks
{
    /// <summary>
    /// A media pipeline udp sink.  All receieved byte buffers are sent out
    /// to the configured udp socket.
    /// </summary>
    public class UdpSink : ISink
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private Socket _socket;
        private IPEndPoint _target;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="target">The target endpoint to send data to.</param>
        public UdpSink(IPEndPoint target)
        {
            _target = target;

            _socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        /// <summary>
        /// Stops and disposes the underlying socket.
        /// </summary>
        public void Stop()
        {
            _socket.Close();
            _socket.Dispose();
        }

        /// <summary>
        /// Writes a received buffer out over the underlying udp socket.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public bool WriteBuffer(ByteBuffer buffer)
        {
            if (buffer.RemainingBytes <= 0)
            {
                LOG.Warn("Unable to write empty buffer");

                return true;
            }

            try
            {
                _socket.SendTo(buffer.Raw, buffer.StartIndex, buffer.Length, SocketFlags.None, _target);
            }
            catch (ObjectDisposedException)
            {
                // ignore the sink was just stopped.
            }
            catch (Exception e)
            {
                LOG.Error(e, "Caught exception while attempting to send data");
            }

            return true;
        }
    }
}
