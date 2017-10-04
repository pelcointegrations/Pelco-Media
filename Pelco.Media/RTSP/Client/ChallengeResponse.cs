using Pelco.PDK.Media.Common;
using System;

namespace Pelco.PDK.Media.RTSP.Client
{
    /// <summary>
    /// Base class used for generating a response to an RTSP WWW-Authenticate
    /// challenge from an RTSP server.
    /// </summary>
    public abstract class ChallengeResponse
    {
        protected ChallengeResponse(Credentials creds, string realm)
        {
            Credentials = creds;
            Realm = realm;
        }

        public enum Type
        {
            Basic,
            Digest,
            Undefined
        }

        public string Realm { get; private set; }

        /// <summary>
        /// Gets the credentials used for Authorization.
        /// </summary>
        public Credentials Credentials { get; private set; }

        /// <summary>
        /// Gets the type of the challenge response.
        /// </summary>
        public abstract Type ChallengeType { get; }

        /// <summary>
        /// Generate the RTSP Authorization header value based on the WWW-Authenticate
        /// challenge type
        /// </summary>
        /// <param name="method">The RTSP method used to generate header value</param>
        /// <param name="uri">The uri used to generate the header value</param>
        /// <returns></returns>
        public abstract string Generate(RtspRequest.RtspMethod method, Uri uri);
    }
}
