using Xunit;
using x = CLVMDotNet.CLVM;

namespace CLVMDotNet.Tests.CLVM.Operators;

public class KeywordFromAtomTests
{
    [Theory]
    [InlineData(0x23, ".")]
    [InlineData(0x02, "a")]
    [InlineData(0x01, "q")]
    [InlineData(0x03, "i")]
    [InlineData(0x04, "c")]
    [InlineData(0x05, "f")]
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
    public void KeywordToAtom_Returns_correct_byte(byte atom, string expectedKeyword)
    {
        var result = x.Keywords.KEYWORD_FROM_ATOM[atom];
        Assert.Equal(expectedKeyword, result);
    }
    
    [Fact]
    public void UnknownAtom_ThrowsError()
    {
        // Arrange
        
        // Act
        var errorMessage =
            Assert.Throws<KeyNotFoundException>(() =>
                x.Keywords.KEYWORD_FROM_ATOM[0xaa]);
        
        // Assert
        Assert.Contains("The given key '170' was not present in the dictionary", errorMessage.Message);
    }
}