using System;
using System.Collections.Immutable;

namespace Pelco.Media.RTSP.Utils
{
    /// <summary>
    /// Decodes header values of the form 'name=value, name2=value2, etc...' into
    /// a dictionary of key value pairs.
    /// </summary>
    public sealed class HeaderValueDecoder
    {
        private HeaderValueDecoder()
        {
        }
        
        /// <summary>
        /// Decodes a string of key value pairs seperated by a ',' into a dictionary.
        /// If a key value pair string does not provide a value, then the entire string is added to
        /// the name and an emtry string is added for the value.
        /// </summary>
        /// <param name="value">The string to decode</param>
        /// <returns></returns>
        public static  ImmutableDictionary<string, string> Decode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Unable to decode null or empty header value");
            }

            var builder = ImmutableDictionary.CreateBuilder<string, string>();

            foreach (var param in value.Split(new char[] { ',' }))
            {
                int index = param.IndexOf('=');
                if (index == -1)
                {
                    // Assume an empty value, just add the key with an empty string value.
                    builder.Add(param.Trim(), string.Empty);
                }

                // Parse out name and value.  Stripping any " characters from value.  This results in realm="this realm" is
                // being converted to key=realm, value=this realm (not "this realm").
                builder.Add(param.Substring(0, index).Trim(), param.Substring(index + 1).Trim().Replace("\"", ""));
            }

            return builder.ToImmutable();
        }
    }
}
