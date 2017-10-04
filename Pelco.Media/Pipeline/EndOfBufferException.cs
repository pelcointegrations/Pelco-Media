using System;

namespace Pelco.PDK.Media.Pipeline
{
    class EndOfBufferException : Exception
    {
        public EndOfBufferException() : base("End of Pipeline.Buffer reached")
        {

        }
    }
}