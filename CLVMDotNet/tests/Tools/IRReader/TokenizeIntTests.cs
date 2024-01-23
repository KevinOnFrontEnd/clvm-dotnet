using Xunit;
using x = CLVMDotNet.Tools.IR;

namespace CLVMDotNet.Tests.Tools.IRReader;

public class TokenizeIntTests
{
    [Fact]
    public void TokenizeInt_returns_null_when_passing_none_int()
    {
        // Arrange
        var noneNumber = "a";
            
        // Act
        var tokenized = x.IRReader.TokenizeInt(noneNumber, 0);

        // Assert
        Assert.Null(tokenized);
    }
    
    // [Fact]
    // public void TokenizeInt_returns_null_when_passing_none_int()
    // {
    //     // Arrange
    //     var noneNumber = "a";
    //         
    //     // Act
    //     var tokenized = x.IRReader.TokenizeInt(noneNumber, 0);
    //
    //     // Assert
    //     Assert.Null(tokenized);
    // }
}