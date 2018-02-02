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
    public class RtspException : Exception
    {
        public RtspException() : base()
        {

        }

        public RtspException(string msg) : base(msg)
        {

        }

        public RtspException(string msg, Exception cause) : base(msg, cause)
        {

        }
    }
}
