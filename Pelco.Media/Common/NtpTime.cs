using System;

namespace Pelco.Media.Common
{
    /// <summary>
    /// NtpTime converts a <see cref="DateTime"/> instance into an NTP time
    /// of seconds and factional seconds.
    /// </summary>
    public class NtpTime
    {
        // Baseline Time 7-Feb-2036 @ 06:28:16 UTC
        private static readonly long baseTime0 = 2085978496000L;

        // Baseline Time 1-Jan-1900 @ 01:00:00 UTC
        private static readonly long baseTime1 = -2208988800000L;

        private long _ntpTime;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dt">DateTime to convert to ntp time</param>
        public NtpTime(DateTime dt)
        {
            if (dt.Kind != DateTimeKind.Utc)
            {
                dt = dt.ToUniversalTime();
            }

            _ntpTime = ToNtpTime(GetMillisecondsFromJan011970(dt));
        }

        public uint Seconds
        {
            get
            {
                return (uint)((_ntpTime >> 32) & 0xffffffffL);
            }
        }

        public uint Fraction
        {
            get
            {
                return (uint)(_ntpTime & 0xffffffffL);
            }
        }

        private long GetMillisecondsFromJan011970(DateTime dt)
        {
            var ts = dt.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            return (long)ts.TotalMilliseconds;
        }

        private long ToNtpTime(long time)
        {
            bool useBaseTime1 = time < baseTime0;

            long baseTime = (long)(useBaseTime1 ? (time - baseTime1) : (time - baseTime0));

            long seconds = baseTime / 1000;
            long fraction = ((baseTime % 1000) * 0x100000000L) / 1000;

            long ntpTime = seconds << 32 | fraction;

            return ntpTime;
        }
    }
}
