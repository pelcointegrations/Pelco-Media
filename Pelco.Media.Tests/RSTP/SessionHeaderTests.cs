//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.RTSP;
using Xunit;

namespace Pelco.Media.Tests.RSTP
{
    public class SessionHeaderTests
    {
        [Fact]
        public void TestParseWithoutTimeout()
        {
            Session sess = Session.Parse("test123");
            Assert.Equal("test123", sess.ID);
            Assert.Equal(60u, sess.Timeout);
        }

        [Fact]
        public void TestParseWithTimeout()
        {
            Session sess = Session.Parse("test123;timeout=900");
            Assert.Equal("test123", sess.ID);
            Assert.Equal(900u, sess.Timeout);
        }

        [Fact]
        public void TestInvalidHeaderParamater()
        {
            Session sess = Session.Parse("test123;");
            Assert.Equal("test123", sess.ID);
            Assert.Equal(60u, sess.Timeout);
        }

        [Fact]
        public void TestInvalidTimeout()
        {
            Session sess = Session.Parse("test123;timeout=skdfjlf");
            Assert.Equal("test123", sess.ID);
            Assert.Equal(60u, sess.Timeout);

            sess = Session.Parse("test123;timeout=");
            Assert.Equal("test123", sess.ID);
            Assert.Equal(60u, sess.Timeout);
        }
    }
}
