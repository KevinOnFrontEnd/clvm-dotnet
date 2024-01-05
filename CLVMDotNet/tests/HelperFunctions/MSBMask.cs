using Xunit;
using CLVM = CLVMDotNet;

namespace CLVMDotNet.Tests.HelperFunctions
{

    [Trait("HelperFunctions", "MSBMask")]
    public class MSBMask
    {
        [Theory]
        [InlineData(0x00, 0x00)]
        // [InlineData(0x01, 0x01)]
        // [InlineData(0x02, 0x02)]
        // [InlineData(0x04, 0x04)]
        // [InlineData(0x08, 0x08)]
        // [InlineData(0x10, 0x10)]
        // [InlineData(0x20, 0x20)]
        // [InlineData(0x40, 0x40)]
        // [InlineData(0x80, 0x80)]
        // [InlineData(0x40, 0x44)]
        // [InlineData(0x20, 0x2A)]
        // [InlineData(0x80, 0xFF)]
        public void TestMsbMask(byte expectedbyte, byte MSB)
        {
            Assert.Equal(expectedbyte, CLVM.HelperFunctions.MSBMask(MSB));
        }
    }
}