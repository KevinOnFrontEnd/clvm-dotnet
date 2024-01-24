using Xunit;
using clvm = CLVMDotNet.CLVM;

namespace CLVMDotNet.Tests.CLVM;

public class KeywordToAtomTests
{
    [Theory]
    [InlineData(".", 0x00)]
    [InlineData("a", 0x02)]
    [InlineData("q", 0x01)]
    [InlineData("i", 0x03)]
    [InlineData("c", 0x04)]
    [InlineData("f", 0x05)]
    [InlineData("r", 0x06)]
    [InlineData("l", 0x07)]
    [InlineData("x", 0x08)]
    [InlineData("=", 0x09)]
    [InlineData("sha256", 0x0b)]
    [InlineData("substr", 0x0c)]
    [InlineData("strlen", 0x0d)]
    [InlineData("concat", 0x0e)]
    // [InlineData("#", 0x0f)]
    [InlineData("+", 0x10)]
    [InlineData("-", 0x11)]
    [InlineData("*", 0x12)]
    [InlineData("/", 0x13)]
    [InlineData("divmod", 0x14)]
    [InlineData(">", 0x15)]
    [InlineData("ash", 0x16)]
    [InlineData("lsh", 0x17)]
    [InlineData("logand", 0x18)]
    [InlineData("logior", 0x19)]
    [InlineData("logxor", 0x1a)]
    [InlineData("lognot", 0x1b)]
    [InlineData("point_add", 0x1d)]
    [InlineData("pubkey_for_exp", 0x1e)]
    [InlineData("not", 0x20)]
    [InlineData("any", 0x21)]
    [InlineData("all", 0x22)]
    public void KeywordToAtom_Returns_correct_byte(string keyword, byte expectedByte)
    {
        var result = clvm.Keywords.KEYWORD_TO_ATOM[keyword];
        Assert.Equal(expectedByte, result);
    }
    
    [Fact]
    public void UnknownKeyword_ThrowsError()
    {
        // Arrange
        
        // Act
        var errorMessage =
            Assert.Throws<KeyNotFoundException>(() =>
                clvm.Keywords.KEYWORD_TO_ATOM["SomeInvalidKeyword"]);
        
        // Assert
        Assert.Contains("given key 'SomeInvalidKeyword' was not present in the dictionary", errorMessage.Message);
    }
}

