//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using NLog;
using Pelco.Media.Pipeline;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Pelco.Media.RTP
{
    public sealed class RtpUdpSource : IRtpSource, IDisposable
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private UdpChannel _rtpChannel;
        private UdpChannel _rtcpChannel;

        public RtpUdpSource(IPAddress address)
        {
            int rtpPort, rtcpPort;

            if (!FindAvailablePorts(out rtpPort, out rtcpPort))
            {
                // Becasue we use a pretty big range this is very unlikely to occur.

                // TODO(frank.lamar): Determine a better exception for this
                throw new SystemException("Unable to locate two consecutive ports to bind to.");
            }

            _rtpChannel = new UdpChannel(address, rtpPort);
            _rtcpChannel = new UdpChannel(address, rtcpPort);

            LOG.Debug($"Created RtpUdpSource with channels {rtpPort}-{rtcpPort}");
        }

        public int RtpPort
        {
            get
            {
                return _rtpChannel.Port;
            }
        }

        public int RtcpPort
        {
            get
            {
                return _rtcpChannel.Port;
            }
        }

        public ISource RtpSource
        {
            get
            {
                return _rtpChannel;
            }
        }

        public ISource RtcpSource
        {
            get
            {
                return _rtcpChannel;
            }
        }

        public void Start()
        {
            _rtpChannel.Start();
            _rtcpChannel.Start();
        }

        public void Stop()
        {
            LOG.Debug("Stopping RtpUdpSource");

            Dispose();
        }

        public void Dispose()
        {
            _rtpChannel?.Dispose();
            _rtcpChannel?.Dispose();
        }

        private bool FindAvailablePorts(out int rtpPort, out int rtcpPort)
        {
            foreach(var port in Enumerable.Range(41950, 64000))
            {
                // The RTSP spec indicates that we should start with a an even port and should
                // return consecutive port numbers for rtp and rtcp respectively.
                if ((port % 2 == 0) && (IsPortAvailable(port) && IsPortAvailable(port + 1)))
                {
                    rtpPort = port;
                    rtcpPort = port + 1;

                    return true;
                }
            }

            rtpPort = 0;
            rtcpPort = 0;

            return false;
        }

        private bool IsPortAvailable(int port)
        {
            return IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpList‌​eners()
                                                             .Any(l => l.Port != port);
        }

        private sealed class UdpChannel : SourceBase, IDisposable
        {
            private static readonly int READ_BUFFER_SIZE = 1600; // bytes
            private static readonly int RECEIVE_BUFFER_SIZE = 1024 * 1024; // 1M

            private readonly object _startLock = new object();

            private int _port;
            private bool _started;
            private Socket _source;
            private IPAddress _address;
            private byte[] _receiveBuffer;

            public UdpChannel(IPAddress address, int port)
            {
                _port = port;
                _address = address;
                _receiveBuffer = new byte[READ_BUFFER_SIZE];

                _source = new Socket(SocketType.Dgram, ProtocolType.Udp);
                _source.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _source.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, RECEIVE_BUFFER_SIZE);
                _source.Bind(new IPEndPoint(IPAddress.Any, port));
                _source.Connect(new IPEndPoint(address, 0));
            }

            public int Port
            {
                get
                {
                    return _port;
                }
            }

            public override void Start()
            {
                lock (_startLock)
                {
                    if (_started)
                    {
                        return;
                    }

                    LOG.Debug($"Starting UDP channel for {_address}:{_port}");

                    try
                    {
                        _source.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, PerformReceive, null);

                        _started = true;
                    }
                    catch (Exception e)
                    {
                        LOG.Error(e, $"Failed to start UDP source for {_address}:{_port}");
                    }
                }
            }

            public override void Stop()
            {
                Dispose();
            }

            public void Dispose()
            {
                lock (_startLock)
                {
                    try
                    {
                        _source?.Dispose();

                        _started = false;

                        LOG.Debug($"UDP channel shutdown for {_address}:{_port}");
                    }
                    catch (Exception e)
                    {
                        LOG.Error(e, $"Received exception closing UDP socket {_address}:{_port}");

                        _started = false;
                    }
                }
            }

            private void PerformReceive(IAsyncResult ar)
            {
                int bytesRead = 0;
                try
                {
                    bytesRead = _source.EndReceive(ar);
                    if (bytesRead > 0)
                    {
                        byte[] data = new byte[bytesRead];
                        Buffer.BlockCopy(_receiveBuffer, 0, data, 0, bytesRead);

                        // Push buffer up stream.
                        PushBuffer(new ByteBuffer(data));
                    }
                }
                catch (ObjectDisposedException)
                {
                    // Just ignore this exception.  The usually means that the socket was shutdown.
                    return;
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.Shutdown)
                    {
                        // The socket was shutdown just return;
                        return;
                    }

                    LOG.Error(e, "Received socket exception while processing request");
                }
                catch (Exception e)
                {
                    LOG.Error(e, "Received unexpected exception while reading from socket");
                }

                try
                {
                    _source.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, PerformReceive, null);
                }
                catch (Exception e)
                {
                    LOG.Error(e, $"Failed to begin receive, stopping UDP Channel for {_address}:{_port}");
                }
            }
        }
    }
}
