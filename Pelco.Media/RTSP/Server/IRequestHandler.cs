//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
namespace Pelco.Media.RTSP.Server
{
    /// <summary>
    /// Interfaced used to defined a request handler for handling RTSP server requests.
    /// </summary>
    public interface IRequestHandler
    {
        /// <summary>
        /// Allows extensions to initialize resources if requried.
        /// </summary>
        void Init();

        /// <summary>
        /// Allows extensions to closed created resources if required.
        /// </summary>
        void Close();

        RtspResponse Announce(RtspRequest request);

        RtspResponse Describe(RtspRequest request);

        RtspResponse GetParamater(RtspRequest request);

        RtspResponse Options(RtspRequest request);

        RtspResponse Pause(RtspRequest request);

        RtspResponse Play(RtspRequest request);

        RtspResponse Record(RtspRequest request);

        RtspResponse Redirect(RtspRequest request);

        RtspResponse SetParamater(RtspRequest request);

        RtspResponse SetUp(RtspRequest request);

        RtspResponse TearDown(RtspRequest request);
       
    }
}
