//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using System;

namespace Pelco.Media.RTP
{
    public class RtpTimestamp
    {
        private const long MAX_TS = uint.MaxValue;

        private uint _current;
        private uint _initial;

        public RtpTimestamp()
        {
            _current = RandomValue();
            _initial = _current;
        }

        public uint Current
        {
            get
            {
                return _current;
            }
        }

        public uint Next(uint increment)
        {
            lock (this)
            {
                uint partial = increment % uint.MaxValue;
                _current += partial;
                _current %= uint.MaxValue;
                return _current;
            }
        }

        public uint At(long delta)
        {
            lock (this)
            {
                if (delta > 0)
                {
                    uint partial = (uint)(delta % MAX_TS);
                    _current = (_initial + partial) % uint.MaxValue;
                }

                return _current;
            }
        }

        public void Reset()
        {
            lock (this)
            {
                _initial = _current;
            }
        }

        private uint RandomValue()
        {
            // Generate a random uint value
            var rand = new Random();
            return (uint)(rand.Next(1 << 30)) << 2 | (uint)(rand.Next(1 << 2));
        }
    }
}
