using CLVMDotNet.Tools;
using Xunit;
using x = CLVMDotNet.Tools.IR;
namespace CLVMDotNet.Tests.Tools.IRReader;

public class TokenizeHexTests
{
    [Fact]
    public void TokenizeHex_DoesNotLook_like_hex_returns_null()
    {
        // ArrangeA
        var invalidHex = "xxxx";
            
        // ActW
        var result = x.IRReader.TokenizeHex(invalidHex, 0);

        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void TokenizeHex_Invalid_Hex_throws_exception()
    {
        // ArrangeA
        var invalidHex = "0Xzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz";
        int offset = 1;
            
        // ActW
        var errorMessage =
            Assert.Throws<SyntaxException>(() =>
                x.IRReader.TokenizeHex(invalidHex, offset));

        // Assert
        Assert.Equal($"invalid hex at {offset}:{invalidHex}",errorMessage.Message);
    }
    
    [Theory]
    [InlineData("0x01", new byte[] { 0x01})]
    [InlineData("0x2DE684E38", new byte[] { 0x02, 0xde, 0x68, 0x4e, 0x38})]
    [InlineData("0x1BFEA1277EF63F25B9D65E79", new byte[] { 0x1b, 0xfe, 0xa1, (byte)'\'', (byte)'~', 0xf6, (byte)'?', (byte)'%', 0xb9, 0xd6, (byte)'^', (byte)'y'})]
    public void TokenizeHex_Valid(string number, byte[] expectedBytes)
    {
        // Arrange
    
        // Act
        var tokenized = x.IRReader.TokenizeHex(number, 0);
    
        // Assert
        Assert.Null(tokenized!.Atom);
        Assert.NotNull(tokenized.Pair);
        Assert.Null(tokenized.AsPair()!.Item1.Atom);
        Assert.NotNull(tokenized.AsPair()!.Item1);
        Assert.NotNull(tokenized.AsPair()!.Item1.AsPair()!);
        
        //HEX as bytes
        Assert.True(tokenized.AsPair()!.Item1.AsPair()!.Item1.Atom!.SequenceEqual(new byte[] { 72, 69, 88})); // HEX
        
        //NUMBER as bytes
        Assert.True(tokenized.AsPair()!.Item2.Atom!.SequenceEqual(expectedBytes)); //bytes
    }
}