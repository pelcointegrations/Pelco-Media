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
    public class RtspMessageEventArgs : EventArgs
    {
        public RtspMessageEventArgs(RtspMessage message)
        {
            Message = message;
        }

        public RtspMessage Message { get; private set; }
    }
}
