using Xunit;
using x = CLVMDotNet.Tools.IR;

namespace CLVMDotNet.Tools.Tests.IRReader;

[Trait("IRReader", value:"TokenStream")]
public class TokenStreamTests
{
    [Theory]
    [InlineData("(equal 7 (+ 5 ;foo bar\n   2))", 9)]
    [InlineData("(equal 7 (+ 5 ;foo bar\n \n\n\n \n\n\r  2))", 9)]
    [InlineData("(equal 7 (+ 5 ;foo bar\n \n\n\n \n\n\r  8))", 9)]
    [InlineData("(equal 7 (+ 5 (+ 1 5)))", 13)]
    [InlineData("(- 5 4)", 5)]
    [InlineData("(- 5 (- 6 (- 5 (- 5 6))))", 17)]
    public void TokenStreamReader_matches_python_stream_length(string input, int lengthOfStream)
    {
        var stream = x.IRReader.TokenStream(input);
        var count = stream.Count();
        Assert.Equal(lengthOfStream, count);
    }
    
}