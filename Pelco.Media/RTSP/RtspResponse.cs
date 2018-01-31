using Pelco.Media.RTSP.SDP;
using System.Collections.Generic;
using System.Text;

namespace Pelco.Media.RTSP
{
    public class RtspResponse : RtspMessage
    {
        public sealed class Status
        {
            public static readonly Status Continue = new Status(100, "Continue");
            public static readonly Status Ok = new Status(200, "OK");
            public static readonly Status Created = new Status(201, "Created");
            public static readonly Status LowStorageSpace = new Status(250, "Low on Storage Space");
            public static readonly Status MultipleChoices = new Status(300, "MultipleChoices");
            public static readonly Status MovedPermanently = new Status(301, "Moved Permanently");
            public static readonly Status MovedTemporarily = new Status(302, "Moved Temporarily");
            public static readonly Status SeeOther = new Status(303, "See Other");
            public static readonly Status NotModified = new Status(304, "Not Modified");
            public static readonly Status UseProxy = new Status(305, "Use Proxy");
            public static readonly Status BadRequest = new Status(400, "Bad Request");
            public static readonly Status Unauthorized = new Status(401, "Unauthorized");
            public static readonly Status PaymentRequired = new Status(402, "Payment Required");
            public static readonly Status Forbidden = new Status(403, "Forbidden");
            public static readonly Status NotFound = new Status(404, "Not Found");
            public static readonly Status MethodNotAllowed = new Status(405, "Method Not Allowed");
            public static readonly Status NotAcceptable = new Status(406, "Not Acceptable");
            public static readonly Status ProxyAuthenticationRequired = new Status(407, "Proxy Authentication Required");
            public static readonly Status RequestTimeout = new Status(408, "Request Time-out");
            public static readonly Status Gone = new Status(410, "Gone");
            public static readonly Status LengthRequired = new Status(411, "Length Required");
            public static readonly Status PreconditionFailed = new Status(412, "Precondition Failed");
            public static readonly Status RequestEntityTooLarge = new Status(413, "Request Entity Too Large");
            public static readonly Status RequestUriTooLarge = new Status(414, "Request-URI Too Large");
            public static readonly Status UnsupportedMediaType = new Status(415, "Unsupported Media Type");
            public static readonly Status ParameterNotUnderstood = new Status(451, "Parameter Not Understood");
            public static readonly Status ConferenceNotFound = new Status(452, "Conference Not Found");
            public static readonly Status NotEnoughBandwidth = new Status(453, "Not Enough Bandwidth");
            public static readonly Status SessionNotFound = new Status(454, "Session Not Found");
            public static readonly Status MethodNotValidInThisState = new Status(455, "Method Not Valid in This State");
            public static readonly Status HeaderFieldNotValidForResource = new Status(456, "Header Field Not Valid for Resource");
            public static readonly Status InvalidRange = new Status(457, "Invalid Range");
            public static readonly Status ParameterIsReadOnly = new Status(458, "Parameter Is Read-Only");
            public static readonly Status AggregateOperationNotAllowed = new Status(459, "Aggregate operation not allowed");
            public static readonly Status OnlyAggregateOperationAllowed = new Status(460, "Only aggregate operation allowed");
            public static readonly Status UnsupportedTransport = new Status(461, "Unsupporated transport");
            public static readonly Status DestinationUnreachable = new Status(462, "Destination unreachable");
            public static readonly Status KeyManagementFailure = new Status(463, "Key management Failure");
            public static readonly Status InternalServerError = new Status(500, "Internal Server Error");
            public static readonly Status NotImplemented = new Status(501, "Not Implemented");
            public static readonly Status BadGateway = new Status(502, "Bad Gateway");
            public static readonly Status ServiceUnavailable = new Status(503, "Service Unavailable");
            public static readonly Status GatewayTimeout = new Status(504, "Gateway Time-out");
            public static readonly Status RtspVersionNotSupported = new Status(505, "RTSP Version not supported");
            public static readonly Status OptionNotSupported = new Status(551, "Option not supported");

            private Status(int code, string reason)
            {
                Code = code;
                ReasonPhrase = reason;
            }

            public int Code { get; private set; }

            public string ReasonPhrase { get; private set; }

            public bool Is(Status status)
            {
                return Code == status.Code;
            }

            public override string ToString()
            {
                return $"{Code} {ReasonPhrase}";
            }

            public static Status ValueOf(int code)
            {
                switch (code)
                {
                    case 100: return Continue;
                    case 200: return Ok;
                    case 250: return Created;
                    case 300: return MultipleChoices;
                    case 301: return MovedPermanently;
                    case 302: return MovedTemporarily;
                    case 303: return SeeOther;
                    case 304: return NotModified;
                    case 305: return UseProxy;
                    case 400: return BadRequest;
                    case 401: return Unauthorized;
                    case 402: return PaymentRequired;
                    case 403: return Forbidden;
                    case 404: return NotFound;
                    case 405: return MethodNotAllowed;
                    case 406: return NotAcceptable;
                    case 407: return ProxyAuthenticationRequired;
                    case 408: return RequestTimeout;
                    case 410: return Gone;
                    case 411: return LengthRequired;
                    case 412: return PreconditionFailed;
                    case 413: return RequestEntityTooLarge;
                    case 414: return RequestUriTooLarge;
                    case 415: return UnsupportedMediaType;
                    case 451: return ParameterNotUnderstood;
                    case 452: return ConferenceNotFound;
                    case 453: return NotEnoughBandwidth;
                    case 454: return SessionNotFound;
                    case 455: return MethodNotValidInThisState;
                    case 456: return HeaderFieldNotValidForResource;
                    case 457: return InvalidRange;
                    case 458: return ParameterIsReadOnly;
                    case 459: return AggregateOperationNotAllowed;
                    case 460: return OnlyAggregateOperationAllowed;
                    case 461: return UnsupportedTransport;
                    case 462: return DestinationUnreachable;
                    case 463: return KeyManagementFailure;
                    case 500: return InternalServerError;
                    case 501: return NotImplemented;
                    case 502: return BadGateway;
                    case 503: return ServiceUnavailable;
                    case 504: return GatewayTimeout;
                    case 505: return RtspVersionNotSupported;
                    case 551: return OptionNotSupported;
                    default : return new Status(999, "Unknown Status");
                }
            }
        }

        public RtspResponse(Status status) : base(RtspVersion.RTSP_1_0)
        {
            ResponseStatus = status;
        }

        internal RtspResponse(string[] lineParts) : base(RtspVersion.Parse(lineParts[0]))
        {
            ResponseStatus = Status.ValueOf(int.Parse(lineParts[1].Trim()));
        }

        /// <summary>
        /// Gets the response's status.
        /// </summary>
        public Status ResponseStatus { get; private set; }

        /// <summary>
        /// Gets and sets the RTSP WWW-Authenticate header.
        /// </summary>
        public string WWWAuthenticate
        {
            get
            {
                return Headers.ContainsKey(RtspHeaders.Names.WWW_AUTHENTICATE)
                            ? Headers[RtspHeaders.Names.WWW_AUTHENTICATE]
                            : null;
            }

            set
            {
                Headers[RtspHeaders.Names.WWW_AUTHENTICATE] = value;
            }
        }

        /// <summary>
        /// Gets the RTSP Session header.
        /// </summary>
        public Session Session
        {
            get
            {
                return Headers.ContainsKey(RtspHeaders.Names.SESSION)
                            ? Session.Parse(Headers[RtspHeaders.Names.SESSION])
                            : null;
            }
        }

        /// <summary>
        /// <see cref="object.ToString"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return new StringBuilder(Version.ToString()).Append(' ')
                                                        .Append(ResponseStatus.Code)
                                                        .Append(' ')
                                                        .Append(ResponseStatus.ReasonPhrase)
                                                        .Append(CRLF)
                                                        .Append(base.ToString())
                                                        .ToString();
        }

        /// <summary>
        /// Retrieves the response body as a <see cref="SessionDescription"/>.
        /// </summary>
        /// <exception cref="SdpParseException">Thrown if the response body cannot be parsed</exception>
        /// <returns></returns>
        public SessionDescription GetBodyAsSdp()
        {
            if (!HasBody)
            {
                throw new SdpParseException("Unable to parse body as SDP, response does not contain a body");
            }
            else if (ContentType != SessionDescription.MIME_TYPE)
            {
                throw new SdpParseException($"Response body is not of type '{SessionDescription.MIME_TYPE}'");
            }

            return SessionDescription.Parse(Encoding.UTF8.GetString(Body));
        }

        public static Builder CreateBuilder()
        {
            return new Builder();
        }

        public sealed class Builder
        {
            private object _body;
            private Status _status;
            private Dictionary<string, string> _headers;

            public Builder()
            {
                _headers = new Dictionary<string, string>();

                Clear();
            }

            public Builder Clear()
            {
                _status = RtspResponse.Status.Ok;
                _body = null;
                _headers.Clear();

                return this;
            }

            public Builder AddHeader(string name, string value)
            {
                _headers.Add(name, value);

                return this;
            }

            public Builder Status(Status status)
            {
                _status = status;
                return this;
            }

            public Builder Body(object body)
            {
                _body = body;
                return this;
            }

            public Builder Body(SDP.SessionDescription sdp)
            {
                if (_headers.ContainsKey(RtspHeaders.Names.CONTENT_TYPE))
                {
                    _headers[RtspHeaders.Names.CONTENT_TYPE] = "application/sdp";
                }
                else
                {
                    _headers.Add(RtspHeaders.Names.CONTENT_TYPE, "application/sdp");
                }

                _body = sdp;
                return this;
            }

            public RtspResponse Build()
            {
                var response = new RtspResponse(_status)
                {
                    Body = _body == null ? new byte[0] : Encoding.UTF8.GetBytes(_body.ToString())
                };

                foreach (var entry in _headers)
                {
                    response.Headers.Add(entry.Key, entry.Value);
                }

                return response;
            }
        }
    }
}