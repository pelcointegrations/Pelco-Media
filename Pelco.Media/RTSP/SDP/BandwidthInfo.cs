//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Pelco.Media.RTSP.SDP
{
    public class BandwidthInfo
    {
        private static readonly Regex REGEX = new Regex(@"^b\s*=\s*(CT|AS)\s*:\s*(\d+)", RegexOptions.Compiled);

        public enum Type
        {
            CT,
            AS,
            UNKNOWN,
        }

        public BandwidthInfo() : this(Type.UNKNOWN, 0)
        {

        }

        public BandwidthInfo(Type type, long bandwidth)
        {
            BandwidthType = type;
            Value = bandwidth;
        }

        public Type BandwidthType { get; set; }

        public long Value { get; set; }

        public override string ToString()
        {
            return new StringBuilder("b=").Append(BandwidthType).Append(':').Append(Value).ToString();
        }

        public static BandwidthInfo Parse(string line)
        {
            var match = REGEX.Match(line);
            
            if (!match.Success)
            {
                throw new SdpParseException($"Unable to parse malformed bandwidth '{line}'");
            }

            long bw = 0;
            if (!long.TryParse(match.Groups[2].Value, out bw))
            {
                throw new SdpParseException($"Unable to parse bandwidth value {match.Groups[2].Value}");
            }

            Type type = Type.UNKNOWN;
            Enum.TryParse<Type>(match.Groups[1].Value, out type);

            return new BandwidthInfo(type, bw);
        }
    }
}
