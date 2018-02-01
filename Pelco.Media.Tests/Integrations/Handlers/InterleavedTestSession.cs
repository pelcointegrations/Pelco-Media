using Pelco.Media.Pipeline;
using Pelco.Media.Pipeline.Sinks;
using Pelco.Media.Pipeline.Transforms;
using Pelco.Media.RTP;
using Pelco.Media.RTSP;
using Pelco.Media.RTSP.Server;
using System;

namespace Pelco.Media.Tests.Integrations.Handlers
{
    public class InterleavedTestSession : RtspSessionBase
    {
        public const int FRAMES_TO_SEND = 3;

        private TestSource _src;
        private PortPair _ports;
        private SessionSpy _spy;
        private byte _payloadType;
        private RequestContext _context;
        private MediaPipeline _pipeline;

        public InterleavedTestSession(RequestContext context, SessionSpy spy, PortPair ports, byte payloadType)
        {
            _payloadType = payloadType;
            _src = new TestSource(FRAMES_TO_SEND);
            _spy = spy ?? throw new ArgumentNullException("Spy cannot be null");
            _ports = ports ?? throw new ArgumentNullException("Ports cannot be null");
            _context = context ?? throw new ArgumentNullException("Context cannot be null");
        }

        public override void Start()
        {
            lock (this)
            {
                _pipeline = MediaPipeline.CreateBuilder()
                                         .Source(_src)
                                         .Transform(new RecordingTransform(_spy, Id))
                                         .Transform(new RtpPacketizer(new DefaultRtpClock(90000), SSRC, _payloadType))
                                         .Sink(new TcpInterleavedSink(_context, (byte)_ports.RtpPort))
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
                    _spy.GetData(_id).Buffers.Add(buffer);
                }
                else
                {
                    var sd = new SessionData();
                    sd.Buffers.Add(buffer);
                    _spy.Insert(_id, sd);
                }

                return PushBuffer(buffer);
            }
        }
    }
}
