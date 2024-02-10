using Xunit;
using x = CLVMDotNet.Tools.IR;

namespace CLVMDotNet.Tests.Tools.IRReader
{

    public class TokenizeSymbolTests
    {
        [Fact]
        public void TokenizeSymbol_returns_sexp()
        {
            // Arrange
            var offset = 2;

            // Act
            var tokenized = x.IRReader.TokenizeSymbol("+", offset);

            // Assert
            Assert.Equal(x.IRType.SYMBOL, tokenized.Item1.Item1);
            Assert.Equal(offset, tokenized.Item1.Item2);
            Assert.True(tokenized.Item2.SequenceEqual(new byte[] { (byte)'+'}));
        }
    }
}