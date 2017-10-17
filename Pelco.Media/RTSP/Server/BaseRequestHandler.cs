namespace Pelco.Media.RTSP.Server
{
    /// <summary>
    /// Base <see cref="IRequestHandler"/> for handling RTSP requests.
    /// </summary>
    public abstract class BaseRequestHandler : IRequestHandler
    {
        public abstract RtspResponse Describe(RtspRequest request);

        public abstract RtspResponse GetParamater(RtspRequest request);

        public abstract RtspResponse Play(RtspRequest request);

        public abstract RtspResponse SetUp(RtspRequest request);

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
                               .AddHeader(RtspHeaders.Names.PUBLIC, "OPTIONS, DESCRIBE, GET_PARAMATER, SETUP, PLAY, TEARDOWN")
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
