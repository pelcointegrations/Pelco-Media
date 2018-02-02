//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.RTSP;
using System;

namespace Pelco.Media.Pipeline.Sinks
{
    /// <summary>
    /// A <see cref="MediaPipeline"/> sink used to send interleaved RTP packets.
    /// Currently does not support RTCP.
    /// </summary>
    public class TcpInterleavedSink : SinkBase
    {
        private const byte INTERLEAVED_MARKER = 0X24;

        private byte _channel;
        private RequestContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">The RTSP Request server contecxt to write to</param>
        /// <param name="channel">The RTP interleaved channel id</param>
        public TcpInterleavedSink(RequestContext context, byte channel)
        {
            _channel = channel;
            _context = context ?? throw new ArgumentNullException("Context cannot be null");
        }

        public override bool WriteBuffer(ByteBuffer buffer)
        {
            // Creating buffer to hold rtsp packet. $<channel id>{2 byte length}{RTP packet}
            var packet = new ByteBuffer(4 + buffer.Length);
            packet.WriteByte(INTERLEAVED_MARKER);
            packet.WriteByte(_channel);
            packet.WriteUInt16NetworkOrder((UInt16)buffer.Length);
            packet.Write(buffer);

            //return true;
            return _context.Write(packet);
        }
    }
}
