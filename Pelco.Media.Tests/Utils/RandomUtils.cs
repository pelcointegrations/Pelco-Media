using Pelco.Media.Pipeline;
using System;

namespace Pelco.Media.Tests.Utils
{
    public class RandomUtils
    {
        public static ByteBuffer RandomBytes(int capacity)
        {
            var rand = new Random();
            var bytes = new byte[capacity];

            rand.NextBytes(bytes);

            return new ByteBuffer(bytes, 0, capacity, true);
        }
    }
}
