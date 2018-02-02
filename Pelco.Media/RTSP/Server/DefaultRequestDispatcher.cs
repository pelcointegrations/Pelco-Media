//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using NLog;
using System;
using System.Collections.Generic;

namespace Pelco.Media.RTSP.Server
{
    public class DefaultRequestDispatcher : IRequestDispatcher
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private Dictionary<string, IRequestHandler> _handlers;

        /// <summary>
        /// Constructor
        /// </summary>
        public DefaultRequestDispatcher()
        {
            _handlers = new Dictionary<string, IRequestHandler>();
        }

        /// <summary>
        /// <see cref="IRequestDispatcher.Init"/>
        /// </summary>
        public virtual void Init()
        {
            foreach (var handler in _handlers)
            {
                try
                {
                    handler.Value.Init();
                }
                catch (Exception e)
                {
                    LOG.Error(e, $"Causght exception while initalizing '{handler.GetType().Name}'");
                }
            }
        }

        /// <summary>
        /// <see cref="IRequestDispatcher.Close"/>
        /// </summary>
        public virtual void Close()
        {
            foreach (var handler in _handlers)
            {
                try
                {
                    handler.Value.Close();
                }
                catch (Exception e)
                {
                    LOG.Error(e, $"Causght exception while closing '{handler.GetType().Name}'");
                }
            }
        }

        /// <summary>
        /// Registers an <see cref="IRequestHandler"/> with this dispatcher. The path must be the complete path
        /// of the URI.  This dispatcher does not support wildcard matches.
        /// </summary>
        /// <param name="path">The path used to look up the handler</param>
        /// <param name="handler">The handler used to process the RTSP request</param>
        public void RegisterHandler(string path, IRequestHandler handler)
        {
            if (!_handlers.ContainsKey(path))
            {
                _handlers.Add(path, handler);
            }
        }

        /// <summary>
        /// Dispatches an RTSP request.
        /// </summary>
        /// <param name="request">The request to dispatch</param>
        /// <returns></returns>
        public RtspResponse Dispatch(RtspRequest request)
        {
            IRequestHandler handler = null;

            if (_handlers.TryGetValue(request.URI.AbsolutePath, out handler))
            {
                switch (request.Method)
                {
                    case RtspRequest.RtspMethod.ANNOUNCE: return handler.Announce(request);
                    case RtspRequest.RtspMethod.DESCRIBE: return handler.Describe(request);
                    case RtspRequest.RtspMethod.GET_PARAMETER: return handler.GetParamater(request);
                    case RtspRequest.RtspMethod.OPTIONS: return handler.Options(request);
                    case RtspRequest.RtspMethod.PAUSE: return handler.Pause(request);
                    case RtspRequest.RtspMethod.PLAY: return handler.Play(request);
                    case RtspRequest.RtspMethod.RECORD: return handler.Record(request);
                    case RtspRequest.RtspMethod.REDIRECT: return handler.Redirect(request);
                    case RtspRequest.RtspMethod.SETUP: return handler.SetUp(request);
                    case RtspRequest.RtspMethod.SET_PARAMETER: return handler.SetParamater(request);
                    case RtspRequest.RtspMethod.TEARDOWN: return handler.TearDown(request);

                    default: return RtspResponse.CreateBuilder().Status(RtspResponse.Status.MethodNotAllowed).Build();
                }
            }
            else
            {
                return RtspResponse.CreateBuilder().Status(RtspResponse.Status.NotFound).Build();
            }
        }
    }
}
