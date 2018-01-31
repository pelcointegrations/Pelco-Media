using Pelco.Media.Pipeline;
using Pelco.Media.Pipeline.Transforms;
using Pelco.Media.RTP;
using Pelco.Media.RTSP;
using System;
using System.Threading;
using Xunit;

namespace Pelco.Media.Tests.Integrations.Handlers
{
    public class InterleavedClientServerTest : IClassFixture<RtspCommunicationFixture>
    {
        private static readonly string URI_PATH = "/interleaved/test";

        private SessionSpy _spy;
        private RtspCommunicationFixture _fixture;

        public InterleavedClientServerTest(RtspCommunicationFixture fixture)
        {
            _fixture = fixture;
            _spy = new SessionSpy();
            _fixture.Initialize(URI_PATH, new InterleavedTestHandler(_spy));
        }

        [Fact]
        public void TestInterleaved()
        {
            var transport = TransportHeader.CreateBuilder()
                                           .Type(TransportType.RtspInterleaved)
                                           .InterleavedChannels(0, 1)
                                           .Build();

            var response = _fixture.Client.Request().Transport(transport).SetUp();
            Assert.True(response.ResponseStatus.Is(RtspResponse.Status.Ok));

            transport = response.Transport;
            Assert.NotNull(transport);
            Assert.Equal(TransportType.RtspInterleaved, transport.Type);
            Assert.Equal(0, transport.InterleavedChannels.RtpPort);
            Assert.Equal(1, transport.InterleavedChannels.RtcpPort);

            var session = response.Session;
            Assert.NotNull(session);
            Assert.NotEmpty(session.ID);
            Assert.Equal(60u, session.Timeout);

            var pipeline = MediaPipeline.CreateBuilder()
                                        .Source(_fixture.Client.GetChannelSource(transport.InterleavedChannels.RtpPort))
                                        .Transform(new DefaultRtpDepacketizer())
                                        .Sink(new Sink())
                                        .Build();

            pipeline.Start();

            response = _fixture.Client.Request().Session(session.ID).Play();
            Assert.True(response.ResponseStatus.Is(RtspResponse.Status.Ok));
            Thread.Sleep(20000);

            _fixture.Client.Request().Session(session.ID).TeardownAsync((res) => { });

            Console.WriteLine($"Sent {_spy.GetData(session.ID).Value.TotalPacketsSent} frames");
            pipeline.Stop();
        }

        private class Sink : ISink
        {
            private int _count;
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
                Console.WriteLine(++_count);
                Console.WriteLine(buffer.Length);

                return true;
            }
        }
    }
}
