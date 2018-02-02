//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using System;

namespace Pelco.Media.RTSP.SDP
{
    public class SdpParseException : Exception
    {
        public SdpParseException(string msg) : base(msg)
        {
        }

        public SdpParseException(string msg, Exception cause) : base(msg, cause)
        {
        }
    }
}
