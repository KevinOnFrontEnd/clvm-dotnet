using System.Numerics;
using System.Text;
using CLVMDotNet.Tools.IR;
using Xunit;
using x = CLVMDotNet.Tools.IR;
namespace CLVMDotNet.Tools.Tests.BinUtils;

[Trait("BinUtils","TypeForAtom")]
public class TypeForAtomTests
{
    // [Theory]
    // [InlineData(4736344, "abdde")] //Quote
    // [InlineData(4736344, "134567")] //Hex
    // [InlineData(4736344,"23456")] //Int
    // public void TestTypeForAtomReturnsCorrectType(BigInteger type, string value)
    // {
    //     byte[] bytes  = Encoding.UTF8.GetBytes(value);
    //     var t = x.BinUtils.TypeForAtom(bytes);
    //     Assert.Equal(type, t);
    // }
}