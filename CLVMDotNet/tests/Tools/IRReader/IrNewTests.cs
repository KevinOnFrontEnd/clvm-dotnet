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
            var val = "100";
            var offset = 1;
            
            // act
            var result = x.Utils.IrNew(x.IRType.INT, val, offset);
            
            // assert
            Assert.Null(result.Atom);
            Assert.NotNull(result.Pair);
            Assert.NotNull(result.Pair.Item1);
            Assert.True(result.AsPair()!.Item1.AsPair()!.Item1.Atom!.SequenceEqual(new byte[] { 73, 78, 84 }));  //INT
            Assert.True(result.AsPair()!.Item1.AsPair()!.Item2.Atom!.SequenceEqual(new byte[] { 0x01 }));  //1 (offset)
            Assert.True(result.AsPair()!.Item2.Atom!.SequenceEqual(new byte[] {49,48,48})); //100
        }
    }
}