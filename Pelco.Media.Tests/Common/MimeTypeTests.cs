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
