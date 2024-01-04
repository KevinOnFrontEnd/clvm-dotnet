using System.Numerics;
using Xunit;

namespace clvm_dotnet.tests;

public class CastsTests
{
    [Theory]
    [InlineData(0, new byte[] { })]
    [InlineData(1, new byte[] { 0x01 })]
    [InlineData(8, new byte[] { 0x08 })]
    [InlineData(16, new byte[] { 0x10 })]
    [InlineData(32, new byte[] { 32 })]
    [InlineData(64, new byte[] { 64 })]
    [InlineData(128, new byte[] { 0x00, 0x80 })]
    [InlineData(256, new byte[] { 0x01, 0x00 })]
    [InlineData(512, new byte[] { 0x02, 0x00 })]
    [InlineData(1024, new byte[] { 0x04, 0x00 })]
    [InlineData(2048, new byte[] { 0x08, 0x00 })]
    [InlineData(4096, new byte[] { 0x10, 0x00 })]
    [InlineData(10241024, new byte[] { 0x00, 0x9c, 0x44, 0x00 })]
    [InlineData(204820482048, new byte[] { 0x2F, 0xB0, 0x40, 0x88, 0x00 })]
    // [InlineData(20482048204820482048, new byte[]  { 0x01, 0x1C, 0x3E, 0xDA, 0x52, 0xE0, 0xC0, 0x88, 0x00 } )]
    public void IntToBytes_returns_expectedbytes(BigInteger number, byte[] expectedBytes)
    {
        var returnedBytes = Casts.IntToBytes(number);
        Assert.Equal(expectedBytes, returnedBytes);
    }

    [Theory]
    [InlineData(0, new byte[] { })]
    [InlineData(1, new byte[] { 0x01 })]
    [InlineData(8, new byte[] { 0x08 })]
    [InlineData(16, new byte[] { 0x10 })]
    [InlineData(32, new byte[] { 32 })]
    [InlineData(64, new byte[] { 64 })]
    [InlineData(128, new byte[] { 0x00, 0x80 })]
    [InlineData(256, new byte[] { 0x01, 0x00 })]
    [InlineData(512, new byte[] { 0x02, 0x00 })]
    [InlineData(1024, new byte[] { 0x04, 0x00 })]
    [InlineData(2048, new byte[] { 0x08, 0x00 })]
    [InlineData(4096, new byte[] { 0x10, 0x00 })]
    [InlineData(10241024, new byte[] { 0x00, 0x9c, 0x44, 0x00 })]
    [InlineData(204820482048, new byte[] { 0x2F, 0xB0, 0x40, 0x88, 0x00 })]
    // [InlineData(20482048204820482048, new byte[]  { 0x01, 0x1C, 0x3E, 0xDA, 0x52, 0xE0, 0xC0, 0x88, 0x00 } )]
    public void IntFromBytes_returns_expectedint(BigInteger expected_number, byte[] from_bytes)
    {
        var returnedBytes = Casts.IntFromBytes(from_bytes);
        Assert.Equal(expected_number, returnedBytes);
    }

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
        var numberOfBytes = Casts.LimbsForInt(num);
        Assert.Equal(expectedNumberOfBytes, numberOfBytes);
    }
}