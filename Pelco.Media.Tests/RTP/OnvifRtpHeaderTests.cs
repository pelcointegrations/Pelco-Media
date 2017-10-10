using Pelco.Media.RTP;
using System;
using Xunit;

namespace Pelco.Media.Tests.RTP
{
    public class OnvifRtpHeaderTests
    {
        [Fact]
        public void TestAllBitsSet()
        {
            var date = new DateTime(2017, 10, 17, 4, 33, 17, 32, DateTimeKind.Local);

            var buffer = new OnvifRtpHeader()
            {
                Time = date,
                CbitSet = true,
                DbitSet = true,
                EbitSet = true,
                CSeq = 3
            }.Encode();

            var header = OnvifRtpHeader.Decode(buffer);

            Assert.Equal(date, header.Time.ToLocalTime());
            Assert.True(header.CbitSet);
            Assert.True(header.DbitSet);
            Assert.True(header.EbitSet);
            Assert.Equal(3, header.CSeq);
        }

        [Fact]
        public void TestOnlyCbitSet()
        {
            var date = new DateTime(2017, 10, 17, 4, 33, 17, 32, DateTimeKind.Local);

            var buffer = new OnvifRtpHeader()
            {
                Time = date,
                CbitSet = true,
                DbitSet = false,
                EbitSet = false,
                CSeq = 3
            }.Encode();

            var header = OnvifRtpHeader.Decode(buffer);

            Assert.Equal(date, header.Time.ToLocalTime());
            Assert.True(header.CbitSet);
            Assert.False(header.DbitSet);
            Assert.False(header.EbitSet);
            Assert.Equal(3, header.CSeq);
        }

        [Fact]
        public void TestOnlyDbitSet()
        {
            var date = new DateTime(2017, 10, 17, 4, 33, 17, 32, DateTimeKind.Local);

            var buffer = new OnvifRtpHeader()
            {
                Time = date,
                CbitSet = false,
                DbitSet = true,
                EbitSet = false,
                CSeq = 3
            }.Encode();

            var header = OnvifRtpHeader.Decode(buffer);

            Assert.Equal(date, header.Time.ToLocalTime());
            Assert.False(header.CbitSet);
            Assert.True(header.DbitSet);
            Assert.False(header.EbitSet);
            Assert.Equal(3, header.CSeq);
        }

        [Fact]
        public void TestOnlyEbitSet()
        {
            var date = new DateTime(2017, 10, 17, 4, 33, 17, 32, DateTimeKind.Local);

            var buffer = new OnvifRtpHeader()
            {
                Time = date,
                CbitSet = false,
                DbitSet = false,
                EbitSet = true,
                CSeq = 3
            }.Encode();

            var header = OnvifRtpHeader.Decode(buffer);

            Assert.Equal(date, header.Time.ToLocalTime());
            Assert.False(header.CbitSet);
            Assert.False(header.DbitSet);
            Assert.True(header.EbitSet);
            Assert.Equal(3, header.CSeq);
        }
    }
}
