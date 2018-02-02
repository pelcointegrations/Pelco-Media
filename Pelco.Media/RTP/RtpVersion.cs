//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
namespace Pelco.Media.RTP
{
    public class RtpVersion
    {
        public static readonly RtpVersion UNDEFINED = new RtpVersion(0);
        public static readonly RtpVersion V1 = new RtpVersion(1);
        public static readonly RtpVersion V2 = new RtpVersion(2);
        public static readonly RtpVersion V3 = new RtpVersion(3);

        private byte _value;

        private RtpVersion(byte value)
        {
            _value = value;
        }

        public byte Value()
        {
            return _value;
        }

        public bool Is(RtpVersion other)
        {
            return other._value == _value;
        }

        public static RtpVersion FromByte(byte value)
        {
            byte version = (byte)((value & 0xC0) >> 6);

            switch(version)
            {
                case 1 : return V1;
                case 2 : return V2;
                case 3 : return V3;
                default: return UNDEFINED;
            }
        }
    }
}
