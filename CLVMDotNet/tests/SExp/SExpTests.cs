using System.Text;
using CLVM = CLVMDotNet;
using Xunit;

namespace CLVMDotNet.Tests.SExp;
public class SExpTests
{
    [Fact]
    public void TestWrapSExp()
    {
        var sexp = CLVM.SExp.To(1);
        CLVM.CLVMObject o = new CLVM.CLVMObject(sexp);
        Assert.True(o.Atom.Equals(1));
    }
    
    [Fact]
    public void TestStringConversions()
    {
        var a = CLVM.SExp.To("foobar");
        byte[] expectedOutput = Encoding.UTF8.GetBytes("foobar");
        byte[] result = a.AsAtom();
        Assert.Equal(expectedOutput, result);
    }
}