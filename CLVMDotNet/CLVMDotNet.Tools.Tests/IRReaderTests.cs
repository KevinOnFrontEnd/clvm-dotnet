using Xunit;

namespace CLVMDotNet.Tools.Tests;

public class IRReaderTests
{
    [Fact]
    public void Tokenize_comments()
    {
        var script_source = "(equal 7 (+ 5 ;foo bar\n   2))";
        var expected_output = "(equal 7 (+ 5 2))";
        var t = Tokenizer.ReadIR(script_source);
        // s = Tokewrite_ir(t)
        // Assert.Equal(s, expected_output);
    }
}