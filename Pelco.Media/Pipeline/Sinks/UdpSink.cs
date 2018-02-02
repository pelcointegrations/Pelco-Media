//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
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
        private ISource _upstreamLink;

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

        public ISource UpstreamLink
        {
            get
            {
                return _upstreamLink;
            }

            set
            {
                _upstreamLink = value;
            }
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

        public virtual void PushEvent(MediaEvent e)
        {
            UpstreamLink?.OnMediaEvent(e);
        }
    }
}
