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

namespace Pelco.Media.RTSP
{
    public class RtspVersion
    {
        /// <summary>
        /// Rtsp version 1.0
        /// </summary>
        public static readonly RtspVersion RTSP_1_0 = new RtspVersion("RTSP", 1, 0);

        /// <summary>
        /// Rtsp version 1.1
        /// </summary>
        public static readonly RtspVersion RTSP_1_1 = new RtspVersion("RTSP", 1, 1);

        private static readonly Regex VERSION_PATTERN = new Regex(@"(\S+)/(\d+)\.(\d+)");

        private static readonly string RTSP_1_0_STRING = "RTSP/1.0";
        private static readonly string RTSP_1_1_STRING = "RTSP/1.1";

        public RtspVersion(string protocolName, int majorVersion, int minorVersion)
        {
            if (protocolName == null)
            {
                throw new ArgumentNullException("Protocol name cannot be null");
            }

            ProtocolName = protocolName;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
        }

        private RtspVersion(string text)
        {
            text = text.Trim().ToUpper();

            var match = VERSION_PATTERN.Match(text);
            if (!match.Success)
            {
                throw new ArgumentException($"Invalid RTSP version format: {text}");
            }

            ProtocolName = match.Groups[1].Value;
            MajorVersion = int.Parse(match.Groups[2].Value);
            MinorVersion = int.Parse(match.Groups[2].Value);
        }

        #region Properties

        public string ProtocolName { get; private set; }

        public int MajorVersion { get; private set; }

        public int MinorVersion { get; private set; }

        #endregion

        public override string ToString()
        {
            return new StringBuilder(ProtocolName).Append('/')
                                                  .Append(MajorVersion)
                                                  .Append('.')
                                                  .Append(MinorVersion).ToString();
        }

        public static RtspVersion Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException("Cannot parse null or empty RTSP version text");
            }

            text = text.Trim();

            RtspVersion version = FromWellKnown(text);
            if (version == null)
            {
                version =  new RtspVersion(text);
            }

            return version;
        }

        private static RtspVersion FromWellKnown(string text)
        {
            if (RTSP_1_0_STRING.Equals(text))
            {
                return RTSP_1_0;
            }
            else if (RTSP_1_1_STRING.Equals(text))
            {
                return RTSP_1_1;
            }

            return null;
        }
    }
}
