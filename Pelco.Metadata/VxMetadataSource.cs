using NLog;
using Pelco.PDK.Media.Pipeline;
using Pelco.PDK.Media.Pipeline.Transforms;
using Pelco.PDK.Media.RTP;
using Pelco.PDK.Media.RTSP;
using Pelco.PDK.Media.RTSP.Client;
using Pelco.PDK.Media.Common;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace Pelco.PDK.Metadata
{
    public class VxMetadataSource : IDisposable
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private readonly object SourceLock = new object();

        private Uri _currentUri;
        private Uri _originalUri;
        private MimeType _filter;
        private ISink _clientSink;
        private RtspClient _client;
        private Credentials _creds;
        private PlayingState _state;
        private uint _refreshInterval;
        private DateTime? _pausedTime;
        private DateTime? _playbackTime;
        private List<RtpSession> _sessions;
        private ImmutableList<MediaTrack> _tracks;
        private Timer _sessionRefreshTimer;

        public VxMetadataSource(Uri uri, Credentials creds = null, MimeType filter = null)
        {
            _currentUri = uri;
            _originalUri = uri;
            _creds = creds;
            _pausedTime = null;
            _playbackTime = null;
            _state = PlayingState.NONE;
            _filter = filter == null ? MimeType.ANY_APPLICATION : filter;
            _sessions = new List<RtpSession>();
            _tracks = ImmutableList.Create<MediaTrack>();
            _client = new RtspClient(uri, creds);
        }

        public void Initialize()
        {
            lock (SourceLock)
            {
                if (_state != PlayingState.NONE)
                {
                    // Already initialized
                    return;
                }

                var method = RtspRequest.RtspMethod.OPTIONS;
                var builder = RtspRequest.CreateBuilder().Uri(_currentUri).Method(method);

                // TODO(frank.lamar): Add check for supported operations.  Otherwise this call
                // is meaning less.
                CheckResponse(_client.Send(builder.Build()), method);

                // Send Describe to server
                method = RtspRequest.RtspMethod.DESCRIBE;
                var response = CheckResponse(_client.Send(builder.Method(method).Build()), method);

                _tracks = MediaTracks.FromSdp(response.GetBodyAsSdp(), _currentUri, _filter);

                // Initialize our session refresh timmer.
                _sessionRefreshTimer = new Timer();
                _sessionRefreshTimer.AutoReset = true;
                _sessionRefreshTimer.Elapsed += SessionRefreshTimer_Elapsed;

                _state = PlayingState.INITIALIZED;
            }
        }

        public ImmutableList<MediaTrack> GetTracks()
        {
            lock (SourceLock)
            {
                if (_state == PlayingState.NONE)
                {
                    throw new InvalidOperationException("Cannot retireve tracks until you Initialize() is called");
                }

                return _tracks;
            }
        }

        public void Play(ISink rtpSink, DateTime? playAt = null)
        {
            Play(rtpSink, playAt, false);
            _state = PlayingState.PLAYING;
        }

        public void Pause()
        {
            lock (SourceLock)
            {
                _pausedTime = DateTime.Now;

                LOG.Info($"Pausing VxMetadataSource at {_pausedTime.Value}");

                _sessions.ForEach(session => Pause(session));
                _state = PlayingState.PAUSED;
            }
        }

        public void UnPause()
        {  
            lock (SourceLock)
            {
                if (_state == PlayingState.PAUSED && _pausedTime.HasValue)
                {
                    LOG.Info($"Resuming playback at {_pausedTime.Value}");

                    _sessions.ForEach(session => Seek(_pausedTime.Value));

                    _pausedTime = null;
                }
            }
        }

        public void Seek(DateTime seekTo)
        {
            lock (SourceLock)
            {
                if (_playbackTime.HasValue)
                {
                    // Our current session(s) are playback sessions.
                    // TODO(frank.lamar): Determine if we can just do this at the
                    // aggregate URI or if we have to do this for each session.
                    _sessions.ForEach(session => Play(session, seekTo));
                }
                else
                {
                    // We currently have a live session and need to initiate a new playback session.
                    ReInitialize(_originalUri);
                    Play(_clientSink, seekTo, interleaved: true);
                }

                _playbackTime = seekTo;
            }
        }

        public void JumpToLive()
        {
            lock (SourceLock)
            {
                if (_playbackTime.HasValue)
                {
                    LOG.Info($"Current playback time is {_playbackTime}, jumping back to live stream");

                    _playbackTime = null;

                    ReInitialize(_originalUri);
                    Play(_clientSink, playAt: null, interleaved: false);
                }
                else
                {
                    LOG.Info($"No playback time defined the live stream is currently being viewed.");
                }
            }
        }

        public void Close()
        {
            Dispose();
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposeClient)
        {
            lock (SourceLock)
            {
                _tracks.Clear();

                _sessionRefreshTimer.Stop();
                _sessionRefreshTimer.Dispose();

                _sessions.ForEach(s => Teardown(s, andRemove: false));
                _sessions.Clear();

                if (disposeClient)
                {
                    _client.Dispose();
                }

                _state = PlayingState.NONE;
            }
        }

        #endregion

        private void ReInitialize(Uri uri)
        {
            lock (SourceLock)
            {
                _currentUri = uri;

                if (_client.EndPoint.Equals(ToEndpoint(uri)))
                {
                    // The server endpoint is the same no need to create a
                    // new client so we will dispose everything and keep the
                    // existing client connection.
                    Dispose(disposeClient: false);
                }
                else
                {
                    Dispose(disposeClient: true);
                    _client = new RtspClient(_currentUri, _creds);
                }

                Initialize();
            }
        }

        private EndPoint ToEndpoint(Uri uri)
        {
            return new IPEndPoint(Dns.GetHostAddresses(uri.Host)[0], uri.Port == -1 ? RtspClient.DEFAULT_RTSP_PORT :  uri.Port);
        }

        private RtpSession Setup(MediaTrack track, RtpChannelSink sink, bool interleaved)
        {
            lock (SourceLock)
            {
                IRtpSource rtpSource = null;
                try
                {
                    TransportHeader transport = null;
                    if (interleaved)
                    {
                        transport = TransportHeader.CreateBuilder()
                                                   .Type(Media.RTSP.TransportType.RtspInterleaved)
                                                   .InterleavedChannels(0, 1)
                                                   .Build();
                    }
                    else
                    {
                        // TODO(frank.lamar): Add multicast support.
                        rtpSource = new RtpUdpSource(track.Address);
                        transport = TransportHeader.CreateBuilder()
                                                   .Type(Media.RTSP.TransportType.UdpUnicast)
                                                   .ClientPorts(rtpSource.RtpPort, rtpSource.RtcpPort)
                                                   .Build();
                    }

                    var response = CheckResponse(_client.Send(RtspRequest.CreateBuilder()
                                                                         .Method(RtspRequest.RtspMethod.SETUP)
                                                                         .Uri(track.ControlUri)
                                                                         .AddHeader(RtspHeaders.Names.TRANSPORT, transport.ToString())
                                                                         .Build()), RtspRequest.RtspMethod.SETUP);

                    if (!response.Headers.ContainsKey(RtspHeaders.Names.SESSION))
                    {
                        throw new RtspClientException("Rtsp SETUP response does not contain a session id");
                    }
                    var rtspSession = Session.Parse(response.Headers[RtspHeaders.Names.SESSION]);

                    transport = response.Transport;
                    if (interleaved)
                    {
                        if (transport.Type != Media.RTSP.TransportType.RtspInterleaved)
                        {
                            throw new RtspClientException($"Server does not support interleaved. Response Transport='{transport}'");
                        }

                        var channels = transport.InterleavedChannels != null ? transport.InterleavedChannels : new PortPair(0, 1);
                        sink.Channel = channels.RtpPort; // Ensure that the sink contains the correct Interleaved channel id.

                        rtpSource = new RtpInterleavedSource(_client.GetChannelSource(channels.RtpPort),
                                                             _client.GetChannelSource(channels.RtcpPort));
                    }

                    var pipeline = MediaPipeline.CreateBuilder()
                                                .Source(rtpSource.RtpSource)
                                                .TransformIf(transport.SSRC != null, new SsrcFilter(transport.SSRC))
                                                .Sink(sink)
                                                .Build();

                    var session = new RtpSession(track, rtspSession, rtpSource);
                    session.Pipelines.Add(pipeline);
                    session.Start();

                    CheckAndStartRefreshTimer(session.Session.Timeout);

                    return session;
                }
                catch (Exception e)
                {
                    if (rtpSource != null)
                    {
                        rtpSource.Stop();
                    }

                    if (e is RtspClientException)
                    {
                        throw e;
                    }

                    throw new RtspClientException($"Unable to set up media track {track.ID}", e);
                }
            }
        }

        private void Play(ISink rtpSink, DateTime? playAt = null, bool interleaved = false)
        {
            lock (SourceLock)
            {
                _clientSink = rtpSink;
                _playbackTime = playAt;

                int channel = 0;
                foreach (var track in _tracks)
                {
                    RtpSession session = null;
                    try
                    {
                        session = Setup(track, new RtpChannelSink(++channel, rtpSink), interleaved);
                        if (!Play(session, playAt))
                        {
                            // This session needs to be deleted because it is not longer valide due to
                            // an RTSP redirect.
                            session.Dispose();
                        }
                        else
                        {
                            // We have a good session add it so we can manage it.
                            _sessions.Add(session);
                        }
                    }
                    catch (Exception e)
                    {
                        LOG.Error(e, $"Failed to start playing VxMetadataSource from '{_currentUri}'");

                        if (session != null)
                        {
                            // A failure occured we should dispose the session to ensure we
                            // cleanup our resources.
                            session.Dispose();
                        }

                        throw e;
                    }
                }
            }
        }

        private bool Play(RtpSession session, DateTime? playAt = null)
        {
            lock (SourceLock)
            {
                // No need to check initialization state because tracks are not available if we have
                // not first initialized things.

                bool retVal = true;
                var method = RtspRequest.RtspMethod.PLAY;

                try
                {
                    var builder = RtspRequest.CreateBuilder()
                                             .Uri(session.Track.ControlUri)
                                             .Method(method)
                                             .AddHeader(RtspHeaders.Names.SESSION, session.ID);

                    if (playAt.HasValue)
                    {
                        // Play recorded video requested, add RTSP Range header.
                        builder.AddHeader(RtspHeaders.Names.RANGE, ToRtspAbsoluteRangeValue(playAt.Value));
                        builder.AddHeader(RtspHeaders.Names.RATECONTROL, "no"); // Tells server we want to control transfer rate.
                    }

                    // Issue play call to RTSP server.
                    var response = CheckResponse(_client.Send(builder.Build()), RtspRequest.RtspMethod.PLAY);

                    var status = response.ResponseStatus;

                    if (status.Is(RtspResponse.Status.MovedPermanently) || status.Is(RtspResponse.Status.MovedTemporarily))
                    {
                        // We received a redirect.  In this case we need to follow the redirect and then re-initialize the
                        // the source to the new endpoint.

                        if (response.Headers.ContainsKey(RtspHeaders.Names.LOCATION))
                        {
                            var value = response.Headers[RtspHeaders.Names.LOCATION];

                            Uri uri = null;
                            if (Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out uri))
                            {
                                // A valid redirect URI was provided lets redirect and start playing from
                                // the provided seek time.
                                ReInitialize(uri);

                                Play(_clientSink, playAt, playAt.HasValue);

                                // Indicates to the caller that the original session should be disposed because of a redirect.
                                retVal = false;
                            }
                            else
                            {
                                throw new RtspException("Server returned a redirect with a malformed LOCATION uri");
                            }
                        }
                        else
                        {
                            throw new RtspException("Server returned a redirect without a LOCATION uri");
                        }

                    }

                    return retVal;
                }
                catch (RtspClientException e)
                {
                    LOG.Error(e, e.Message);
                    throw e;
                }
                catch (Exception e)
                {
                    var msg = $"RTSP play call failed for session '{session.ID}', reason: {e.Message}";

                    LOG.Error(e, msg);
                    throw new RtspClientException(msg, e);
                }
            }
        }

        private void Pause(RtpSession session)
        {
            lock (SourceLock)
            {
                if (_state == PlayingState.PLAYING)
                {
                    LOG.Debug($"Sending pause to session '{session.ID}' at {session.Track.ControlUri}");

                    CheckResponse(_client.Send(RtspRequest.CreateBuilder()
                                                          .Uri(_currentUri)
                                                          .Method(RtspRequest.RtspMethod.PAUSE)
                                                          .AddHeader(RtspHeaders.Names.SESSION, session.ID)
                                                          .Build()), RtspRequest.RtspMethod.PAUSE);

                    session.Pause();
                }

            }
        }

        private void Teardown(RtpSession session, bool andRemove = true)
        {
            try
            {
                LOG.Debug($"Tearing RTSP session '{session.ID}' at '{session.Track.ControlUri}'");

                // TODO(frank.lamar): Use async request when support is added to the client.
                var response = _client.Send(RtspRequest.CreateBuilder()
                                                       .Uri(_currentUri)
                                                       .Method(RtspRequest.RtspMethod.TEARDOWN)
                                                       .AddHeader(RtspHeaders.Names.SESSION, session.ID)
                                                       .Build());

                if (response.ResponseStatus.Code >= RtspResponse.Status.BadRequest.Code)
                {
                    LOG.Error($"Failed to teardown session '{session.ID}' received {response.ResponseStatus}");
                }

            }
            catch (Exception e)
            {
                LOG.Error($"Failed to Teardown session '{session.ID}' for {session.Track.ControlUri}, reason: {e.Message}");
            }
            finally
            {
                session.Dispose();

                if (andRemove)
                {
                    _sessions.Remove(session); // Remove from the list of sessions.
                }
            }
        }


        private void CheckAndStartRefreshTimer(uint timeoutSecs)
        {
            if (!_sessionRefreshTimer.Enabled)
            {
                _refreshInterval = timeoutSecs;

                _sessionRefreshTimer.Interval = (timeoutSecs - 3) * 1000;
                _sessionRefreshTimer.Start();
            }
            else if (timeoutSecs < _refreshInterval)
            {
                // Because it is possible to have multiple sessions for different tracks
                // we will make the refresh interval the floor of all the available sessions.
                // Having different timeouts should never happen but we will handling it because
                // you just never know.
                _refreshInterval = timeoutSecs;

                // Because the timeout value is less we need to stop the timer and adjust
                // the interval.
                _sessionRefreshTimer.Stop();
                _sessionRefreshTimer.Interval = (timeoutSecs - 3) * 1000;

                // Defensive refresh just incase the timer was just about ready to
                // elapse before we shutdown things down.  This will ensure that the
                // session doesn't expire.
                SessionRefreshTimer_Elapsed(this, null);

                _sessionRefreshTimer.Start();
            }
        }

        private async void SessionRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Perform our refresh in the background.
            await Task.Run(() =>
            {
                lock (SourceLock)
                {
                        // If there are multiple RTSP sessions available refreshing a single one
                        // with the base (aggragate) uri should refresh them all.
                        var sessionId = _sessions[0].ID;

                    try
                    {
                        _client.Send(RtspRequest.CreateBuilder()
                                                .Uri(_currentUri)
                                                .Method(RtspRequest.RtspMethod.GET_PARAMETER)
                                                .AddHeader(RtspHeaders.Names.SESSION, sessionId)
                                                .Build());
                    }
                    catch (Exception ex)
                    {
                        LOG.Error($"Unable to perform session refresh on session {sessionId}, reason: {ex.Message}");
                    }
                }
            });
        }

        private string ToRtspAbsoluteRangeValue(DateTime dateTime)
        {
            // In the case of playback we should be using absolute times for the range header
            // as defined in RFC 7826 section 4.4.3.  We will define an open range since really
            // all we care about is where to start playing.
            return $"clock={dateTime.ToUniversalTime().ToString(format: "yyyyMMddTHHmmss.fffZ")}-";
        }

        private RtspResponse CheckResponse(RtspResponse response, RtspRequest.RtspMethod method)
        {
            var status = response.ResponseStatus;

            if (status.Code >= RtspResponse.Status.BadRequest.Code)
            {
                throw new RtspClientException($"{method} received response status {status.Code} {status.ReasonPhrase}");
            }

            return response;
        }

        // Class used to append the channel ID to the buffer as well as mux
        // streams together if the source contains multiple streams of the
        // requested metadadta type.
        private sealed class RtpChannelSink : TransformBase
        {

            public RtpChannelSink(int channel, ISink rtpSink)
            {
                DownstreamLink = rtpSink;
                Channel = channel;
            }

            public int Channel { get; set; }

            public new void Stop()
            {
                base.Stop();
            }

            public override bool WriteBuffer(Media.Pipeline.ByteBuffer buffer)
            {
                buffer.Channel = Channel; // Sets the buffer's channel

                return PushBuffer(buffer);
            }
        }
    }
}
