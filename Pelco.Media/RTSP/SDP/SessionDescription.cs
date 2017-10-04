using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pelco.PDK.Media.RTSP.SDP
{
    public class SessionDescription
    {
        public static readonly string MIME_TYPE = "application/sdp";

        private static readonly string CRLF = "\r\n";
        private static readonly decimal SDP_VERSION = 0M;

        public SessionDescription()
        {
            Version = SDP_VERSION;
            Attributes = new List<Attribute>();
            Bandwidths = new List<BandwidthInfo>();
            TimeDescriptions = new List<TimeDescription>();
            MediaDescriptions = new List<MediaDescription>();
        }

        public decimal Version { get; set; }

        public string SessionName { get; set; }

        public string SessionInformation { get; set; }

        public SessionOriginator Origin { get; set; }

        public Uri URI { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public ConnectionInfo Connection { get; set; }

        public List<BandwidthInfo> Bandwidths { get; private set; }

        public List<TimeDescription> TimeDescriptions { get; private set; }

        public TimeZone TimeZone { get; set; }

        public List<Attribute> Attributes { get; private set; }

        public List<MediaDescription> MediaDescriptions { get; private set; }

        public override string ToString()
        {
            var sb = new StringBuilder("v=").Append(Version)
                                            .Append(CRLF);

            if (Origin != null)
            {
                sb.Append(Origin).Append(CRLF);
            }

            if (SessionName != null)
            {
                sb.Append("s=").Append(SessionName).Append(CRLF);
            }

            if (SessionInformation != null)
            {
                sb.Append("i=").Append(SessionInformation).Append(CRLF);
            }

            if (URI != null)
            {
                sb.Append("u=").Append(URI).Append(CRLF);
            }

            if (Email != null)
            {
                sb.Append("e=").Append(Email).Append(CRLF);
            }

            if (PhoneNumber != null)
            {
                sb.Append("p=").Append(PhoneNumber).Append(CRLF);
            }

            if (Connection != null)
            {
                sb.Append(Connection).Append(CRLF);
            }

            Bandwidths.ForEach(bw =>
            {
                sb.Append(bw).Append(CRLF);
            });

            TimeDescriptions.ForEach(td =>
            {
                sb.Append(td);
            });

            if (TimeZone != null)
            {
                sb.Append(TimeZone).Append(CRLF);
            }

            Attributes.ForEach(attr =>
            {
                sb.Append(attr).Append(CRLF);
            });

            MediaDescriptions.ForEach(md =>
            {
                sb.Append(md);
            });

            return sb.ToString();
        }

        public static SessionDescription Parse(string data)
        {
            TimeDescription currentTd = null;
            MediaDescription currentMd = null;

            try
            {
                SessionDescription sdp = new SessionDescription();

                var lines = Regex.Split(data, CRLF);

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();

                    if (trimmed.StartsWith("v="))
                    {
                        var value = ParseKeyValueLine(trimmed, "Session Version");

                        decimal version;
                        if (!decimal.TryParse(value, out version))
                        {
                            // TODO(frank.lamar): warn.
                        }
                        else
                        {
                            sdp.Version = version;
                        }

                    }
                    else if (trimmed.StartsWith("o="))
                    {
                        sdp.Origin = SessionOriginator.Parse(trimmed);
                    }
                    else if (trimmed.StartsWith("s="))
                    {
                        sdp.SessionName = ParseKeyValueLine(trimmed, "Session Name");
                    }
                    else if (trimmed.StartsWith("i="))
                    {
                        var info = ParseKeyValueLine(trimmed, "Session Information");

                        if (currentMd != null)
                        {
                            // Media Title at the media level
                            currentMd.MediaTitle = info;
                        }
                        else
                        {
                            // Session information at the Session level.
                            sdp.SessionInformation = info;
                        }
                    }
                    else if (trimmed.StartsWith("u="))
                    {
                        var value = ParseKeyValueLine(trimmed, "Session URI");

                        Uri uri;
                        if (!Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out uri))
                        {
                            throw new SdpParseException($"Unable to parse malformed Session URI {value}");
                        }

                        sdp.URI = uri;
                    }
                    else if (trimmed.StartsWith("e="))
                    {
                        sdp.Email = ParseKeyValueLine(trimmed, "Session Email");
                    }
                    else if (trimmed.StartsWith("p="))
                    {
                        sdp.PhoneNumber = ParseKeyValueLine(trimmed, "Session Phone Number");
                    }
                    else if (trimmed.StartsWith("c="))
                    {
                        var connection = ConnectionInfo.Parse(trimmed);

                        if (currentMd != null)
                        {
                            // Media Description connection
                            currentMd.Connection = connection;
                        }
                        else
                        {
                            // Session level connection
                            sdp.Connection = connection;
                        }
                    }
                    else if (trimmed.StartsWith("b="))
                    {
                        var bandwidth = BandwidthInfo.Parse(trimmed);

                        if (currentMd != null)
                        {
                            // Media Description bandwidth
                            currentMd.Bandwidths.Add(bandwidth);
                        }
                        else
                        {
                            // Session level bandwidth
                            sdp.Bandwidths.Add(bandwidth);
                        }
                    }
                    else if (trimmed.StartsWith("t="))
                    {
                        currentTd = TimeDescription.Parse(trimmed);
                        sdp.TimeDescriptions.Add(currentTd);
                    }
                    else if (trimmed.StartsWith("r="))
                    {
                        if (currentTd != null)
                        {
                            currentTd.RepeatTimes.Add(RepeatTime.Parse(trimmed));
                        }

                        // Just ignore the repeate time if no current time was defined.
                    }
                    else if (trimmed.StartsWith("z="))
                    {
                        sdp.TimeZone = TimeZone.Parse(trimmed);
                    }
                    else if (trimmed.StartsWith("a="))
                    {
                        if (currentMd != null)
                        {
                            // Media Description level attribute
                            currentMd.Attributes.Add(Attribute.Parse(trimmed));
                        }
                        else
                        {
                            // Session level attribute
                            sdp.Attributes.Add(Attribute.Parse(trimmed));
                        }
                    }
                    else if (trimmed.StartsWith("m="))
                    {
                        currentMd = MediaDescription.Parse(trimmed);
                        sdp.MediaDescriptions.Add(currentMd);
                    }
                } // end foreach...

                return sdp;
            }
            catch (Exception e)
            {
                if (e is SdpParseException)
                {
                    throw e;
                }

                throw new SdpParseException("Failed to parse session description", e);
            }
        }

        private static string ParseKeyValueLine(string line, string description)
        {
            var parts = Regex.Split(line, "=").Where(s => s != string.Empty).ToArray();
            if (parts.Length != 2)
            {
                throw new SdpParseException($"Unable to parse malformed {description} '{line}'");
            }

            return parts[1].Trim();
        }
    }
}
