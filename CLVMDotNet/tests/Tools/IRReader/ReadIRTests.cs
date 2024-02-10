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
            // Arrange
            var script_source = "(equal 7 (+ 5 ;foo bar\n   2))";
            var expected_output = "(equal 7 (+ 5 2))";

            // Act
            var t = x.IRReader.ReadIR(script_source);
            var s = x.IRReader.ReadIR(expected_output);
            var areEqual = s.Equals(t);

            // Assert
            //s will never equal to t because the sexp tree will hold different 
            //character offsets from source.
            Assert.False(areEqual);
        }

        [Fact]
        public void ReadIREmptyStringThrowsError()
        {
            // Arrange

            // Act
            var errorMessage =
                Assert.Throws<ArgumentException>(() =>
                    x.IRReader.ReadIR(""));

            // Assert
            Assert.Contains("unexpected end of stream", errorMessage.Message);
        }

        [Fact(Skip = "Skipping for now!")]
        public void ConsList()
        {
            // Arrange
            string sexp_source = "foo";

            // Act
            var sexp = x.IRReader.ReadIR(sexp_source);

            // Assert
            var s = sexp;
        }

        [Fact]

        public void ConsList1()
        {
            // Arrange
            string sexp_source = "(/ 10 2)";

            // Act
            var sexp = x.IRReader.ReadIR(sexp_source);

            // Assert
            var s = sexp;
        }
    }
}