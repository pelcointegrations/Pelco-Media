using System;

namespace Pelco.PDK.Metadata
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
