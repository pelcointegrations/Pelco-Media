using System;
using System.Collections.Generic;
using System.Text;

namespace Pelco.PDK.Media.RTSP
{
    public class RtspRequest : RtspMessage
    {
        /// <summary>
        /// Rtsp method type
        /// </summary>
        public enum RtspMethod
        {
            UNKNOWN,
            DESCRIBE,
            ANNOUNCE,
            GET_PARAMETER,
            OPTIONS,
            PAUSE,
            PLAY,
            RECORD,
            REDIRECT,
            SETUP,
            SET_PARAMETER,
            TEARDOWN
        }

        internal RtspRequest(RtspMethod method, Uri uri) : base(RtspVersion.RTSP_1_0)
        {
            URI = uri;
            Method = method;
        }

        internal RtspRequest(string[] lineParts) : base(RtspVersion.Parse(lineParts[2]))
        {
            RtspMethod method;
            if (!Enum.TryParse<RtspMethod>(lineParts[0].Trim(), true, out method))
            {
                method = RtspMethod.UNKNOWN;
            }
            Method = method;

            URI = new Uri(lineParts[1]);
        }

        /// <summary>
        /// The request Uri of the <see cref="RtspRequest"/>.
        /// </summary>
        public Uri URI { get; private set; }

        /// <summary>
        /// The Rtsp method of the <see cref="RtspRequest"/>.
        /// </summary>
        public RtspMethod Method { get; private set; }

        /// <summary>
        /// Gets and sets the RTSP Authorization header.
        /// </summary>
        public string Authorization
        {
            get
            {
                return Headers.ContainsKey(RtspHeaders.Names.AUTHORIZATION) ? Headers[RtspHeaders.Names.AUTHORIZATION] : null;
            }

            set
            {
                Headers[RtspHeaders.Names.AUTHORIZATION] = value; 
            }
        }

        public override object Clone()
        {
            return base.Clone() as RtspRequest;
        }

        protected override RtspMessage CreateInstanceForClone()
        {
            return new RtspRequest(Method, URI);
        }

        public override string ToString()
        {
            return new StringBuilder().Append(Method)
                                      .Append(' ')
                                      .Append(URI)
                                      .Append(' ')
                                      .Append(Version)
                                      .Append(CRLF)
                                      .Append(base.ToString())
                                      .ToString();
        }

        public static Builder CreateBuilder()
        {
            return new Builder();
        }

        public sealed class Builder
        {
            private Uri _uri;
            private RtspRequest.RtspMethod _method;
            private Dictionary<string, string> _headers;

            public Builder()
            {
                _headers = new Dictionary<string, string>();
            }

            public Builder Clear()
            {
                _uri = null;
                _method = RtspMethod.UNKNOWN;

                _headers.Clear();

                return this;
            }

            public Builder Uri(Uri uri)
            {
                _uri = uri;

                return this;
            }

            public Builder Method(RtspMethod method)
            {
                _method = method;

                return this;
            }

            public Builder AddHeader(string name, string value)
            {
                _headers.Add(name, value);

                return this;
            }

            public RtspRequest Build()
            {
                var request = new RtspRequest(_method, _uri);
                
                foreach (var entry in _headers)
                {
                    request.Headers.Add(entry.Key, entry.Value);
                }

                return request;
            }
        }
    }
}
