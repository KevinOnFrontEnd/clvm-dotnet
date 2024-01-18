using System.Numerics;
using Xunit;
using x = CLVMDotNet.CLVM;

namespace CLVMDotNet.Tests.CLVM.Casts
{
    [Trait("Casts", "LimbsForInt")]
    public class LimbsForInt
    {
        [Theory]
        [InlineData("0", 0)]
        [InlineData("1", 1)]
        [InlineData("8", 1)]
        [InlineData("16", 1)]
        [InlineData("32", 1)]
        [InlineData("64", 1)]
        [InlineData("128", 2)]
        [InlineData("1024", 2)]
        [InlineData("10241024", 4)]
        [InlineData("204820482048", 5)]
        [InlineData("20482048204820482048", 9)]
        public void LimbsForInt_returns_expectedlength(string numStr, int expectedNumberOfBytes)
        {
            BigInteger num = BigInteger.Parse(numStr);
            var numberOfBytes = x.Casts.LimbsForInt(num);
            Assert.Equal(expectedNumberOfBytes, numberOfBytes);
        }
    }
}