using Pelco.Media.Pipeline;
using Pelco.Media.Pipeline.Transforms;
using Pelco.Media.RTSP;
using Pelco.Media.RTSP.Server;
using System;

namespace Pelco.Media.Tests.Integrations.Handlers
{
    public class InterleavedTestSession : RtspSessionBase
    {
        private TestSource _src;
        private PortPair _ports;
        private SessionSpy _spy;
        private byte _payloadType;
        private RequestContext _context;
        private MediaPipeline _pipeline;

        public InterleavedTestSession(RequestContext context, SessionSpy spy, PortPair ports, byte payloadType)
        {
            _payloadType = payloadType;
            _src = new TestSource();
            _spy = spy ?? throw new ArgumentNullException("Spy cannot be null");
            _ports = ports ?? throw new ArgumentNullException("Ports cannot be null");
            _context = context ?? throw new ArgumentNullException("Context cannot be null");
        }

        public int FramesSent
        {
            get
            {
                return _src.FramesSent;
            }
        }

        public override void Start()
        {
            lock (this)
            {
                _pipeline = MediaPipeline.CreateBuilder()
                                         .Source(_src)
                                         .Transform(new RecordingTransform(_spy, Id))
                                         .Transform(new RtpPacketizer(SSRC, _payloadType))
                                         .Sink(new InterleavedSink(_context, (byte)_ports.RtpPort))
                                         .Build();

                _pipeline.Start();
            }
        }

        public override void Stop()
        {
            lock (this)
            {
                _pipeline?.Stop();
            }
        }

        private sealed class RecordingTransform : TransformBase
        {
            private string _id;
            private SessionSpy _spy;

            public RecordingTransform(SessionSpy spy, string id)
            {
                _id = id;
                _spy = spy;
            }

            public override bool WriteBuffer(ByteBuffer buffer)
            {
                if (_spy.ContainsData(_id))
                {
                    _spy.IncrementBy(_id, 1, buffer.Length);
                }
                else
                {
                    _spy.Insert(_id, new SessionData { TotalPacketsSent = 1, TotalBytesSent = buffer.Length });
                }

                return PushBuffer(buffer);
            }
        }

        private sealed class InterleavedSink : ISink
        {
            private const byte INTERLEAVED_MARKER = 0X24;

            private byte _channel;
            private RequestContext _context;

            public InterleavedSink(RequestContext context, byte channel)
            {
                _channel = channel;
                _context = context;
            }

            public ISource UpstreamLink { set; get; }

            public void PushEvent(MediaEvent e)
            {
                throw new NotImplementedException();
            }

            public void Stop()
            {
            }

            public bool WriteBuffer(ByteBuffer buffer)
            {
                // Creating buffer to hold rtsp packet. $<channel id>{2 byte length}{RTP packet}
                var packet = new ByteBuffer(4 + buffer.Length);
                packet.WriteByte(INTERLEAVED_MARKER);
                packet.WriteByte(_channel);
                packet.WriteUInt16((UInt16)buffer.Length);
                packet.Write(buffer);

                return _context.Write(packet);
            }
        }

    }
}
