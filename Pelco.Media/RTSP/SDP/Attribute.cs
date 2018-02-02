//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using System.Text;
using System.Text.RegularExpressions;

namespace Pelco.Media.RTSP.SDP
{
    public class Attribute
    {
        public static readonly Regex REGEX = new Regex(@"a\s*=\s*(.+)$", RegexOptions.Compiled);

        public Attribute() : this(string.Empty, string.Empty)
        {

        }

        public Attribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder("a=").Append(Name);

            if (!string.IsNullOrEmpty(Value))
            {
                sb.Append(":").Append(Value);
            }

            return sb.ToString();
        }

        public static Attribute Parse(string line)
        {
            var match = REGEX.Match(line);
                
            if (!match.Success)
            {
                throw new SdpParseException($"Unable to parse attribute '{line}'");
            }

            var value = match.Groups[1].Value.Trim();

            int index = value.IndexOf(':');
            if (index == -1)
            {
                return new Attribute(value, string.Empty);
            }
            else
            {
                return new Attribute(value.Substring(0, index), value.Substring(index + 1));
            }
        }

        public static Builder CreateBuilder()
        {
            return new Builder();
        }

        public sealed class Builder
        {
            private string _name;
            private string _value;

            public Builder()
            {
                Clear();
            }

            public Builder Clear()
            {
                _name = string.Empty;
                _value = string.Empty;

                return this;
            }

            public Builder Name(string name)
            {
                _name = name;

                return this;
            }

            public Builder Value(string value)
            {
                _value = value;

                return this;
            }

            public Attribute Build()
            {
                return new Attribute(_name, _value);
            }
        }
    }
}
