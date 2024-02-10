using Xunit;
using x = CLVMDotNet.CLVM;

namespace CLVMDotNet.Tests.CLVM.HelperFunctions;

[Trait("HelperFunctions","ConvertAtomToBytes")]
public class ConvertAtomToBytesTests
{
    [Theory]
    [InlineData("@", new byte[] { 0x40})]
    [InlineData("unquote", new byte[] { 0x75, 0x6E, 0x71, 0x75, 0x6F, 0x74, 0x65})]
    [InlineData("qq", new byte[] { 0x71, 0x71  })]
    [InlineData("quote", new byte[] { 0x71, 0x75, 0x6F, 0x74, 0x65 })]
    public void ConvertStringToBytes_returnsCorrectBytes(string val, byte[] expectedBytes)
    {
        // Arrange
        
        // Act
        var bytes = x.HelperFunctions.ConvertAtomToBytes(val);

        // Assert
        Assert.True(bytes.SequenceEqual(expectedBytes));

    }
}