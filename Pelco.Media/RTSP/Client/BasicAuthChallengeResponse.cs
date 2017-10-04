using System;
using System.Text;
using Pelco.PDK.Media.Common;

namespace Pelco.PDK.Media.RTSP.Client
{
    /// <summary>
    /// A <see cref="ChallengeResponse"/> of type Basic.  This class will generate an
    /// Authentication header value as defined in Section 2 of RFC 2617.
    /// </summary>
    public class BasicAuthChallengeResponse : ChallengeResponse
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="creds"></param>
        /// <param name="realm"></param>
        internal BasicAuthChallengeResponse(Credentials creds, string realm) : base(creds, realm)
        {
        }

        /// <summary>
        /// <see cref="ChallengeResponse.ChallengeType"/>
        /// </summary>
        public override Type ChallengeType
        {
            get
            {
                return Type.Basic;
            }
        }

        /// <summary>
        /// <see cref="ChallengeResponse.Generate(RtspRequest.RtspMethod, Uri)"/>
        /// </summary>
        public override string Generate(RtspRequest.RtspMethod method, Uri uri)
        {
            string auth = $"{Credentials.Username}:{Credentials.Password}";

            return $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(auth))}";
        }
    }
}
