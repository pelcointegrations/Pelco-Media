//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.RTSP.Server;
using Pelco.Media.RTSP;
using Pelco.Media.RTSP.SDP;

namespace Pelco.Media.Tests.Integrations.Handlers
{
    class TestRequestHandler : RequestHandlerBase
    {
        public const string CALLED_METHOD_HEADER = "Called-Method";

        public override RtspResponse Describe(RtspRequest request)
        {
            var origin = SessionOriginator.CreateBuilder()
                                          .Username("-")
                                          .SessionId(1)
                                          .SessionVersion(1)
                                          .NetType(NetworkType.IN)
                                          .AddrType(AddressType.IP4)
                                          .UnicastAddress(request.RemoteEndpoint.Address.ToString())
                                          .Build();

            var connection = ConnectionInfo.CreateBuilder()
                                           .NetType(NetworkType.IN)
                                           .AddrType(AddressType.IP4)
                                           .Address("0.0.0.0")
                                           .Build();

            var media = MediaDescription.CreateBuilder()
                                        .MediaType(MediaType.APPLICATION)
                                        .Port(0)
                                        .Protocol(TransportProtocol.RTP_AVP)
                                        .AddFormat(98)
                                        .AddAttribute(new Attribute("control", request.URI.ToString()))
                                        .AddAttribute(new Attribute("rtpmap", "98 application/vnd.pelco.test.metadata/90000"))
                                        .Build();

            var sdp = new SessionDescription()
            {
                SessionInformation = "Test session",
                Origin = origin,
                Connection = connection,
            };

            sdp.MediaDescriptions.Add(media);

            return RtspResponse.CreateBuilder()
                               .Status(RtspResponse.Status.Ok)
                               .Body(sdp)
                               .Build();
        }

        public override RtspResponse GetParamater(RtspRequest request)
        {
            return RtspResponse.CreateBuilder()
                               .Status(RtspResponse.Status.Ok)
                               .AddHeader(RtspHeaders.Names.SESSION, request.Session)
                               .AddHeader(CALLED_METHOD_HEADER, "GET_PARAMATER")
                               .Build();
        }

        public override RtspResponse Play(RtspRequest request)
        {
            return RtspResponse.CreateBuilder()
                              .Status(RtspResponse.Status.Ok)
                              .AddHeader(CALLED_METHOD_HEADER, "PLAY")
                              .Build();
        }

        public override RtspResponse SetUp(RtspRequest request)
        {
            return RtspResponse.CreateBuilder()
                               .Status(RtspResponse.Status.Ok)
                               .AddHeader(CALLED_METHOD_HEADER, "SETUP")
                               .Build();
        }

        public override RtspResponse TearDown(RtspRequest request)
        {
            return RtspResponse.CreateBuilder()
                               .Status(RtspResponse.Status.Ok)
                               .AddHeader(CALLED_METHOD_HEADER, "TEARDOWN")
                               .Build(); ;
        }
    }
}
