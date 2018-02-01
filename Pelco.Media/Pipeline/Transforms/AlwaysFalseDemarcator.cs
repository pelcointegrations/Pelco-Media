using Pelco.Media.RTP;

namespace Pelco.Media.Pipeline.Transforms
{
    public class AlwaysFalseDemarcator : IRtpDemarcator
    {
        public bool Check(RtpPacket packet)
        {
            return false;
        }
    }
}
