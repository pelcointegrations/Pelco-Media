using System;
using System.Linq;
using System.Text;
using Pelco.PDK.Media.Common;
using System.Security.Cryptography;

namespace Pelco.PDK.Media.RTSP.Client
{
    /// <summary>
    /// A <see cref="ChallengeResponse"/> of type Digest.  This class will generate an Authentication
    /// header value as defined in Section 3.2.2 of RFC 2617.
    /// </summary>
    /// <remarks>
    /// This class does not support the optional qop-options that should be supported
    /// if provided by the server.  Because servers should be backwards compatible digest
    /// authentication should still work, in fact VLC also acts the same way.
    /// 
    /// Also only MD5 algorithm types are currently supported.
    /// </remarks>
    public class DigestAuthChallengeResponse : ChallengeResponse
    {
        public enum Algorithm
        {
            MD5,
            MD5_SEES,
            TOKEN,
            UNDEFINED
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="creds"></param>
        /// <param name="realm"></param>
        /// <param name="domain"></param>
        /// <param name="nonce"></param>
        /// <param name="opaque"></param>
        /// <param name="algorithm"></param>
        /// <param name="stale"></param>
        internal DigestAuthChallengeResponse(Credentials creds,
                                             string realm,
                                             string domain,
                                             string nonce,
                                             string opaque,
                                             Algorithm algorithm,
                                             bool stale) : base(creds, realm)
        {
            Domain = domain;
            Nonce = nonce;
            Opaque = opaque;
            DigestAlgorithm = algorithm;
            Stale = stale;
        }

        #region Properties

        public string Domain { get; private set; }

        public string Nonce { get; private set; }

        public Algorithm DigestAlgorithm { get; private set; }

        public string Opaque { get; private set; }

        public bool Stale { get; private set; }

        /// <summary>
        /// <see cref="ChallengeResponse.ChallengeType"/>
        /// </summary>
        public override Type ChallengeType
        {
            get
            {
                return Type.Digest;
            }
        }

        #endregion

        /// <summary>
        /// <see cref="ChallengeResponse.Generate(RtspRequest.RtspMethod, Uri)"/>
        /// </summary>
        public override string Generate(RtspRequest.RtspMethod method, Uri uri)
        {
            // Generate digets response value.
            var ha1 = ToMd5HexString($"{Credentials.Username}:{Realm}:{Credentials.Password}");
            var ha2 = ToMd5HexString($"{method}:{uri}");
            var response = ToMd5HexString($"{ha1}:{Nonce}:{ha2}");

            var sb = new StringBuilder("Digest").Append(" username=\"")
                                                .Append(Credentials.Username)
                                                .Append("\"")
                                                .Append(", realm=\"")
                                                .Append(Realm)
                                                .Append("\"")
                                                .Append(", nonce=\"")
                                                .Append(Nonce)
                                                .Append("\"");

            if (!string.IsNullOrEmpty(Domain))
            {
                sb.Append(", domain=\"").Append(Domain).Append("\"");
            }

            if (!string.IsNullOrEmpty(Opaque))
            {
                sb.Append(", opaque=\"").Append(Opaque).Append("\"");
            }

            sb.Append(", uri=\"").Append(uri).Append("\"");
            sb.Append(", response=\"").Append(response).Append("\"");

            return sb.ToString();
        }

        private string ToMd5HexString(string str)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(str));

                return string.Concat(hash.Select(b => b.ToString("x2")));
            }
        }
    }
}
