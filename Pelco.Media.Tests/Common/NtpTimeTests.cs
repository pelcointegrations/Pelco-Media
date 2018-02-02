//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.Common;
using System;
using Xunit;

namespace Pelco.Media.Tests.Common
{
    public class NtpTimeTests
    {
        [Fact]
        public void TestConvertToNtp()
        {
            DateTime date = new DateTime(2017, 10, 17, 4, 33, 17, 32, DateTimeKind.Local);
            NtpTime ntpTime = new NtpTime(date);

            Assert.Equal(3717225197, ntpTime.Seconds);
            Assert.Equal((uint)137438953, ntpTime.Fraction);
            Assert.Equal(date, ntpTime.UtcDate.ToLocalTime());

            date = new DateTime(2012, 8, 2, 8, 52, 1, 43);
            ntpTime = new NtpTime(date);

            Assert.Equal(3552907921, ntpTime.Seconds);
            Assert.Equal((uint)184683593, ntpTime.Fraction);
            Assert.Equal(date, ntpTime.UtcDate.ToLocalTime());
        }
    }
}
