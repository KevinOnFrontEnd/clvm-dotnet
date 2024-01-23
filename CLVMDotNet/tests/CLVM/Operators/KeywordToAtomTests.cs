using Xunit;
using clvm = CLVMDotNet.CLVM.Operator;

namespace CLVMDotNet.Tests.CLVM.Operators;

public class KeywordToAtomTests
{
    [Theory]
    [InlineData(".", 0x23)]
    [InlineData("a", 0x02)]
    [InlineData("q", 0x01)]
    [InlineData("i", 0x03)]
    [InlineData("c", 0x04)]
    [InlineData("f", 0x05)]
    [InlineData("r", 0x06)]
    [InlineData("l", 0x07)]
    [InlineData("x", 0x08)]
    public void KeywordToAtom_Returns_correct_byte(string keyword, byte expectedByte)
    {
        var result = clvm.KEYWORD_TO_ATOM(keyword);
        Assert.Equal(result, expectedByte);
    }
    
    [Fact]
    public void UnknownKeyword_ThrowsError()
    {
        // Arrange
        
        // Act
        var errorMessage =
            Assert.Throws<Exception>(() =>
                clvm.KEYWORD_TO_ATOM("SomeInvalidKeyword"));
        
        // Assert
        Assert.Contains("Invalid Keyword", errorMessage.Message);
    }
}

