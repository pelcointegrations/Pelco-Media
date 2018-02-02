//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.RTP;

namespace Pelco.Media.Pipeline.Transforms
{
    public class AlwaysFalseDemarcator : IRtpDemarcator
    {
        public bool Check(RtpPacket packet)
        {
            return false;
        }
    }
}
