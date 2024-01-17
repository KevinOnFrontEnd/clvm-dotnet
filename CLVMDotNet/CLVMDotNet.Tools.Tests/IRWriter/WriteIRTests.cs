using Xunit;
using x = CLVMDotNet.Tools.IR;
using comm = CLVMDotNet.Tools;

namespace CLVMDotNet.Tools.Tests.IRWriter;

[Trait("IRWriter", "WriteIr")]
public class IRWriterTests
{
    [Theory]
    [InlineData("100")]
    //[InlineData("0x0100")]
    // [InlineData("0x100")]
    // [InlineData("\"100\"")]
    // [InlineData("the quick brown fox jumps over the lazy dogs")]
    // [InlineData("(the quick brown fox jumps over the lazy dogs)")]
    // [InlineData("foo")]
    // [InlineData("(100 0x0100)")]
    public void WriterTests(string sexpText)
    {
        var irSexp = x.IRReader.ReadIR(sexpText);
        var sexpTextNormalized = x.IRWRiter.WriteIr(irSexp);

        var irSexp2 = x.IRReader.ReadIR(sexpText);
        var sexpTextNormalized2 = x.IRWRiter.WriteIr(irSexp2); 
        Assert.Equal(sexpTextNormalized ,sexpTextNormalized2);
    }
}