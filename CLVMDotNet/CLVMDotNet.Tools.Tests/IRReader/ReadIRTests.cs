using Xunit;
using x = CLVMDotNet.Tools.IR;

namespace CLVMDotNet.Tools.Tests.IRReader;

[Trait("IRReader", "ReadIR")]
public class ReadIRTests
{
    [Fact]
    public void Tokenize_comments()
    {
        var script_source = "(equal 7 (+ 5 ;foo bar\n   2))";
        var expected_output = "(equal 7 (+ 5 2))";
        var t = x.IRReader.ReadIR(script_source);
        // s = Tokewrite_ir(t)
        // Assert.Equal(s, expected_output);
    }
}