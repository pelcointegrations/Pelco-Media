using NLog;
using Pelco.Media.Common;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;

namespace Pelco.Media.RTSP.Client
{

    public delegate void RtspResponseCallback(RtspResponse response);

    public class RtspClient : IDisposable
    {
        public static readonly int DEFAULT_RTSP_PORT = 554;

        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private readonly object _cseqLock = new object();

        private Uri _uri;
        private int _cseq;
        private RtspListener _listener;
        private TimeSpan _defaultTimeout;
        private Credentials _credentials;
        private RtspConnection _connection;
        private ChallengeResponse _authResponse;
        private ConcurrentDictionary<int, AsyncResponse> _callbacks;

        public RtspClient(Uri uri, Credentials creds = null)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("Cannot create RTSP client from null uri");
            }

            _uri = uri;
            _cseq = 0;
            _credentials = creds;
            _defaultTimeout = TimeSpan.FromSeconds(20);
            _callbacks = new ConcurrentDictionary<int, AsyncResponse>();
            _connection = new RtspConnection(IPAddress.Parse(uri.Host), uri.Port == -1 ? DEFAULT_RTSP_PORT : uri.Port);
            _listener = new RtspListener(_connection);
            _listener.RtspMessageReceived += Rtsp_RtspMessageReceived;

            LOG.Info($"Created RTSP client for '{_connection.Endpoint}'");

            _listener.Start();
        }

        public bool IsConnected
        {
            get
            {
                return _connection.IsConnected;
            }
        }

        public EndPoint EndPoint
        {
            get
            {
                return _connection.Endpoint;
            }
        }

        public void Reconnect()
        {
            _listener.Reconnect();
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            LOG.Info($"Disposing RTSP client connected to '{_connection.Endpoint}'");

            _listener.Stop();

            foreach (var cb in _callbacks)
            {
                cb.Value.Dispose();
            }
            _callbacks.Clear();
        }

        public RtspResponse Send(RtspRequest request)
        {
            return Send(request, _defaultTimeout);
        }

        /// <summary>
        /// Retrieves an RTP/RTCP source for the specified channel id.
        /// </summary>
        /// <param name="channelId">The channel id of interest</param>
        /// <returns></returns>
        public RtpInterleaveMediaSource GetChannelSource(int channelId)
        {
            return _listener.GetChannelSource(channelId);
        }

        /// <summary>
        /// Asynchronously sends RTSP request.  Invokes callback if a response is received
        /// from the server.
        /// </summary>
        /// <param name="request">The request to send</param>
        /// <param name="callback">Callback to be called when a response is available</param>
        public void SendAsync(RtspRequest request, RtspResponseCallback callback)
        {
            AsyncResponse asyncRes = null;
            try
            {
                if (_authResponse != null && _credentials != null)
                {
                    // Set the authorization header if we have a cached auth response.
                    request.Authorization = _authResponse.Generate(request.Method, request.URI);
                }

                asyncRes = DoSend(request, (res) =>
                {
                    var status = res.ResponseStatus;
                    if (status.Is(RtspResponse.Status.Unauthorized) && _credentials != null)
                    {
                        _authResponse = AuthChallenge.Parse(_credentials, res.WWWAuthenticate);

                        if (_authResponse != null)
                        {
                            LOG.Warn($"Received RTSP Unauthorized response re-trying with creds {_credentials.Username}:{_credentials.Password}");

                            request.Authorization = _authResponse.Generate(request.Method, request.URI);
                            asyncRes = DoSend(request, callback);
                        }
                    }
                    else
                    {
                        callback.Invoke(res);
                    }
                });
            }
            catch (Exception e)
            {
                if (asyncRes != null)
                {
                    RemoveCallback(asyncRes.CSeq);
                }
                throw e;
            }
        }

        public RtspResponse Send(RtspRequest request, TimeSpan timeout)
        {
            AsyncResponse asyncRes = null;
            try
            {
                if (_authResponse != null && _credentials != null)
                {
                    // Set the authorization header if we have a cached auth response.
                    request.Authorization = _authResponse.Generate(request.Method, request.URI);
                }

                asyncRes = DoSend(request);
                RtspResponse response = asyncRes.Get(timeout);

                var status = response.ResponseStatus;
                if (status.Is(RtspResponse.Status.Unauthorized) && _credentials != null)
                {
                    _authResponse = AuthChallenge.Parse(_credentials, response.WWWAuthenticate);

                    if (_authResponse != null)
                    {
                        LOG.Warn($"Received RTSP Unauthorized response re-trying with creds {_credentials.Username}:{_credentials.Password}");

                        request.Authorization = _authResponse.Generate(request.Method, request.URI);
                        asyncRes = DoSend(request);
                        response = asyncRes.Get(timeout);
                    }
                }

                return response;
            }
            catch (Exception e)
            {
                if (asyncRes != null)
                {
                    RemoveCallback(asyncRes.CSeq);
                }

                throw e;
            }
        }

        private AsyncResponse DoSend(RtspRequest request, RtspResponseCallback resCallback = null)
        {
            int cseq = GetNextCSeq();
            request.CSeq = _cseq;

            AsyncResponse callback = new AsyncResponse(cseq, resCallback);

            if (!_connection.WriteMessage(request))
            {
                callback.Dispose();
                throw new RtspClientException("Unable to send request to client");
            }

            _callbacks[cseq] = callback;

            return callback;
        }

        private int GetNextCSeq()
        {
            lock (_cseqLock)
            {
                _cseq++;

                // Handle wrap around in the very rare case.
                if (_cseq == ushort.MaxValue)
                {
                    _cseq = 1;
                }

                return _cseq;
            }
        }

        private void RemoveCallback(int cseq)
        {
            AsyncResponse res = null;
            if (_callbacks.TryRemove(cseq, out res))
            {
                res.Dispose();
            }
        }

        private void Rtsp_RtspMessageReceived(object sender, RtspMessageEventArgs e)
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"Received RTSP message from '{_connection.Endpoint}'");
                LOG.Debug(e.Message.ToString());
            }

            if (e.Message is RtspResponse)
            {
                var response = e.Message as RtspResponse;

                int cseq = response.CSeq;
                if (cseq <= 0)
                {
                    LOG.Warn("Receive RTSP response that does not contain cseq header, disgarding.");
                    return;
                }

                AsyncResponse cb = null;
                if (_callbacks.TryRemove(cseq, out cb))
                {
                    cb.HandleResponse(response);
                }
            }
            else
            {
                // Server sent request.  Since we do not support server side requests lets just
                // respond back with MethodNotAllowed.
                RtspResponse response = new RtspResponse(RtspResponse.Status.MethodNotAllowed);
                response.CSeq = e.Message.CSeq;

                if (!_connection.WriteMessage(response))
                {
                    LOG.Error("Received RTSP request from server but unable to send response.");
                }
            }
        }

        // Helper class used for handling async responses and making them synchronous.
        private class AsyncResponse : IDisposable
        {
            private int _cseq;
            private ManualResetEvent _event;
            private RtspResponse _response;
            private RtspResponseCallback _callback;

            public AsyncResponse(int cseq)
            {
                _cseq = cseq;
                _response = null;
                _callback = null;
                _event = new ManualResetEvent(false);
            }

            public AsyncResponse(int cseq, RtspResponseCallback callback) : this(cseq)
            {
                _callback = callback;
            }

            public int CSeq
            {
                get
                {
                    return _cseq;
                }
            }

            public RtspResponse Get(TimeSpan timeout)
            {
                try
                {
                    if (!_event.WaitOne(timeout))
                    {
                        throw new TimeoutException($"Timed out waiting for RTSP response 'cseq={_cseq}'");
                    }

                    if (_response == null)
                    {
                        throw new RtspClientException($"Did not receive response from server for 'cseq={_cseq}'");
                    }

                    return _response;
                }
                catch (Exception e)
                {
                    throw new RtspClientException($"Received exception while waiting for response, msg: {e.Message}");
                }
            }

            public void HandleResponse(RtspResponse response)
            {
                if (_callback != null)
                {
                    _callback.Invoke(response);
                }
                else
                {
                    _response = response;
                    _event.Set();
                }
            }

            public void Dispose()
            {
                _event.Dispose();
            }
        }
    }
}
