using System.Numerics;
using Xunit;
using x = CLVMDotNet.CLVM;

namespace CLVMDotNet.Tests.CLVM.Casts
{

    [Trait("Casts", "IntFromBytes")]
    public class IntFromBytes
    {
        [Theory]
        [InlineData("0", new byte[] { })]
        [InlineData("1", new byte[] { 0x01 })]
        [InlineData("8", new byte[] { 0x08 })]
        [InlineData("16", new byte[] { 0x10 })]
        [InlineData("32", new byte[] { 32 })]
        [InlineData("64", new byte[] { 64 })]
        [InlineData("128", new byte[] { 0x00, 0x80 })]
        [InlineData("256", new byte[] { 0x01, 0x00 })]
        [InlineData("512", new byte[] { 0x02, 0x00 })]
        [InlineData("1024", new byte[] { 0x04, 0x00 })]
        [InlineData("2048", new byte[] { 0x08, 0x00 })]
        [InlineData("4096", new byte[] { 0x10, 0x00 })]
        [InlineData("10241024", new byte[] { 0x00, 0x9c, 0x44, 0x00 })]
        [InlineData("204820482048", new byte[] { 0x2F, 0xB0, 0x40, 0x88, 0x00 })]
        [InlineData("20482048204820482048", new byte[] { 0x01, 0x1C, 0x3E, 0xDA, 0x52, 0xE0, 0xC0, 0x88, 0x00 })]
        public void IntFromBytes_returns_expectedint(string expectedNumberStr, byte[] from_bytes)
        {
            // Arrange
            BigInteger expectedNumber = BigInteger.Parse(expectedNumberStr);
            
            // Act
            var returnedBytes = x.Casts.IntFromBytes(from_bytes);
            
            // Assert
            Assert.Equal(expectedNumber, returnedBytes);
        }
    }
}