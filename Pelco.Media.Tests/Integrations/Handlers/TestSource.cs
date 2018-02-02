//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.Pipeline;
using Pelco.Media.Tests.Utils;
using System.Threading;

namespace Pelco.Media.Tests.Integrations.Handlers
{
    /// <summary>
    /// Test data source to generate fake data to send over an RTP to
    /// a client.
    /// </summary>
    class TestSource : SourceBase
    {
        public const int DATA_SIZE = 7500; //bytes

        private uint _iterations;

        public TestSource(uint iterations = 1)
        {
            _iterations = iterations;
        }

        public override void Start()
        {
            Thread.Sleep(1000); // Wait a bit before starting to send.

            for (int i = 0; i < _iterations; ++i)
            {
                var randData = RandomUtils.RandomBytes(DATA_SIZE);
                PushBuffer(randData);
                Thread.Sleep(40);
            }
        }
    }
}
