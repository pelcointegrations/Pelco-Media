//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using NLog;
using Pelco.Media.RTP;
using System;
using System.Collections.Generic;

namespace Pelco.Media.Pipeline.Transforms
{
    /// <summary>
    /// Transform that slices an incoming buffer into mtu sized RTP encoded packets.
    /// If an incoming packets contains a TimeReference then the first encoded RTP
    /// packet will contains the Onvif RTP header extension set.
    /// </summary>
    public class RtpPacketizer : TransformBase
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        private const int DEFAULT_MTU = 1400;

        private int _mtu;
        private uint _ssrc;
        private ushort _seqNum;
        private byte _payloadType;
        private IRtpClock _rtpClock;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clock">Th rtp clock instance to use for generating RTP timestamps</param>
        /// <param name="ssrc">The ssrc to use</param>
        /// <param name="payloadType">The RTP payload type</param>
        /// <param name="mtu">The size of the mtu packets. Defaults to 1400</param>
        public RtpPacketizer(IRtpClock clock, uint ssrc, byte payloadType, int mtu = DEFAULT_MTU)
        {
            var rand = new Random();

            _mtu = mtu;
            _ssrc = ssrc;
            _rtpClock = clock ?? throw new ArgumentNullException("Clock cannot be null");
            _seqNum = (ushort)rand.Next(0, ushort.MaxValue);
            _payloadType = payloadType;
        }

        /// <summary>
        /// <see cref="TransformBase.WriteBuffer(ByteBuffer)"/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public override bool WriteBuffer(ByteBuffer buffer)
        {
            var instant = _rtpClock.Clock(buffer);

            var slices = SliceData(buffer);

            for (int i = 0; i < slices.Count; ++i)
            {
                var slice = slices[i];
                var packet = new RtpPacket()
                {
                    Version = RtpVersion.V2,
                    Payload = slice,
                    PayloadType = _payloadType,
                    SequenceNumber = ++_seqNum,
                    SSRC = _ssrc,
                    Marker = i == (slices.Count - 1),
                };

                instant.Apply(packet);
                PushBuffer(packet.Encode());
            }

            return true;
        }

        protected virtual List<ByteBuffer> SliceData(ByteBuffer buffer)
        {
            List<ByteBuffer> slices = new List<ByteBuffer>(buffer.RemainingBytes / _mtu + 1);

            while (buffer.RemainingBytes > 0)
            {
                var slice = buffer.ReadSlice(Math.Min(buffer.RemainingBytes, _mtu));
                slices.Add(slice);
            }

            return slices;
        }
    }
}
