//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using NLog;
using Pelco.Media.Pipeline;
using Pelco.Media.RTSP;

namespace Pelco.Media.RTP
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
