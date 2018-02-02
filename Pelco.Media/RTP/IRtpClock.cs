//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.Pipeline;

namespace Pelco.Media.RTP
{
    public interface IClockInstant
    {
        void Apply(RtpPacket packet);
    }

    /// <summary>
    /// Clock used to convert from absolute times to RTP times.
    /// </summary>
    public interface IRtpClock
    {
        /// <summary>
        /// Returns a <see cref="IClockInstant"/> object from the provided buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        IClockInstant Clock(ByteBuffer buffer);
    }
}
