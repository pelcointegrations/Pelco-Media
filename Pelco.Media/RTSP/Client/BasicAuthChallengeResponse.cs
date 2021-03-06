﻿//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.Common;
using System;
using System.Text;

namespace Pelco.Media.RTSP.Client
{
    /// <summary>
    /// A <see cref="ChallengeResponse"/> of type Basic.  This class will generate an
    /// Authentication header value as defined in Section 2 of RFC 2617.
    /// </summary>
    public class BasicAuthChallengeResponse : ChallengeResponse
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="creds"></param>
        /// <param name="realm"></param>
        internal BasicAuthChallengeResponse(Credentials creds, string realm) : base(creds, realm)
        {
        }

        /// <summary>
        /// <see cref="ChallengeResponse.ChallengeType"/>
        /// </summary>
        public override Type ChallengeType
        {
            get
            {
                return Type.Basic;
            }
        }

        /// <summary>
        /// <see cref="ChallengeResponse.Generate(RtspRequest.RtspMethod, Uri)"/>
        /// </summary>
        public override string Generate(RtspRequest.RtspMethod method, Uri uri)
        {
            string auth = $"{Credentials.Username}:{Credentials.Password}";

            return $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(auth))}";
        }
    }
}
