//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.Pipeline;
using System;

namespace Pelco.Media.RTP
{
    public class DefaultRtpClock : IRtpClock
    {
        private uint _clockRate;
        private RtpTimestamp _rtpTimestamp;
        private DateTime _streamClockReference;

        public DefaultRtpClock(uint clockRate)
        {
            _clockRate = clockRate;
            _rtpTimestamp = new RtpTimestamp();
        }

        public IClockInstant Clock(ByteBuffer buffer)
        {
            return new RtpClockInstant(buffer, _clockRate, _rtpTimestamp, GetTimestamp, ApplyTimestamp);
        }

        private void ApplyTimestamp(DateTime ts)
        {
            _streamClockReference = ts;
        }

        private DateTime GetTimestamp()
        {
            return _streamClockReference;
        }

        private sealed class RtpClockInstant : IClockInstant
        {
            public delegate DateTime GetReferenceTimestamp();
            public delegate void SetReferenceTimestamp(DateTime ts);

            private bool _first = true;
            private bool _reset = true;
            private ByteBuffer _buffer;
            private RtpTimestamp _rtpTimestamp;

            public RtpClockInstant(ByteBuffer buffer,
                                   uint clockRate,
                                   RtpTimestamp ts,
                                   GetReferenceTimestamp getRefTs,
                                   SetReferenceTimestamp setRefTs)
            {
                _buffer = buffer;
                _rtpTimestamp = ts;

                if (_reset)
                {
                    _reset = false;
                    setRefTs(_buffer.TimeReference);
                    _rtpTimestamp.Next(1);
                    _rtpTimestamp.Reset();
                }
                else
                {
                    var delta = getRefTs() - _buffer.TimeReference;
                    _rtpTimestamp.At((long)TimeSpan.FromMilliseconds(delta.TotalMilliseconds * clockRate).TotalSeconds);
                }
            }

            public void Apply(RtpPacket packet)
            {
                if (_first)
                {
                    _first = false;

                    var onvifHeader = new OnvifRtpHeader()
                    {
                        CbitSet = true,
                        Time = _buffer.TimeReference
                    };

                    packet.HasExtensionHeader = true;
                    packet.ExtensionHeaderData = OnvifRtpHeader.PROFILE_ID;
                    packet.ExtensionData = onvifHeader.Encode();
                }

                packet.Timestamp = _rtpTimestamp.Current;
            }
        }
    }
}
