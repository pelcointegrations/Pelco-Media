//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.Pipeline;

namespace Pelco.Media.RTSP
{
    /// <summary>
    /// Represents an interleaved RTP/RTCP data source for a <see cref="MediaPipeline"/>
    /// </summary>
    public sealed class RtpInterleaveMediaSource : SourceBase
    {
        public RtpInterleaveMediaSource(int channel)
        {
            Channel = channel;
        }

        /// <summary>
        /// Gets the channel associated with the source.
        /// </summary>
        public int Channel { get; private set; }

        /// <summary>
        /// Writes the interleaved buffer causing it to be pushed upstream.
        /// </summary>
        /// <param name="buffer"></param>
        public void WriteBuffer(ByteBuffer buffer)
        {
            PushBuffer(buffer);
        }
    }
}
