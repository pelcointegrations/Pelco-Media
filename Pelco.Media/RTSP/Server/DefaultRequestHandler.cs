//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.RTSP.SDP;

namespace Pelco.Media.RTSP.Server
{
    /// <summary>
    /// Default RTSP handler implementation. The handler will provide base support for
    /// most functionality needed for a base RTSP handler.
    /// </summary>
    public abstract class DefaultRequestHandler : RequestHandlerBase
    {
        protected IRtspSessionManager _sessionManager;

        /// <summary>
        /// Constructor. Creates a handler that uses a <see cref="RtspSessionManager"/>.
        /// </summary>
        protected DefaultRequestHandler() : this(new RtspSessionManager())
        {
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mgr">The <see cref="IRtspSessionManager"/> to use.</param>
        protected DefaultRequestHandler(IRtspSessionManager mgr)
        {
            _sessionManager = mgr;
        }

        /// <summary>
        /// <see cref="RequestHandlerBase.Init"/>
        /// </summary>
        public override void Init()
        {
            _sessionManager.Start();
        }

        /// <summary>
        /// <see cref="RequestHandlerBase.Close"/>
        /// </summary>
        public override void Close()
        {
            _sessionManager.Stop();
        }

        /// <summary>
        /// <see cref="IRequestHandler.Describe(RtspRequest)"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override RtspResponse Describe(RtspRequest request)
        {
            return RtspResponse.CreateBuilder()
                               .Status(RtspResponse.Status.Ok)
                               .Body(CreateSDP(request))
                               .Build();
        }

        /// <summary>
        /// <see cref="IRequestHandler.GetParamater(RtspRequest)"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override RtspResponse GetParamater(RtspRequest request)
        {
            var builder = RtspResponse.CreateBuilder().Status(RtspResponse.Status.Ok);

            var sessionId = request.Session;
            if (string.IsNullOrEmpty(sessionId) || !_sessionManager.RefreshSession(sessionId))
            {
                return builder.Status(RtspResponse.Status.SessionNotFound).Build();
            }

            return builder.Build();
        }

        /// <summary>
        /// <see cref="IRequestHandler.Play(RtspRequest)"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override RtspResponse Play(RtspRequest request)
        {
            var builder = RtspResponse.CreateBuilder().Status(RtspResponse.Status.Ok);

            var sessionId = request.Session;
            if (string.IsNullOrEmpty(sessionId) || !_sessionManager.PlaySession(sessionId))
            {
                return builder.Status(RtspResponse.Status.SessionNotFound).Build();
            }

            return builder.Build(); ;
        }

        /// <summary>
        /// <see cref="IRequestHandler.TearDown(RtspRequest)"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override RtspResponse TearDown(RtspRequest request)
        {
            var builder = RtspResponse.CreateBuilder().Status(RtspResponse.Status.Ok);

            var sessionId = request.Session;
            if (string.IsNullOrEmpty(sessionId) || !_sessionManager.TearDownSession(sessionId))
            {
                return builder.Status(RtspResponse.Status.SessionNotFound).Build();
            }

            return builder.Build();
        }

        /// <summary>
        /// <see cref="IRequestHandler.SetUp(RtspRequest)"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract override RtspResponse SetUp(RtspRequest request);

        /// <summary>
        /// Creates the SDP to send back when a describe call is made.
        /// </summary>
        /// <param name="request">The incoming rtsp request</param>
        /// <returns></returns>
        protected abstract SessionDescription CreateSDP(RtspRequest request);
    }
}
