using System;

namespace Pelco.Media.Metadata
{
    public class PlayerInitializationException : Exception
    {
        public PlayerInitializationException() : base()
        {

        }

        public PlayerInitializationException(string msg) : base(msg)
        {

        }
    }
}
