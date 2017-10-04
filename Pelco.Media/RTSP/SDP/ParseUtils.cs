using System;

namespace Pelco.PDK.Media.RTSP.SDP
{
    /// <summary>
    /// SDP parsing utilities
    /// </summary>
    internal class ParseUtils
    {
        /// <summary>
        /// Parses a string formated as follows (30d, 3h, 5m, 7s, or 8) into a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="str">The string to parse</param>
        /// <returns>The <see cref="TimeSpan"/> represented by the string</returns>
        internal static TimeSpan ToTimeSpan(string str)
        {
            str = str.Trim();

            switch (str[str.Length - 1])
            {
                case 'd': return TimeSpan.FromDays(ulong.Parse(str.Substring(0, str.Length - 1)));
                case 'h': return TimeSpan.FromHours(ulong.Parse(str.Substring(0, str.Length - 1)));
                case 'm': return TimeSpan.FromMinutes(ulong.Parse(str.Substring(0, str.Length - 1)));
                case 's': return TimeSpan.FromSeconds(ulong.Parse(str.Substring(0, str.Length - 1)));
                default: return TimeSpan.FromSeconds(ulong.Parse(str));
            }
        }
    }
}
