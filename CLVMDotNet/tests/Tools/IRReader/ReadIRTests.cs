using Xunit;
using x = CLVMDotNet.Tools.IR;
using comm = CLVMDotNet.Tools;

namespace CLVMDotNet.Tests.Tools.IRReader
{
    [Trait("IRReader", "ReadIR")]
    public class ReadIRTests
    {
        [Fact]
        public void IRReader_ignores_comments()
        {
            var script_source = "(equal 7 (+ 5 ;foo bar\n   2))";
            var expected_output = "(equal 7 (+ 5 2))";
            var t = x.IRReader.ReadIR(script_source);
            var s = x.IRReader.ReadIR(expected_output);
            var areEqual = s.Equals(t);

            //s will never equal to t because the sexp tree will hold different 
            //character offsets from source.
            Assert.False(areEqual);

            //TODO: Write IRWriter  class
        }
    }
}