//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using System;

namespace Pelco.Media.Pipeline
{
    /// <summary>
    /// Reports that th end of the <see cref="ByteBuffer"/> has been meet.
    /// </summary>
    class EndOfBufferException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public EndOfBufferException() : base("End of Pipeline.Buffer reached")
        {

        }
    }
}