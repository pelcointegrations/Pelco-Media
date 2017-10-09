using System;

namespace Pelco.Media.Pipeline
{
    class EndOfBufferException : Exception
    {
        public EndOfBufferException() : base("End of Pipeline.Buffer reached")
        {

        }
    }
}