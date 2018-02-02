//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using System;

namespace Pelco.Media.Pipeline.Transforms
{
    /// <summary>
    /// Transform that filters out any RTP packets that do not contain
    /// the provided SSRC.
    /// </summary>
    public class SsrcFilter : TransformBase
    {
        private uint _ssrc;

        public SsrcFilter(string ssrc)
        {
            _ssrc = Convert.ToUInt32(ssrc, 16);
        }

        public override bool WriteBuffer(ByteBuffer buffer)
        {
            // TODO(frank.lamar): Actually implement the filtering.
            return PushBuffer(buffer);
        }
    }
}
