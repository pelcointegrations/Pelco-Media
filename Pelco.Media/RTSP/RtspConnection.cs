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
using System.Text;

namespace Pelco.Media.RTSP
{
    /// <summary>
    /// RTSP TCP transport.
    /// </summary>
    public class RtspConnection : IRtspConnection, IDisposable
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private readonly object WriteLock = new object();

        private Stream _stream;
        private IPEndPoint _endpoint;
        private TcpClient _tcpClient;

        /// <summary>
        /// Constructs a new <see cref="RtspConnection"/> instance.
        /// </summary>
        /// <param name="address">The IP address to bind to.</param>
        /// <param name="port">The port to bind to.</param>
        public RtspConnection(IPAddress address, int port)
        {
            if (address == null)
            {
                throw new ArgumentNullException("Address cannot be null");
            }

            LOG.Info($"Creating RTSP transport for {address}:{port}");

            _endpoint = new IPEndPoint(address, port);
            _tcpClient = new TcpClient(address.ToString(), port);
            _tcpClient.LingerState = new LingerOption(false, 5);

            _stream = _tcpClient.GetStream();
        }

        /// <summary>
        /// Constructs a new <see cref="RtspConnection"/> instance
        /// </summary>
        /// <param name="client"><see cref="TcpClient"/> instance to create the connection from</param>
        public RtspConnection(TcpClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("TcpClient is null");
            }

            _endpoint = client.Client.RemoteEndPoint as IPEndPoint;
            if (_endpoint == null)
            {
                throw new ArgumentException("TcpClient does not contain a remote endpoint");
            }

            LOG.Info($"Creating RTSP connection from existing TcpClient {_endpoint}");

            _tcpClient = client;
            _stream = _tcpClient.GetStream();
        }

        /// <summary>
        /// <see cref="IRtspConnection.IsConnected"/>
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _tcpClient.Connected;
            }
        }

        /// <summary>
        /// <see cref="IRtspConnection.Endpoint"/>
        /// </summary>
        public IPEndPoint Endpoint
        {
            get
            {
                return _endpoint;
            }
        }

        /// <summary>
        /// Returns the remote socket address.
        /// </summary>
        public string RemoteAddress
        {
            get
            {
                return $"{_endpoint.Address}:{_endpoint.Port}";
            }
        }

        public bool CanRead
        {
            get
            {
                return _stream.CanRead;
            }
        }

        public bool CanWrite
        {
            get
            {
                return _stream.CanWrite;
            }
        }

        /// <summary>
        /// <see cref="IRtspConnection.ReadByte"/>
        /// </summary>
        /// <returns></returns>
        public int ReadByte()
        {
            CheckAndAttemptReconnect();

            return _stream.ReadByte();
        }

        /// <summary>
        /// <see cref="IRtspConnection.Read(byte[], int, int)"/> 
        /// </summary>
        public int Read(byte[] buffer, int offset, int size)
        {
            CheckAndAttemptReconnect();

            return _stream.Read(buffer, offset, size);
        }

        /// <summary>
        /// <see cref="IRtspConnection.WriteByte(byte)"/>
        /// </summary>
        public void WriteByte(byte value)
        {
            CheckAndAttemptReconnect();

            try
            {
                _stream.WriteByte(value);
            }
            catch (Exception e)
            {
                LOG.Error(e, $"Failed to write to connection stream, reason: {e.Message}");
            }
        }

        /// <summary>
        /// <see cref="IRtspConnection.Write(byte[], int, int)"/>
        /// </summary>
        public void Write(byte[] buffer, int offset, int size)
        {
            CheckAndAttemptReconnect();

            try
            {
                _stream.WriteAsync(buffer, offset, size);
            }
            catch (Exception e)
            {
                LOG.Error(e, $"Failed to write to connection stream, reason: {e.Message}");
            }
        }

        /// <summary>
        /// <see cref="IRtspConnection.WriteMessage(RtspMessage)"/>
        /// </summary>
        public bool WriteMessage(RtspMessage msg)
        {
            if (msg == null)
            {
                throw new ArgumentNullException("Cannot send null message");
            }

            var message = msg.ToString();
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"Sending RTSP message\n {message}");
            }

            try
            {
                var data = Encoding.UTF8.GetBytes(message);
                Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                LOG.Error($"Unable to send RTSP message to '{Endpoint}', reason: {e.Message}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// <see cref="IRtspConnection.Close"/>
        /// </summary>
        public void Close()
        {
            LOG.Info($"Closing RtspConnection for {_endpoint}");
            Dispose();
        }

        /// <summary>
        /// <see cref="IRtspConnection.Reconnect"/>
        /// </summary>
        public void Reconnect()
        {
            if (IsConnected)
            {
                return;
            }

            if (_tcpClient != null)
            {
                _stream.Dispose();
                _tcpClient.Close();
            }

            LOG.Info($"Reconnecting RtspConnection to {_endpoint}");
            _tcpClient = new TcpClient(_endpoint.Address.ToString(), _endpoint.Port);
            _stream = _tcpClient.GetStream();
        }

        #region IDisposable

        /// <summary>
        /// <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            _stream.Dispose();
            _tcpClient.Dispose();
        }

        #endregion

        private void CheckAndAttemptReconnect()
        {

            if (!IsConnected)
            {
                LOG.Warn($"RTSP transport not connected to {Endpoint}, attempting to reconnect...");
                try
                {
                    Reconnect();
                }
                catch (Exception e)
                {
                    LOG.Error(e, $"Unable to reconnect to '{Endpoint}', reason: {e.Message}");
                    throw e;
                }
            }
        }
    }
}
