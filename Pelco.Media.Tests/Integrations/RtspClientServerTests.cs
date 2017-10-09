using Pelco.Media.RTSP;
using Pelco.Media.Tests.Utils;
using Xunit;

namespace Pelco.Media.Tests.Integrations
{
    public class RtspClientServerTests : IClassFixture<RtspCommunicationFixture> 
    {
        private RtspCommunicationFixture _fixture;

        public RtspClientServerTests(RtspCommunicationFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void TestOptionsCall()
        {
            int cseq = _fixture.NextCseq();

            var request = RtspRequest.CreateBuilder()
                                     .Uri(_fixture.ServerUriEndpoint)
                                     .Method(RtspRequest.RtspMethod.OPTIONS)
                                     .AddHeader(RtspHeaders.Names.CSEQ, cseq.ToString())
                                     .Build();

            var response = _fixture.Client.Send(request);

            Assert.True(response.ResponseStatus.Is(RtspResponse.Status.Ok));
            Assert.Equal(cseq.ToString(), response.Headers[RtspHeaders.Names.CSEQ]);
            Assert.Equal("OPTIONS, DESCRIBE, GET_PARAMATER, SETUP, PLAY, TEARDOWN", response.Headers[RtspHeaders.Names.PUBLIC]);
        }

        [Fact]
        public void TestGetParamaterCall()
        {
            int cseq = _fixture.NextCseq();

            var request = RtspRequest.CreateBuilder()
                                     .Uri(_fixture.ServerUriEndpoint)
                                     .Method(RtspRequest.RtspMethod.GET_PARAMETER)
                                     .AddHeader(RtspHeaders.Names.CSEQ, cseq.ToString())
                                     .Build();

            var response = _fixture.Client.Send(request);

            Assert.True(response.ResponseStatus.Is(RtspResponse.Status.Ok));
            Assert.Equal(cseq.ToString(), response.Headers[RtspHeaders.Names.CSEQ]);
            Assert.Equal("GET_PARAMATER", response.Headers[TestRequestHandler.CALLED_METHOD_HEADER]);
        }

        [Fact]
        public void TestPlayCall()
        {
            int cseq = _fixture.NextCseq();

            var request = RtspRequest.CreateBuilder()
                                     .Uri(_fixture.ServerUriEndpoint)
                                     .Method(RtspRequest.RtspMethod.PLAY)
                                     .AddHeader(RtspHeaders.Names.CSEQ, cseq.ToString())
                                     .Build();

            var response = _fixture.Client.Send(request);

            Assert.True(response.ResponseStatus.Is(RtspResponse.Status.Ok));
            Assert.Equal(cseq.ToString(), response.Headers[RtspHeaders.Names.CSEQ]);
            Assert.Equal("PLAY", response.Headers[TestRequestHandler.CALLED_METHOD_HEADER]);
        }

        [Fact]
        public void TestSetupCall()
        {
            int cseq = _fixture.NextCseq();

            var request = RtspRequest.CreateBuilder()
                                     .Uri(_fixture.ServerUriEndpoint)
                                     .Method(RtspRequest.RtspMethod.SETUP)
                                     .AddHeader(RtspHeaders.Names.CSEQ, cseq.ToString())
                                     .Build();

            var response = _fixture.Client.Send(request);

            Assert.True(response.ResponseStatus.Is(RtspResponse.Status.Ok));
            Assert.Equal(cseq.ToString(), response.Headers[RtspHeaders.Names.CSEQ]);
            Assert.Equal("SETUP", response.Headers[TestRequestHandler.CALLED_METHOD_HEADER]);
        }

        [Fact]
        public void TestTeardownCall()
        {
            int cseq = _fixture.NextCseq();

            var request = RtspRequest.CreateBuilder()
                                     .Uri(_fixture.ServerUriEndpoint)
                                     .Method(RtspRequest.RtspMethod.TEARDOWN)
                                     .AddHeader(RtspHeaders.Names.CSEQ, cseq.ToString())
                                     .Build();

            var response = _fixture.Client.Send(request);

            Assert.True(response.ResponseStatus.Is(RtspResponse.Status.Ok));
            Assert.Equal(cseq.ToString(), response.Headers[RtspHeaders.Names.CSEQ]);
            Assert.Equal("TEARDOWN", response.Headers[TestRequestHandler.CALLED_METHOD_HEADER]);
        }
    }
}
