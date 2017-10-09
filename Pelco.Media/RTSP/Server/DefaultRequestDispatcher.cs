using System.Collections.Generic;

namespace Pelco.Media.RTSP.Server
{
    public class DefaultRequestDispatcher : IRequestDispatcher
    {
        private Dictionary<string, IRequestHandler> _handlers;

        public DefaultRequestDispatcher()
        {
            _handlers = new Dictionary<string, IRequestHandler>();
        }

        /// <summary>
        /// Registers an <see cref="IRequestHandler"/> with this dispatcher. The path must be the complete path
        /// of the URI.  This dispatcher does not support wildcard matches.
        /// </summary>
        /// <param name="path">The path used to look up the handler</param>
        /// <param name="handler">The handler used to process the RTSP request</param>
        public void RegisterListener(string path, IRequestHandler handler)
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
