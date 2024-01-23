using Xunit;
using x = CLVMDotNet.Tools.IR;

namespace CLVMDotNet.Tests.Tools.IRReader;

public class TokenizeSexpTests
{
    [Fact]
    public void TestTokenizeSexp()
    {
        // arrange
        var stream = x.IRReader.TokenStream("(100 0x0100)");
        
        // act
        foreach (var item in stream)
        {
            
        }
        
        // assert
    }
}