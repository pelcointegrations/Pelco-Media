using NLog;
using Pelco.PDK.Media.Pipeline;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Pelco.PDK.Media.RTSP
{
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
        private BlockingCollection<ByteBuffer> _rtpQueue;
        private ConcurrentDictionary<int, RtpInterleaveMediaSource> _sources;

        public RtspListener(IRtspConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException("Transport cannot be null");
            _rtpQueue = new BlockingCollection<ByteBuffer>();
            _decoder = new RtspMessageDecoder(_rtpQueue, _connection.Endpoint);
            _sources = new ConcurrentDictionary<int, RtpInterleaveMediaSource>();
        }

        /// <summary>
        /// Event handler for receiving <see cref="RtspMessage"/>s. Rtsp messages
        /// are either <see cref="RtspRequest"/>s or <see cref="RtspResponse"/>s. 
        /// </summary>
        public event EventHandler<RtspMessageEventArgs> RtspMessageReceived
        {
            add
            {
                _decoder.RtspMessageReceived += value;
            }

            remove
            {
                _decoder.RtspMessageReceived -= value;
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

                ThreadPool.QueueUserWorkItem(ListenForRequests);
                ThreadPool.QueueUserWorkItem(ProcessInterleavedData);

                _started = true;
            }
        }

        /// <summary>
        /// Cloes the RTSP listener.
        /// </summary>
        /// <param name="closeConnection">Flag indicating if the connection should be closed as well.</param>
        public void Stop(bool closeConnection = true)
        {
            _sources.Clear();
            _rtpQueue.Dispose(); // Shutsdowns the InterleavedProcessing thread

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

            ThreadPool.QueueUserWorkItem(ListenForRequests);
        }

        /// <summary>
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

        public void SendResponse(RtspResponse response)
        {
            if (!_connection.WriteMessage(response))
            {
                LOG.Error($"Failed to write response to client at {_connection.RemoteAddress} \n{response}");
            }
        }

        private void ProcessInterleavedData(object state)
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

        private void ListenForRequests(object state)
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
                    _decoder.Decode(stream);
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
