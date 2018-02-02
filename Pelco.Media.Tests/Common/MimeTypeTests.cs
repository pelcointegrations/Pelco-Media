//
// Copyright (c) 2018 Pelco. All rights reserved.
//
// This file contains trade secrets of Pelco.  No part may be reproduced or
// transmitted in any form by any means or for any purpose without the express
// written permission of Pelco.
//
using Pelco.Media.Common;
using Xunit;

namespace Pelco.Media.Tests.Common
{
    public class MimeTypeTests
    {
        [Fact]
        public void TestParseMimeTypeString()
        {
            var type = "application/vnd.opencv.facial_detection";

            var mimeType = MimeType.Parse(type);
            Assert.Equal("application", mimeType.Type);
            Assert.Equal("vnd.opencv.facial_detection", mimeType.Subtype);

            type = "application       / vnd.opencv.facial_detection";
            mimeType = MimeType.Parse(type);
            Assert.Equal("application", mimeType.Type);
            Assert.Equal("vnd.opencv.facial_detection", mimeType.Subtype);

            type = "application";
            mimeType = MimeType.Parse(type);
            Assert.Equal("application", mimeType.Type);
            Assert.Equal("*", mimeType.Subtype);

            type = "application/";
            mimeType = MimeType.Parse(type);
            Assert.Equal("application", mimeType.Type);
            Assert.Equal("*", mimeType.Subtype);
        }
    }
}
