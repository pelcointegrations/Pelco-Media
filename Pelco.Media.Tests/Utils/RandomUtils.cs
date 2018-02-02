//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.Pipeline;
using System;

namespace Pelco.Media.Tests.Utils
{
    public class RandomUtils
    {
        public static ByteBuffer RandomBytes(int capacity)
        {
            var rand = new Random();
            var bytes = new byte[capacity];

            rand.NextBytes(bytes);

            return new ByteBuffer(bytes, 0, capacity, true);
        }
    }
}
