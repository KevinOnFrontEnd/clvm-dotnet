using System.Numerics;
using Xunit;
using CLVM = CLVMDotNet;

namespace CLVMDotNet.Tests.Casts
{
    [Trait("Casts", "LimbsForInt")]
    public class LimbsForInt
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(8, 1)]
        [InlineData(16, 1)]
        [InlineData(32, 1)]
        [InlineData(64, 1)]
        [InlineData(128, 2)]
        [InlineData(1024, 2)]
        [InlineData(10241024, 4)]
        [InlineData(204820482048, 5)]
        // [InlineData(20482048204820482048, new byte[]  { 0x01, 0x1C, 0x3E, 0xDA, 0x52, 0xE0, 0xC0, 0x88, 0x00 } )]
        public void LimbsForInt_returns_expectedlength(long num, int expectedNumberOfBytes)
        {
            var numberOfBytes = CLVM.Casts.LimbsForInt(num);
            Assert.Equal(expectedNumberOfBytes, numberOfBytes);
        }
    }
}