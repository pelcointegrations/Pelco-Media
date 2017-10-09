using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pelco.Media.RTSP.SDP
{
    public class Adjustment
    {
        public Adjustment()
        {
            Time = 0;
            Offset = new TimeSpan(0, 0, 0, 0);
        }

        public Adjustment(ulong time, TimeSpan offset)
        {
            Time = time;
            Offset = offset;
        }

        public ulong Time { get; set; }

        public TimeSpan Offset { get; set; }
    }

    public class TimeZone
    {
        public TimeZone()
        {
            TimeAdjustments = new List<Adjustment>();
        }

        public List<Adjustment> TimeAdjustments { get; private set; }

        /// <summary>
        /// <see cref="object.ToString"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder("z=");

            TimeAdjustments.ForEach(adj =>
            {
                sb.Append(adj.Time).Append(' ').Append(adj.Offset.TotalSeconds).Append("s ");
            });

            return sb.ToString().Trim();
        }

        public static TimeZone Parse(string line)
        {
            var tz = new TimeZone();

            var adjustments = Regex.Split(line, @"\s+").Where(s => s != string.Empty).ToArray();
            if ((adjustments.Count() % 2) != 0)
            {
                throw new SdpParseException($"Unable to parse malformed TimeZone '{line}'");
            }

            try
            {
                for (int i = 0; i < adjustments.Count(); i += 2)
                {
                    tz.TimeAdjustments.Add(new Adjustment(ulong.Parse(adjustments[i]),
                                                          ParseUtils.ToTimeSpan(adjustments[i + 1])));
                }

                return tz;
            }
            catch (Exception e)
            {
                throw new SdpParseException($"Unable to parse Timezone '{line}'", e);
            }
        }

        public static Builder CreateBuilder()
        {
            return new Builder();
        }

        public sealed class Builder
        {
            List<Adjustment> _adjustments;

            public Builder()
            {
                _adjustments = new List<Adjustment>();
            }

            public Builder Clear()
            {
                _adjustments.Clear();

                return this;
            }

            public Builder Add(Adjustment adjustment)
            {
                _adjustments.Add(adjustment);

                return this;
            }

            public Builder Add(uint time, TimeSpan offset)
            {
                _adjustments.Add(new Adjustment(time, offset));
                return this;
            }

            public TimeZone Build()
            {
                var tz = new TimeZone();

                tz.TimeAdjustments.AddRange(_adjustments);

                return tz;
            }
        }
    }
}
