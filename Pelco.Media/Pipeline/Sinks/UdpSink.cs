using NLog;
using System;
using System.Net;
using System.Net.Sockets;

namespace Pelco.PDK.Media.Pipeline.Sinks
{
    public class UdpSink : ISink
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private Socket _socket;
        private IPEndPoint _target;

        public UdpSink(IPEndPoint target)
        {
            _target = target;

            _socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        public void Stop()
        {
            _socket.Dispose();
        }

        public bool WriteBuffer(ByteBuffer buffer)
        {
            if (buffer.RemainingBytes <= 0)
            {
                LOG.Warn("Unable to write empty buffer");

                return true;
            }

            try
            {
                var e = new SocketAsyncEventArgs();
                e.RemoteEndPoint = _target;
                e.SetBuffer(buffer.Raw, buffer.StartIndex, buffer.Length);

                _socket.SendAsync(e);
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
