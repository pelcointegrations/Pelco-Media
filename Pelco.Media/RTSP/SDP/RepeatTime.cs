using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace Pelco.PDK.Media.RTSP.SDP
{
    public class RepeatTime
    {
        private static readonly string DURATION = @"\d+|\d+d|\d+h|\d+m|\d+s";
        public static readonly Regex REGEX = new Regex($@"^r\s*=\s*({DURATION})\s+({DURATION})\s+(.+)", RegexOptions.Compiled);

        public RepeatTime()
        {
            StartTimeOffsets = new List<TimeSpan>();
        }

        #region Properties

        public TimeSpan RepeatInterval { get; set; }

        public TimeSpan ActiveDuration { get; set; }

        public List<TimeSpan> StartTimeOffsets { get; private set; }

        #endregion

        public override string ToString()
        {
            var sb = new StringBuilder("r=").Append(RepeatInterval.TotalSeconds)
                                            .Append(' ')
                                            .Append(ActiveDuration.TotalSeconds);

            StartTimeOffsets.ForEach(offset =>
            {
                sb.Append(' ').Append(offset.TotalSeconds);
            });

            return sb.ToString();
        }

        public static Builder CreateBuilder()
        {
            return new Builder();
        }

        public static RepeatTime Parse(string line)
        {
            var match = REGEX.Match(line);

            if (!match.Success)
            {
                throw new SdpParseException($"Unable to parse malformed Repeat Time '{line}'");
            }

            try
            {
                var builder = CreateBuilder().RepeatInterval(ParseUtils.ToTimeSpan(match.Groups[1].Value))
                                             .ActiveDuration(ParseUtils.ToTimeSpan(match.Groups[2].Value));

                ParseStartTimeOffsets(match.Groups[3].Value, builder);

                return builder.Build();
            }
            catch (Exception e)
            {
                if (e is SdpParseException)
                {
                    throw e;
                }

                throw new SdpParseException($"Unable to parse Repeat Time {line}", e);
            }
        }

        public static void ParseStartTimeOffsets(string str, Builder builder)
        {
            Regex.Split(str, @"\s+").Where(s => s != string.Empty).ToList().ForEach(offset =>
            {
                builder.ActiveDuration(ParseUtils.ToTimeSpan(offset));
            });
        }

        public sealed class Builder
        {
            private TimeSpan _activeDuration;
            private TimeSpan _repeatInterval;
            private List<TimeSpan> _offsets;

            public Builder()
            {
                _offsets = new List<TimeSpan>();
            }

            public Builder Clear()
            {
                _activeDuration = new TimeSpan(0, 0, 0, 0);
                _repeatInterval = new TimeSpan(0, 0, 0, 0);
                _offsets.Clear();

                return this;
            }

            public Builder RepeatInterval(TimeSpan interval)
            {
                _repeatInterval = interval;

                return this;
            }

            public Builder ActiveDuration(TimeSpan duration)
            {
                _activeDuration = duration;

                return this;
            }

            public Builder AddOffset(TimeSpan offset)
            {
                _offsets.Add(offset);

                return this;
            }

            public RepeatTime Build()
            {
                var rt = new RepeatTime()
                {
                    RepeatInterval = _repeatInterval,
                    ActiveDuration = _activeDuration,
                };

                rt.StartTimeOffsets.AddRange(_offsets);

                return rt;
            }
        }
    }
}
