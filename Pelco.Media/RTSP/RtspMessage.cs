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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Pelco.Media.RTSP
{
    public class RtspMessage : RtspChunk, ICloneable
    {
        protected static readonly string CRLF = "\r\n";

        private static Logger LOG = LogManager.GetCurrentClassLogger();

        private static readonly Regex StartLineTest = new Regex(@"^RTSP/\d\.\d", RegexOptions.Compiled);

        private Dictionary<string, string> _headers;

        protected RtspMessage(RtspVersion version)
        {
            _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            Body = new byte[0];
            Version = version;
        }

        public static RtspMessage CreateNewMessage(string[] parts, IPEndPoint endpoint)
        {
            if (parts == null || parts.Length < 3)
            {
                throw new RtspMessageParseException("Unable to parse invalid RtspMessage.");
            }

            try
            {
                if (StartLineTest.IsMatch(parts[2]))
                {
                    LOG.Debug($"Detected RtspRequest '{parts[0]} {parts[1]} {parts[2]}'");
                    var request =  new RtspRequest(parts);
                    request.RemoteEndpoint = endpoint;

                    return request;
                }
                else if (StartLineTest.IsMatch(parts[0]))
                {
                    LOG.Debug($"Detected RtspResponse '{parts[0]} {parts[1]} {parts[2]}'");
                    var response = new RtspResponse(parts);
                    response.RemoteEndpoint = endpoint;

                    return response;
                }

                throw new RtspMessageParseException($"Received malformed RTSPMessage start line '{parts[0]} {parts[1]} {parts[2]}'");
            }
            catch (Exception e)
            {
                if (e is RtspMessageParseException)
                {
                    throw e;
                }

                throw new RtspMessageParseException($"Unable to parse RTSP message, reason: {e.Message}", e);
            }
        }

        /// <summary>
        /// Gets the RTSP version of the message.
        /// </summary>
        public RtspVersion Version { get; internal set; }

        /// <summary>
        /// Gets the Rtsp message's headers.
        /// </summary>
        public Dictionary<string, string> Headers
        {
            get
            {
                return _headers;
            }
        }

        /// <summary>
        /// Gets flag indicating if the message contains a body or not.
        /// </summary>
        public bool HasBody
        {
            get
            {
                return Body.Length > 0;
            }
        }

        /// <summary>
        /// Gets the RTSP message's entity body.
        /// </summary>
        public byte[] Body
        {
            get
            {
                return Data;
            }

            internal set
            {
                Data = value;
            }
        }

        public int CSeq
        {
            get
            {
                int cseq = 0;
                string outString;
                if (_headers.TryGetValue(RtspHeaders.Names.CSEQ, out outString))
                {
                    if (int.TryParse(outString, out cseq))
                    {
                        return cseq;
                    }
                }

                return cseq;
            }

            set
            {
                _headers[RtspHeaders.Names.CSEQ] = value.ToString();
            }
        }

        public int ContentLength
        {
            get
            {
                int contentLen = -1;
                string outString;
                if (_headers.TryGetValue(RtspHeaders.Names.CONTENT_LENGTH, out outString))
                {
                    if (int.TryParse(outString, out contentLen))
                    {
                        return contentLen;
                    }
                }

                return contentLen;
            }

            set
            {
                _headers[RtspHeaders.Names.CONTENT_LENGTH] = value.ToString();
            }
        }

        /// <summary>
        /// Gets and sets the RTSP transport header
        /// </summary>
        public TransportHeader Transport
        {
            get
            {
                try
                {
                    string transport;
                    if (_headers.TryGetValue(RtspHeaders.Names.TRANSPORT, out transport))
                    {
                        return TransportHeader.Parse(transport);
                    }
                }
                catch (Exception e)
                {
                    LOG.Error($"Unable to parse RTSP Transport header, reason: {e.Message}");
                }

                return null;
            }

            set
            {
                _headers[RtspHeaders.Names.TRANSPORT] = value.ToString();
            }
        }

        public string ContentType
        {
            get
            {
                return _headers.ContainsKey(RtspHeaders.Names.CONTENT_TYPE) ? _headers[RtspHeaders.Names.CONTENT_TYPE] : null;
            }

            set
            {
                _headers[RtspHeaders.Names.CONTENT_TYPE] = value;
            }
        }

        public IPEndPoint RemoteEndpoint { get; private set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var header in Headers)
            {
                sb.Append(header.Key).Append(": ").Append(header.Value).Append(CRLF);
            }

            // Append required CRLF after headers.
            sb.Append(CRLF);

            // Add the body if one is available.
            if (Body.Length > 0)
            {
                sb.Append(Encoding.UTF8.GetString(Body));
            }

            return sb.ToString();
        }

        public virtual object Clone()
        {
            RtspMessage msg = CreateInstanceForClone();
            
            foreach (var entry in Headers)
            {
                msg.Headers[entry.Key] = entry.Value;
            }

            if (Body.Length > 0)
            {
                Body.CopyTo(msg.Body, 0);
            }

            return msg;
        }

        protected virtual RtspMessage CreateInstanceForClone()
        {
            return new RtspMessage(Version);
        }
    }
}
