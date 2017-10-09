using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Pelco.Media.RTSP.Server
{
    public class RtspServer
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private readonly object ListenerLock = new object();

        private int _port;
        private TcpListener _listener;
        private ManualResetEvent _stop;
        private IRequestDispatcher _dispatcher;
        private BlockingCollection<RtspMessage> _messages;
        private Dictionary<string, RtspListener> _listeners;

        public RtspServer(int port, IRequestDispatcher dispatcher)
        {
            _port = port;
            _dispatcher = dispatcher;
            _stop = new ManualResetEvent(false);
            _listeners = new Dictionary<string, RtspListener>();
        }

        public void Start()
        {
            lock (ListenerLock)
            {
                if (_listener == null)
                {
                    _stop.Reset();
                    _listener = new TcpListener(IPAddress.Any, _port);
                    _messages = new BlockingCollection<RtspMessage>();

                    _listener.Start();
                    LOG.Info($"Started RTSP server on '{_port}'");

                    ThreadPool.QueueUserWorkItem(Accept);
                    ThreadPool.QueueUserWorkItem(ProcessMessages);
                }
            }
        }

        public void Stop()
        {
            lock (ListenerLock)
            {
                if (_listener != null)
                {
                    _stop.Set();
                    _listener.Stop();
                    _messages.Dispose();
                    _listener = null;

                    foreach (var entry in _listeners)
                    {
                        try
                        {
                            entry.Value.RtspMessageReceived -= Listener_RtspMessageReceived;
                            entry.Value.Stop();
                        }
                        catch (Exception e)
                        {
                            LOG.Error($"Received exception while stopping RTSP listener for {entry.Key}, message={e.Message}");
                        }
                    }
                    _listeners.Clear();

                    LOG.Info($"RTSP server on '{_port}' successfully shutdown");
                }
            }
        }

        private void Accept(object state)
        {
            while (_stop.WaitOne(0))
            {
                try
                {
                    var client = _listener.AcceptTcpClient();
                    var conn = new RtspConnection(client);
                    var listener = new RtspListener(conn);

                    LOG.Debug($"Accepted client connection from '{conn.RemoteAddress}'");

                    listener.RtspMessageReceived += Listener_RtspMessageReceived;
                    _listeners.Add(conn.RemoteAddress, listener);
                }
                catch (Exception e)
                {
                    LOG.Error(e, $"Caught exception while accepting client connection, message={e.Message}");
                }
            }
        }

        private void Listener_RtspMessageReceived(object sender, RtspMessageEventArgs e)
        {
            _messages.Add(e.Message);
        }

        private void ProcessMessages(object state)
        {
            try
            {
                while (true)
                {
                    var msg = _messages.Take();

                    if ((msg != null) && (msg is RtspRequest))
                    {
                        HandleRequest(msg as RtspRequest);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                LOG.Info($"Message processing queue shutdown for RTSP Server at '{_port}'");
            }
            catch (Exception e)
            {
                LOG.Error(e, "Caught exception while processing RTSP message");
            }
        }

        private async void HandleRequest(RtspRequest request)
        {
            RtspListener listener = null;
            if (_listeners.TryGetValue(request.URI.Authority, out listener))
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var response = _dispatcher.Dispatch(request);

                        if (response != null)
                        {
                            listener.SendResponse(response);
                        }
                    }
                    catch (Exception e)
                    {
                        LOG.Error(e, $"Caught exception while procesing RTSP request from {request.URI}");

                        listener.SendResponse(RtspResponse.CreateBuilder()
                                                          .Status(RtspResponse.Status.InternalServerError)
                                                          .Build());
                    }
                    
                });
            }
            else
            {
                LOG.Error($"Unable to process request because no active connection was found for {request.URI}");
            }
        }
    }
}
