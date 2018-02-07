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
    /// Base <see cref="IRequestHandler"/> for handling RTSP requests.
    /// </summary>
    public abstract class RequestHandlerBase : IRequestHandler
    {
        /// <summary>
        /// <see cref="IRequestHandler.Describe(RtspRequest)"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract RtspResponse Describe(RtspRequest request);

        /// <summary>
        /// <see cref="IRequestHandler.GetParamater(RtspRequest)"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract RtspResponse GetParamater(RtspRequest request);

        /// <summary>
        /// <see cref="IRequestHandler.Play(RtspRequest)"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract RtspResponse Play(RtspRequest request);

        /// <summary>
        /// <see cref="IRequestHandler.SetUp(RtspRequest)"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract RtspResponse SetUp(RtspRequest request);

        /// <summary>
        /// <see cref="IRequestHandler.TearDown(RtspRequest)"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract RtspResponse TearDown(RtspRequest request);
        
        /// <summary>
        /// <see cref="IRequestHandler.Init"/>
        /// </summary>
        public virtual void Init()
        {
        }
        
        /// <summary>
        /// <see cref="IRequestHandler.Close"/>
        /// </summary>
        public virtual void Close()
        {
        }

        public RtspResponse Options(RtspRequest request)
        {
            return RtspResponse.CreateBuilder()
                               .Status(RtspResponse.Status.Ok)
                               .AddHeader(RtspHeaders.Names.PUBLIC, "OPTIONS, DESCRIBE, GET_PARAMETER, SETUP, PLAY, TEARDOWN")
                               .Build();
        }

        virtual public RtspResponse Announce(RtspRequest request)
        {
            return RtspResponse.CreateBuilder().Status(RtspResponse.Status.MethodNotAllowed).Build();
        }

        virtual public RtspResponse Pause(RtspRequest request)
        {
            return RtspResponse.CreateBuilder().Status(RtspResponse.Status.MethodNotAllowed).Build();
        }

        virtual public RtspResponse Record(RtspRequest request)
        {
            return RtspResponse.CreateBuilder().Status(RtspResponse.Status.MethodNotAllowed).Build();
        }

        virtual public RtspResponse Redirect(RtspRequest request)
        {
            return RtspResponse.CreateBuilder().Status(RtspResponse.Status.MethodNotAllowed).Build();
        }

        virtual public RtspResponse SetParamater(RtspRequest request)
        {
            return RtspResponse.CreateBuilder().Status(RtspResponse.Status.MethodNotAllowed).Build();
        }
    }
}
