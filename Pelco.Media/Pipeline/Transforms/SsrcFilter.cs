using System;

namespace Pelco.Media.Pipeline.Transforms
{
    public class SsrcFilter : TransformBase
    {
        private uint _ssrc;

        public SsrcFilter(string ssrc)
        {
            _ssrc = Convert.ToUInt32(ssrc, 16);
        }

        public override bool WriteBuffer(ByteBuffer buffer)
        {
            return PushBuffer(buffer);
        }
    }
}
