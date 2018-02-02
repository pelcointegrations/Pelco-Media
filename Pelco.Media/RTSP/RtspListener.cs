//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using NLog;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Pelco.Media.RTSP
{
    /// <summary>
    /// Callback delegate for handling <see cref="RtspChunk"/>s.
    /// Rtsp chunks are either interleaved data or Rtsp request/responses.
    /// </summary>
    /// <param name="chunk">The chunk to handle</param>
    public delegate void RtspChunkHandler(RtspChunk chunk);

    /// <summary>
    /// RTSP Listener.  A listener is used to listen for requests/responses
    /// from an RTSP transport.
    /// </summary>
    public class RtspListener
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private readonly object StartedLock = new object();

        private bool _started;
        private RtspMessageDecoder _decoder;
        private IRtspConnection _connection;
        private RtspChunkHandler _chunkHandler;

        public RtspListener(IRtspConnection connection, RtspChunkHandler handler)
        {
            _connection = connection ?? throw new ArgumentNullException("Connection cannot be null");
            _chunkHandler = handler ?? throw new ArgumentNullException("Handler cannot be null");
            _decoder = new RtspMessageDecoder(_connection.Endpoint);
        }

        /// <summary>
        /// Get's the underlying connection's IP endpoint.
        /// </summary>
        public IPEndPoint Endpoint
        {
            get
            {
                return _connection.Endpoint;
            }
        }

        /// <summary>
        /// Strarts the RTSP listener.  This will initialize the listener to receive
        /// requests.
        /// </summary>
        public void Start()
        {
            lock (StartedLock)
            {
                if (_started)
                {
                    return;
                }

                Task.Run(() => ListenForRequests());

                _started = true;
            }
        }

        /// <summary>
        /// Cloes the RTSP listener.
        /// </summary>
        /// <param name="closeConnection">Flag indicating if the connection should be closed as well.</param>
        public void Stop(bool closeConnection = true)
        {
            if (closeConnection && _connection.IsConnected)
            {
                _connection.Close();
            }

        }

        public void Reconnect()
        {
            if (_connection.IsConnected)
            {
                // Nothing to do.
                return;
            }

            LOG.Info($"Reconnecting RtspConnection for '{_connection.Endpoint}'");

            _connection.Reconnect();

            Task.Run(() => ListenForRequests());
        }

        public void SendResponse(RtspResponse response)
        {
            if (!_connection.WriteMessage(response))
            {
                LOG.Error($"Failed to write response to client at {_connection.RemoteAddress} \n{response}");
            }
        }

        public bool Write(byte[] data, int offset, int count)
        {
            try
            {
                _connection.Write(data, offset, count);
                return true;
            }
            catch (Exception e)
            {
                LOG.Error($"Failed to write data to connection '{_connection.Endpoint}', reason: {e.Message}");
            }

            return false;
        }

        private void ListenForRequests()
        {
            byte[] readBuffer = new byte[4096];

            LOG.Info($"Starting Rtsp listening thread '{Thread.CurrentThread.ManagedThreadId}' for '{_connection.Endpoint}'");

            while (_connection.IsConnected)
            {
                try
                {
                    if (_connection.CanRead)
                    {
                        int bytesRead = _connection.Read(readBuffer, 0, readBuffer.Length);

                        if (bytesRead > 0)
                        {
                            using (var ms = new MemoryStream(readBuffer, 0, bytesRead))
                            {
                                HandleRequest(ms);
                            }
                        }
                    }
                }
                catch (ObjectDisposedException e)
                {
                    LOG.Warn($"Rtsp listening socket disposed during read, msg: {e.Message}");
                }
                catch (Exception e)
                {
                    HandleException(e);
                }
            }

            LOG.Info($"Exiting RTSP Listening thread '{Thread.CurrentThread.ManagedThreadId}' for '{_connection.Endpoint}'");
        }

        private void HandleRequest(MemoryStream stream)
        {
            // Keep iterating until we have consumed the full buffer.
            while (stream.Position < stream.Length)
            {
                try
                {
                    RtspChunk chunk = null;
                    if (_decoder.Decode(stream, out chunk))
                    {
                        _chunkHandler?.Invoke(chunk);
                    }
                }
                catch (Exception e)
                {
                    if (e is EndOfStreamException)
                    {
                        // We reached the end of the stream.
                        return;
                    }

                    LOG.Error(e, $"Error occured while decoding buffer, reason: {e.Message}");
                }
            }
        }

        private void HandleException(Exception e)
        {
            if (e.InnerException != null && e.InnerException is SocketException)
            {
                var sockEx = e.InnerException as SocketException;
                if (sockEx.SocketErrorCode == SocketError.Interrupted)
                {
                    LOG.Warn("Socket interrupted during read, possibly the socket was closed");
                    return;
                }
            }

            LOG.Error($"Received exception while reading or decoding RTSP msg, reason: {e.Message}");
        }
    }
}
