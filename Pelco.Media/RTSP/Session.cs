using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pelco.PDK.Media.RTSP
{
    /// <summary>
    /// Represents the RTSP session header.
    /// </summary>
    public class Session
    {
        private static readonly uint DEFAULT_SESSION_TIMOUT_SECS = 60;

        private Session()
        {

        }

        /// <summary>
        /// Gets the session id.
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// Gets the session's timeout value.
        /// </summary>
        public uint Timeout { get; private set; }

        /// <summary>
        /// <see cref="object.ToString"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{ID};timeout={Timeout}";
        }

        /// <summary>
        /// Creates a session instance from a string. The string must conform to the the following format
        /// 'session-id [ ";" "timeout" "=" delta-seconds ]' as specified in RFC 2326 section 12.37.
        /// </summary>
        /// <param name="str">The string to parse</param>
        /// <returns></returns>
        public static Session Parse(string str)
        {
            var session = new Session();

            if (str.Contains(";"))
            {
                var parts = Regex.Split(str, ";").Where(s => s != string.Empty).ToArray();
                if (parts.Length <= 0)
                {
                    throw new ArgumentException($"Malformed RTSP session header {str}");
                }
                else
                {
                    session.ID = parts[0].Trim();
                    session.Timeout = DEFAULT_SESSION_TIMOUT_SECS;

                    if (parts.Length == 2)
                    {
                        var match = Regex.Match(parts[1], @"\s*timeout\s*=\s*(\d+)$");

                        if (match.Success)
                        {
                            uint timeout;
                            if (uint.TryParse(match.Groups[1].Value.Trim(), out timeout))
                            {
                                session.Timeout = timeout;
                            }
                        }
                    }
                }
                
            }
            else
            {
                session.ID = str.Trim();
                session.Timeout = DEFAULT_SESSION_TIMOUT_SECS;
            }

            return session;
        }

        /// <summary>
        /// Creates a new session instance from an id.  The string must not contain
        /// the timeout value.  Uses the default session timeout of 60 seconds.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Session FromId(string id)
        {
            return new Session { ID = id, Timeout = DEFAULT_SESSION_TIMOUT_SECS };
        }

        /// <summary>
        /// Creates a new session instance from its parts.
        /// </summary>
        /// <param name="id">The session's id</param>
        /// <param name="timeout">The session's timout value.</param>
        /// <returns></returns>
        public static Session FromParts(string id, uint timeout)
        {
            return new Session() { ID = id, Timeout = timeout };
        }
    }
}
