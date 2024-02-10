using Xunit;
using x = CLVMDotNet.CLVM;

namespace CLVMDotNet.Tests.CLVM.Operators;

public class KeywordFromAtomTests
{
    [Theory]
    [InlineData(01, "q")]
    [InlineData(2, "a")]
    [InlineData(3, "i")]
    [InlineData(4, "c")]
    [InlineData(5, "f")]
    [InlineData(0x06, "r")]
    [InlineData(0x07, "l")]
    [InlineData(0x08, "x")]
    [InlineData(0x0b, "sha256")]
    [InlineData(0x0c, "substr")]
    [InlineData(0x0d, "strlen")]
    [InlineData(0x0e, "concat")]
    [InlineData(0x0f, ".")]
    [InlineData(0x10, "+")]
    [InlineData(0x11, "-")]
    [InlineData(0x12, "*")]
    [InlineData(0x13, "/")]
    [InlineData(0x14, "divmod")]
    [InlineData(0x15, ">")]
    [InlineData(0x16, "ash")]
    [InlineData(0x17, "lsh")]
    [InlineData(0x18, "logand")]
    [InlineData(0x19, "logior")]
    [InlineData(0x1a, "logxor")]
    [InlineData(0x1b, "lognot")]
    [InlineData(0x1c, ".")]
    [InlineData(0x1d, "point_add")]
    [InlineData(0x1e, "pubkey_for_exp")]
    [InlineData(0x1f, ".")]
    [InlineData(0x20, "not")]
    [InlineData(0x21, "any")]
    [InlineData(0x22, "all")]
    [InlineData(0x23, ".")]
    public void KeywordToAtom_Returns_correct_byte(byte atom, string expectedKeyword)
    {
        var bytes = new byte[] { atom };
        var result = x.Operators.KEYWORD_FROM_ATOM[bytes];
        Assert.Equal(expectedKeyword, result);
    }
    
    [Fact]
    public void UnknownAtom_ThrowsError()
    {
        // Arrange
        
        // Act
        var errorMessage =
            Assert.Throws<KeyNotFoundException>(() =>
                x.Operators.KEYWORD_FROM_ATOM[new byte[] {0xaa}]);
        
        // Assert
        Assert.Contains("was not present in the dictionary", errorMessage.Message);
    }
}