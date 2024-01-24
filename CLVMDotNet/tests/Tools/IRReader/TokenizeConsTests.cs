using Xunit;
using x = CLVMDotNet.Tools.IR;

namespace CLVMDotNet.Tests.Tools.IRReader
{

    public class TokenizeConsTests
    {
        [Fact]
        public void TokenizeCons_ClosingBracket()
        {
            // Arrange
            var stream = x.IRReader.TokenStream("(+ 1 3)");

            // Act
            var tokenized = x.IRReader.TokenizeCons(")", 6, stream.GetEnumerator());

            // Assert
            Assert.Null(tokenized.Atom);
            Assert.NotNull(tokenized.Pair);
            Assert.NotNull(tokenized.AsPair().Item1);
            Assert.Null(tokenized.AsPair().Item1.Atom);
            Assert.NotNull(tokenized.AsPair().Item1.Pair);
            Assert.NotNull(tokenized.AsPair().Item1.AsPair());
            Assert.True(tokenized.AsPair().Item1.AsPair().Item1.Atom.SequenceEqual(new byte[] { 78, 85, 76, 76 }));
            Assert.True(tokenized.AsPair().Item2.Atom.SequenceEqual(new byte[] { }));
        }

        // [Theory]
        // [InlineData("1", new byte[] { 0x01})]
        // [InlineData("256", new byte[] { 0x01, 0x00 })]
        // [InlineData("12345678910", new byte[] { 0x02, 0xdf, 0xdc, 0x1c, (byte)'>'})]
        // [InlineData("1234567891012345678910", new byte[] { 0x42, 0xed, 0x12, 0x3b, 0xda, 0xce, 0x08, 0x4c, (byte)'>' })]
        // public void TokenizeInt_Builds_CorrectObject(string number, byte[] expectedBytes)
        // {
        //     // Arrange
        //
        //     // Act
        //     var tokenized = x.IRReader.TokenizeInt(number, 0);
        //
        //     // Assert
        //     Assert.Null(tokenized.Atom);
        //     Assert.NotNull(tokenized.Pair);
        //     Assert.Null(tokenized.AsPair().Item1.Atom);
        //     Assert.NotNull(tokenized.AsPair().Item1);
        //     Assert.NotNull(tokenized.AsPair().Item1.AsPair());
        //     
        //     //INT as bytes
        //     Assert.True(tokenized.AsPair().Item1.AsPair().Item1.Atom.SequenceEqual(new byte[] { 73, 78, 84})); // INT
        //     
        //     //NUMBER as bytes
        //     Assert.True(tokenized.AsPair().Item2.Atom.SequenceEqual(expectedBytes)); // INT
        // }
    }
}