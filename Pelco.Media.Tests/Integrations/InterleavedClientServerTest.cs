//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.Pipeline;
using Pelco.Media.Pipeline.Transforms;
using Pelco.Media.RTSP;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var sink = new Sink();
            var pipeline = MediaPipeline.CreateBuilder()
                                        .Source(_fixture.Client.GetChannelSource(transport.InterleavedChannels.RtpPort))
                                        .Transform(new DefaultRtpDepacketizer())
                                        .Sink(sink)
                                        .Build();

            pipeline.Start();

            _fixture.Client.Request().Session(session.ID).PlayAsync((res) =>
            {
                Assert.True(res.ResponseStatus.Is(RtspResponse.Status.Ok));
            });

            sink.WaitForCompletion(TimeSpan.FromSeconds(20));

            _fixture.Client.Request().Session(session.ID).TeardownAsync((res) => { });

            pipeline.Stop();

            var sessData = _spy.GetData(session.ID);
            Assert.Equal(sessData.Buffers.Count, sink.ReceivedBuffers.Count);
            Assert.True(Enumerable.SequenceEqual(sessData.Buffers, sink.ReceivedBuffers));
        }

        private class Sink : SinkBase
        {
            private ManualResetEvent _event;

            public Sink()
            {
                _event = new ManualResetEvent(false);

                ReceivedBuffers = new List<ByteBuffer>();
            }

            public List<ByteBuffer> ReceivedBuffers { get; private set; }

            public bool WaitForCompletion(TimeSpan timeout)
            {
                return _event.WaitOne(timeout);
            }

            public override bool WriteBuffer(ByteBuffer buffer)
            {
                ReceivedBuffers.Add(buffer);
                if (ReceivedBuffers.Count == InterleavedTestSession.FRAMES_TO_SEND)
                {
                    _event.Set(); // Notify waiting thread we received all requests.
                }

                return true;
            }
        }
    }
}
