using Pelco.PDK.Media.Common;
using Pelco.PDK.Media.RTSP.Utils;
using System.Collections.Immutable;

namespace Pelco.PDK.Media.RTSP.Client
{
    /// <summary>
    /// Factory class that can be used to generate a <see cref="ChallengeResponse"/> instance.
    /// A <see cref="ChallengeResponse"/> can be used to generate the Authentication request
    /// header value, based on the WWW-Authenticate response challenge.
    /// </summary>
    public sealed class AuthChallenge
    {
        private AuthChallenge()
        {

        }

        /// <summary>
        /// Parses a string that contains the value of an WWW-Authenticate response header. 
        /// </summary>
        /// <param name="creds">The credentials to use for creating the <see cref="ChallengeResponse"/></param>
        /// <param name="wwwAuth">The value of the WWW-Authenticate response header.</param>
        /// <returns><see cref="ChallengeResponse"/> instance. One of either Basic or Digest</returns>
        public static ChallengeResponse Parse(Credentials creds, string wwwAuth)
        {
            if (string.IsNullOrEmpty(wwwAuth))
            {
                // Some clients respond with an RTSP 401 code but do not
                // provide a WWW-Authenticate header in the response. If this
                // occurs we should respond with a Basic Auth response.
                return new BasicAuthChallengeResponse(creds, "");
            }

            int index = wwwAuth.IndexOf(' ');
            if (index == -1)
            {
                throw new RtspClientException($"Malformed WWW-Authenticate header '{wwwAuth}'");
            }

            var challenge = wwwAuth.Substring(0, index);
            var args = wwwAuth.Substring(index + 1).Replace("\"", "").Trim();

            var parameters = HeaderValueDecoder.Decode(args);

            switch (challenge.ToLower())
            {
                case "basic":
                    return new BasicAuthChallengeResponse(creds, parameters["realm"]);

                case "digest":
                    return HandleDigest(creds, parameters);

                default:
                    throw new RtspClientException($"Unknow WWW-Authenticate challenge response {challenge}");
            }
        }

        private static ChallengeResponse HandleDigest(Credentials creds, ImmutableDictionary<string, string> parameters)
        {
            var realm = parameters["realm"];
            var nonce = parameters["nonce"];
            var opaque = parameters.ContainsKey("opaque") ? parameters["opaque"] : string.Empty;
            var domain = parameters.ContainsKey("domain") ? parameters["domain"] : string.Empty;
            var algo = parameters.ContainsKey("algorithm") ? parameters["algorithm"] : string.Empty;

            bool stale = false;
            string outStr;
            if (parameters.TryGetValue("stale", out outStr))
            {
                if (!bool.TryParse(outStr, out stale))
                {
                    stale = false;
                }
            }

            if (string.IsNullOrEmpty(realm) || string.IsNullOrEmpty(nonce))
            {
                throw new RtspClientException("Malformed WWW-Authenticate header, must contain both nonce and realm params");
            }

            // Fail if an algorithm is defined that is not MD5.  If one is not defined
            // we will assume MD5.
            if (!string.IsNullOrEmpty(algo) && !algo.ToLower().Equals("md5"))
            {
                throw new RtspClientException("Only the MD5 algorithm for RTSP Digest authentication is supported");
            }

            return new DigestAuthChallengeResponse(creds,
                                                   realm,
                                                   domain,
                                                   nonce,
                                                   opaque,
                                                   DigestAuthChallengeResponse.Algorithm.MD5,
                                                   stale);
        }
    }
}
