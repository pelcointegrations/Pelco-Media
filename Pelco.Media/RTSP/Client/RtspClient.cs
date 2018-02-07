//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using NLog;
using Pelco.Media.Common;
using Pelco.Media.Pipeline;
using Pelco.Media.RTSP.SDP;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Pelco.Media.RTSP.Client
{
    public delegate void RtspResponseCallback(RtspResponse response);

    public sealed class RtspClient : IDisposable
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
        private BlockingCollection<ByteBuffer> _rtpQueue;
        private ConcurrentDictionary<int, AsyncResponse> _callbacks;
        private ConcurrentDictionary<int, RtpInterleaveMediaSource> _sources;

        public RtspClient(Uri uri, Credentials creds = null)
        {
            _uri = uri ?? throw new ArgumentNullException("Cannot create RTSP client from null uri");
            _cseq = 0;
            _credentials = creds;
            _defaultTimeout = TimeSpan.FromSeconds(20);
            _callbacks = new ConcurrentDictionary<int, AsyncResponse>();
            _sources = new ConcurrentDictionary<int, RtpInterleaveMediaSource>();
            _connection = new RtspConnection(IPAddress.Parse(uri.Host), uri.Port == -1 ? DEFAULT_RTSP_PORT : uri.Port);
            _listener = new RtspListener(_connection, OnRtspChunk);
            _rtpQueue = new BlockingCollection<ByteBuffer>();

            LOG.Info($"Created RTSP client for '{_connection.Endpoint}'");

            Task.Run(() => ProcessInterleavedData());

            _listener.Start();
        }

        ~RtspClient()
        {
            Dispose();
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
            if (_listener != null)
            {
                LOG.Info($"Disposing RTSP client connected to '{_connection.Endpoint}'");

                _listener?.Stop();
                _rtpQueue?.Dispose();

                foreach (var cb in _callbacks)
                {
                    cb.Value.Dispose();
                }
                _callbacks.Clear();

                _listener = null;

                foreach (var src in _sources)
                {
                    src.Value.Stop();
                }
                _sources.Clear();

                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Returns an <see cref="IRtspInvoker"/> instance used to send RTSP requests
        /// </summary>
        /// <returns></returns>
        public IRtspInvoker Request()
        {
            return new InternalRtspInvoker(this, _uri);
        }

        public RtspResponse Send(RtspRequest request)
        {
            return Send(request, _defaultTimeout);
        }

        //// <summary>
        /// Retrieves a <see cref="ISource"/> used for receiving data associated with the
        /// interleaved channel.  If a source does not exist then one is created.
        /// </summary>
        /// <param name="channel">The channel id of interest</param>
        /// <returns></returns>
        public RtpInterleaveMediaSource GetChannelSource(int channel)
        {
            if (!_sources.ContainsKey(channel))
            {
                var source = new RtpInterleaveMediaSource(channel);
                _sources[channel] = source;

                return source;
            }

            return _sources[channel];
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

        private void OnRtspChunk(RtspChunk chunk)
        {
            if (chunk is InterleavedData)
            {
                var interleaved = chunk as InterleavedData;
                var buffer = new ByteBuffer(interleaved.Payload, 0, interleaved.Payload.Length, true);
                buffer.Channel = interleaved.Channel;

                _rtpQueue.Add(buffer);
            }
            else if (chunk is RtspResponse)
            {
                var response = chunk as RtspResponse;

                LOG.Debug($"Received RTSP response from '{_connection.Endpoint}'");
                LOG.Debug(response.ToString());

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
                var msg = chunk as RtspMessage;

                LOG.Debug($"Received RTSP request from '{_connection.Endpoint}'");
                LOG.Debug(msg.ToString());

                // Server sent request.  Since we do not support server side requests lets just
                // respond back with MethodNotAllowed.
                RtspResponse response = new RtspResponse(RtspResponse.Status.MethodNotAllowed);
                response.CSeq = msg.CSeq;

                if (!_connection.WriteMessage(response))
                {
                    LOG.Error("Received RTSP request from server but unable to send response.");
                }
            }
        }

        private void ProcessInterleavedData()
        {
            try
            {
                LOG.Info($"Starting RTP/RTCP processing thread '{Thread.CurrentThread.ManagedThreadId}' for '{_connection.Endpoint}'");

                while (true)
                {
                    var buffer = _rtpQueue.Take();
                    if (_sources.ContainsKey(buffer.Channel))
                    {
                        var channel = buffer.Channel;
                        try
                        {
                            // Write the buffer to the correct source.
                            _sources[channel].WriteBuffer(buffer);
                        }
                        catch (Exception e)
                        {
                            LOG.Error(e, $"Unable to process interleaved data for channel {buffer.Channel}.");
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                LOG.Debug("Interleaved processing queue disposed, exiting RTP/RTCP processing thread.");
            }
            catch (InvalidOperationException e)
            {
                // This will only occur if the queue is marked for add complete, or the underlying
                // collection was modified outside the scope of the BlockingCollection.
                LOG.Error(e, $"Unable to retrieve RTP/RTCP interleaved data, queue was improperly modified.");
            }
            catch (Exception e)
            {
                LOG.Error(e, "Received unexpected exception while processing RTP/RTCP interleaved packet");
            }

            LOG.Info($"Exiting RTSP/RTP interleaved processing thread({Thread.CurrentThread.ManagedThreadId}) for '{_connection.Endpoint}'");
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

        private sealed class InternalRtspInvoker : IRtspInvoker
        {
            private Uri _baseUri;
            private RtspClient _client;
            private RtspRequest.Builder _builder;

            public InternalRtspInvoker(RtspClient client, Uri baseUri)
            {
                _baseUri = baseUri;
                _client = client;
                _builder = RtspRequest.CreateBuilder();
            }

            public IRtspInvoker AddHeader(string name, string value)
            {
                _builder.AddHeader(name, value);

                return this;
            }

            public RtspResponse Describe()
            {
                return _client.Send(_builder.Method(RtspRequest.RtspMethod.DESCRIBE)
                                            .Uri(_baseUri)
                                            .Build());
            }

            public void DescribeAsync(RtspResponseCallback callback)
            {

                _client.SendAsync(_builder.Method(RtspRequest.RtspMethod.DESCRIBE)
                                          .Uri(_baseUri)
                                          .Build(),
                                  callback);
            }

            public RtspResponse GetParameter()
            {
                return _client.Send(_builder.Method(RtspRequest.RtspMethod.GET_PARAMETER)
                                            .Uri(_baseUri)
                                            .Build());
            }

            public void GetParameterAsync(RtspResponseCallback callback)
            {
                _client.SendAsync(_builder.Method(RtspRequest.RtspMethod.GET_PARAMETER)
                                          .Uri(_baseUri)
                                          .Build(),
                                  callback);
            }

            public SessionDescription GetSessionDescription()
            {
                throw new NotImplementedException();
            }

            public RtspResponse Options()
            {
                return _client.Send(_builder.Method(RtspRequest.RtspMethod.OPTIONS)
                                            .Uri(_baseUri)
                                            .Build());
            }

            public void OptionsAsync(RtspResponseCallback callback)
            {
                _client.SendAsync(_builder.Method(RtspRequest.RtspMethod.OPTIONS)
                                          .Uri(_baseUri)
                                          .Build(),
                                  callback);
            }

            public RtspResponse Play()
            {
                return _client.Send(_builder.Method(RtspRequest.RtspMethod.PLAY)
                                            .Uri(_baseUri)
                                            .Build());
            }

            public void PlayAsync(RtspResponseCallback callback)
            {
                _client.SendAsync(_builder.Method(RtspRequest.RtspMethod.PLAY)
                                          .Uri(_baseUri)
                                          .Build(),
                                  callback);
            }

            public IRtspInvoker Session(string session)
            {
                _builder.AddHeader(RtspHeaders.Names.SESSION, session);

                return this;
            }

            public RtspResponse SetUp()
            {
                return _client.Send(_builder.Method(RtspRequest.RtspMethod.SETUP)
                                            .Uri(_baseUri)
                                            .Build());
            }

            public void SetUpAsync(RtspResponseCallback callback)
            {
                _client.SendAsync(_builder.Method(RtspRequest.RtspMethod.SETUP)
                                          .Uri(_baseUri)
                                          .Build(),
                                  callback);
            }

            public RtspResponse TearDown()
            {
                return _client.Send(_builder.Method(RtspRequest.RtspMethod.TEARDOWN)
                                            .Uri(_baseUri)
                                            .Build());
            }

            public void TeardownAsync(RtspResponseCallback callback)
            {
                _client.SendAsync(_builder.Method(RtspRequest.RtspMethod.TEARDOWN)
                                          .Uri(_baseUri)
                                          .Build(),
                                  callback);
            }

            public IRtspInvoker Transport(TransportHeader transport)
            {
                _builder.AddHeader(RtspHeaders.Names.TRANSPORT, transport.ToString());

                return this;
            }

            public IRtspInvoker Uri(Uri uri)
            {
                _builder.Uri(uri);

                return this;
            }
        }
    }
}
