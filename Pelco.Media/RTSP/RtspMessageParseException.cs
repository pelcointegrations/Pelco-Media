//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using System;

namespace Pelco.Media.RTSP
{
    class RtspMessageParseException : Exception
    {
        public RtspMessageParseException(string msg) : base(msg)
        {

        }

        public RtspMessageParseException(string msg, Exception cause) : base(msg, cause)
        {

        }
    }
}
