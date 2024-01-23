using System.Numerics;
using Xunit;
using x = CLVMDotNet.CLVM;

namespace CLVMDotNet.Tests.CLVM.Casts
{
    [Trait("Casts", "IntToBytes")]
    public class IntToBytes
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
        [InlineData("-1", new byte[] { 255 })]
        [InlineData("-128", new byte[] { 0x80 })]
        [InlineData("-129", new byte[] { 0xFF, 0x7F })]
        [InlineData("-256", new byte[] { 0xFF, 0x00 })]
        [InlineData("-512", new byte[] { 0xFE, 0x00 })]
        [InlineData("-1024", new byte[] { 0xFC, 0x00 })]
        [InlineData("-2048", new byte[] { 0xF8, 0x00 })]
        [InlineData("-4096", new byte[] { 0xF0, 0x00 })]
        [InlineData("-10241024", new byte[] { 0xFF, 0x63, 0xBC, 0x00 })]
        [InlineData("-204820482048", new byte[] { 0xD0, 0x4F, 0xBF, 0x78, 0x00 })]
        [InlineData("-20482048204820482048", new byte[] { 0xFE, 0xE3, 0xC1, 0x25, 0xAD, 0x1F, 0x3F, 0x78, 0x00 })]
        //[InlineData("-204820482048204820482048204820482048204820482048204820482048", new byte[]
        // {
        //     0xDF, 0x5E, 0xC6, 0x63, 0xAD, 0x6F, 0xBA, 0x4E, 0x4A, 0x38, 0x39, 0x33, 0xCA, 0xD0, 0x3C, 0x22,
        //     0xCE, 0x56, 0x2A, 0x58, 0xBD, 0x0C, 0x9F, 0x3F, 0x78, 0x00
        // })]
        public void IntToBytes_returns_expectedbytes(string numberStr, byte[] expectedBytes)
        {
            // Arrange
            BigInteger number = BigInteger.Parse(numberStr);
            
            // Act
            var returnedBytes = x.Casts.IntToBytes(number);
            
            // Assert
            Assert.Equal(expectedBytes, returnedBytes);
        }
    }
}