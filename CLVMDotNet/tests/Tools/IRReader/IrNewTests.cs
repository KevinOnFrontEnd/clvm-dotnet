using Xunit;
using x = CLVMDotNet.Tools.IR;

namespace CLVMDotNet.Tests.Tools.IRReader
{

    public class IrNewTests
    {
        [Fact]
        public void IrNewWithValidIntegerAndOffset()
        {
            // arrange
            
            // act
            var result = x.Utils.IrNew(x.IRType.INT, 100, 1);
            
            // assert
            Assert.Null(result.Atom);
            Assert.NotNull(result.Pair);
            Assert.NotNull(result.Pair.Item1);
            Assert.True(result.AsPair().Item1.AsPair().Item1.Atom.SequenceEqual(new byte[] { 73, 78, 84 }));  //INT
            Assert.True(result.AsPair().Item1.AsPair().Item2.Atom.SequenceEqual(new byte[] { 0x01 }));  //1 (offset)
            Assert.True(result.AsPair().Item2.Atom.SequenceEqual(new byte[] {0x64})); //100
        }
    }
}