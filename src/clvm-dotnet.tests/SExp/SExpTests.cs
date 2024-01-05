using System.Text;
using clvm = clvm_dotnet;
using Xunit;

namespace clvm_dotnet.tests;
public class SExpTests
{
    [Fact]
    public void TestWrapSExp()
    {
        var sexp = clvm.SExp.To(1);
        CLVMObject o = new CLVMObject(sexp);
        Assert.True(o.Atom.Equals(1));
    }
    
    [Fact]
    public void TestStringConversions()
    {
        var a = clvm.SExp.To("foobar");
        byte[] expectedOutput = Encoding.UTF8.GetBytes("foobar");
        byte[] result = a.AsAtom();
        Assert.Equal(expectedOutput, result);
    }
}