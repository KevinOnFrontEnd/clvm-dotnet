using Xunit;
using x = CLVMDotNet.CLVM.Operator;

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
    public void KeywordToAtom_Returns_correct_byte(byte atom, string expectedKeyword)
    {
        var result = x.KEYWORD_FROM_ATOM(atom);
        Assert.Equal(result, expectedKeyword);
    }
    
    [Fact]
    public void UnknownAtom_ThrowsError()
    {
        var errorMessage =
            Assert.Throws<Exception>(() =>
                x.KEYWORD_FROM_ATOM(0xaa));
        Assert.Contains("Invalid Atom", errorMessage.Message);
    }
}