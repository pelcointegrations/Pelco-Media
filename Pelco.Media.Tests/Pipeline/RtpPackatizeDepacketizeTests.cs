using Pelco.Media.Pipeline;
using Pelco.Media.Pipeline.Transforms;
using Pelco.Media.RTP;
using Pelco.Media.Tests.Utils;
using System;
using System.Threading;
using Xunit;

namespace Pelco.Media.Tests.Pipeline
{
    public class RtpPackatizeDepacketizeTests
    {
        [Fact]
        public void TestPacketizeDepacketize()
        {
            var src = new Source();
            var sink = new Sink();
            var pipeline = MediaPipeline.CreateBuilder()
                                        .Source(src)
                                        .Transform(new RtpPacketizer(new DefaultRtpClock(90000), 5678, 96))
                                        .Transform(new DefaultRtpDepacketizer())
                                        .Sink(sink)
                                        .Build();

            pipeline.Start();

            while (sink.ReceivedBuffer == ByteBuffer.EMPTY)
            {
                Thread.Sleep(500);
            }

            pipeline.Stop();

            Assert.True(sink.ReceivedBuffer.Equals(src.SentBuffer));
        }

        private sealed class Source : SourceBase
        {
            public Source()
            {
                SentBuffer = ByteBuffer.EMPTY;
            }

            public ByteBuffer SentBuffer { get; private set; }

            public override void Start()
            {
                SentBuffer = RandomUtils.RandomBytes(14000);
                SentBuffer.TimeReference = DateTime.Now;
                PushBuffer(SentBuffer);
            }
        }

        private sealed class Sink : SinkBase
        {
            public Sink()
            {
                ReceivedBuffer = ByteBuffer.EMPTY;
            }

            public ByteBuffer ReceivedBuffer { get; set; }

            public override bool WriteBuffer(ByteBuffer buffer)
            {
                ReceivedBuffer = buffer;

                return true;
            }
        }
    }
}
