//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.RTSP;
using Pelco.Media.RTSP.SDP;
using Pelco.Media.RTSP.Server;
using System;

namespace Pelco.Media.Tests.Integrations.Handlers
{
    public class InterleavedTestHandler : DefaultRequestHandler
    {
        private const int PAYLOAD_TYPE = 98;

        private SessionSpy _spy;
        private int _currentChannel = 0;

        public InterleavedTestHandler(SessionSpy spy) : base()
        {
            _spy = spy ?? throw new ArgumentNullException("Spy cannot be null");
        }

        public override RtspResponse SetUp(RtspRequest request)
        {
            var builder = RtspResponse.CreateBuilder().Status(RtspResponse.Status.Ok);

            var transport = request.Transport;
            if (transport == null)
            {
                return builder.Status(RtspResponse.Status.BadRequest).Build();
            }
            else if (transport.Type != TransportType.RtspInterleaved)
            {
                return builder.Status(RtspResponse.Status.UnsupportedTransport).Build();
            }

            lock (this)
            {
                PortPair channels = new PortPair(_currentChannel, _currentChannel + 1);
                _currentChannel += 2;

                var session = new InterleavedTestSession(request.Context, _spy, channels, PAYLOAD_TYPE);
                _sessionManager.RegisterSession(session);

                transport = TransportHeader.CreateBuilder()
                                           .Type(TransportType.RtspInterleaved)
                                           .InterleavedChannels(channels)
                                           .Build();

                return builder.AddHeader(RtspHeaders.Names.TRANSPORT, transport.ToString())
                              .AddHeader(RtspHeaders.Names.SESSION, session.Id)
                              .Build();
            }
        }

        protected override SessionDescription CreateSDP(RtspRequest request)
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
                                        .AddFormat(PAYLOAD_TYPE)
                                        .AddAttribute(new RTSP.SDP.Attribute("control", request.URI.ToString()))
                                        .AddAttribute(new RTSP.SDP.Attribute("rtpmap", $"{PAYLOAD_TYPE} application/vnd.pelco.test.metadata/90000"))
                                        .Build();

            var sdp = new SessionDescription()
            {
                SessionInformation = "Test session",
                Origin = origin,
                Connection = connection,
            };

            sdp.MediaDescriptions.Add(media);

            return sdp;
        }
    }
}
