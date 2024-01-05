using Xunit;

namespace clvm_dotnet.tests;
using clvm = clvm_dotnet;

[Trait("SExp","AtomFromStream")]
public class AtomFromStream
{
    [Fact]
    public void AtomFromStreamEmptyString()
    {
        byte b = 0x80;
        var sexp = clvm.Serialize.AtomFromStream(null, b,typeof(CLVMObject));
        Assert.Equal(Array.Empty<byte>(), sexp.Atom);
    }

    [Fact]
    public void AtomFromStreamMaxSingleByte()
    {
        byte b = 0x7F;
        byte[] byteArray = new byte[] { 0x7F };
        var sexp = clvm.Serialize.AtomFromStream(null, b, typeof(CLVMObject));
        Assert.Equal(byteArray, sexp.Atom);
    }
}