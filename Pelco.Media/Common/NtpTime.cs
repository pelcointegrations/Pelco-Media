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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ntpTime">The ntp timestamp</param>
        public NtpTime(long ntpTime)
        {
            _ntpTime = ntpTime;
        }

        /// <summary>
        /// Convert 64-bit NTP timestamp to a <see cref="DateTime"/>
        ///
        /// Note that java time(milliseconds) by definition has less precision
        /// then NTP time(picoseconds) so converting NTP timestamp to c# time and back
        /// to NTP timestamp loses precision. For example, Tue, Dec 17 2002 09:07:24.810 EST
        /// is represented by a single C# time value of f22cd1fc8a, but its
        /// NTP equivalent are all values ranging from c1a9ae1c.cf5c28f5 to c1a9ae1c.cf9db22c.
        /// </summary>
        public DateTime UtcDate
        {
            get
            {
                var offset = DateTimeOffset.FromUnixTimeMilliseconds(GetTime());
                return offset.UtcDateTime;
            }
        }

        /// <summary>
        /// Returns high-order 32-bits representing the seconds of this NTP timestamp.
        /// </summary>
        public uint Seconds
        {
            get
            {
                return (uint)((_ntpTime >> 32) & 0xffffffffL);
            }
        }

        /// <summary>
        /// Returns low-order 32-bits representing the fractional seconds.
        /// </summary>
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

        private long GetTime()
        {
            long seconds = Seconds;
            long fraction = Fraction;

            double fpart = 1000D * fraction / 0x100000000;
            // Use round-off on fractional part to preserve going to lower precision
            fraction = (long)Math.Round(fpart);

            /*
             * If the most significant bit (MSB) on the seconds field is set we use
             * a different time base. The following text is a quote from RFC-2030 (SNTP v4):
             *
             *  If bit 0 is set, the UTC time is in the range 1968-2036 and UTC time
             *  is reckoned from 0h 0m 0s UTC on 1 January 1900. If bit 0 is not set,
             *  the time is in the range 2036-2104 and UTC time is reckoned from
             *  6h 28m 16s UTC on 7 February 2036.
             */
            long msb = seconds & 0x80000000;
            if (msb == 0)
            {
                return baseTime0 + (seconds * 1000) + fraction;
            }
            else
            {
                return baseTime1 + (seconds * 1000) + fraction;
            }
        }
    }
}
