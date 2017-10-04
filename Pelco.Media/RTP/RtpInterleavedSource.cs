using NLog;
using Pelco.PDK.Media.Pipeline;
using Pelco.PDK.Media.RTSP;

namespace Pelco.PDK.Media.RTP
{
    public class RtpInterleavedSource : IRtpSource
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        public RtpInterleavedSource(RtpInterleaveMediaSource rtpSource, RtpInterleaveMediaSource rtcpSource)
        {
            RtpSource = rtpSource;
            RtcpSource = rtcpSource;
            RtpPort = rtpSource.Channel;
            RtcpPort = rtcpSource.Channel;

            LOG.Debug($"Created RtpInterleavedSource with channels {rtpSource.Channel}-{rtcpSource.Channel}");
        }

        public int RtpPort { get; private set; }

        public int RtcpPort { get; private set; }

        public ISource RtpSource { get; private set; }

        public ISource RtcpSource { get; private set; }

        public void Start()
        {
            // nothing to do.
        }

        public void Stop()
        {
            // nothing to do.
        }
    }
}
