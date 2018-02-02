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

namespace Pelco.Media.Pipeline.Transforms
{
    /// <summary>
    /// An RtpPayloadTransform processes an encoded RTP packet and pushes the
    /// provided packet payload upstream. If the packet contains a Onvif header extension
    /// the payload buffer's TimeReference will be set to the provided Onvif time.
    /// </summary>
    public class RtpPayloadTransform : TransformBase
    {
        private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Converts an RTP packet into an the RTP payload.  Basically it just strips off
        /// the RTP transport info and returns that actual payload data.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public override bool WriteBuffer(ByteBuffer buffer)
        {
            try
            {
                var packet = RtpPacket.Decode(buffer);
                var paylaod = packet.Payload;

                if (packet.HasExtensionHeader && packet.ExtensionHeaderData == OnvifRtpHeader.PROFILE_ID)
                {
                    // Packet contains Onvif header extension set packet time reference.
                    var onvifHdr = OnvifRtpHeader.Decode(packet.ExtensionData);
                    paylaod.TimeReference = onvifHdr.Time;
                }

                return PushBuffer(paylaod);
            }
            catch (Exception e)
            {
                LOG.Error($"Unable to decode RTP packet, reason: {e.Message}");
                return false;
            }
        }
    }
}
