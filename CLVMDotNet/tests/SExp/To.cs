using System.Text;
using clvm_dotnet.tests;
using Xunit;
using clvm = CLVMDotNet;

namespace CLVMDotNet.Tests.SExp;

[Trait("SExp", "To")]
public class To
{
    [Fact]
    public void builds_correct_tree()
    {
        var s = clvm.SExp.To(new dynamic[] { "+", 1, 2});
        var t = s;
        var tree = Common.PrintTree(t);
        Assert.Equal("(43 (1 (2 () )))", tree);
    }
    
    [Fact]
    public void test_case_1()
    {
        var sexp = clvm.SExp.To(Encoding.UTF8.GetBytes("foo"));
        var t1 = clvm.SExp.To(new dynamic[] { 1, sexp });
        Common.ValidateSExp(t1);
    }
    
    [Fact]
    public void TestListConversions()
    {
        var a = clvm.SExp.To(new object[] { 1, 2, 3 });
        string expectedOutput = "(1 (2 (3 () )))";
        string result = Common.PrintTree(a);       
        Assert.Equal(expectedOutput, result);
    }
    
    [Fact]
    public void TestNullConversions()
    {
        var a = clvm.SExp.To(null);
        byte[] expected = Array.Empty<byte>();
        Assert.Equal(expected, a.AsAtom());
    }
    
    [Fact]
    public void empty_list_conversions()
    {
        var a = clvm.SExp.To(new object[] { });
        byte[] expected = new byte[0];
        Assert.Equal(expected, a.AsAtom());
    }
    
    [Fact]
    public void int_conversions()
    {
        var a = clvm.SExp.To(1337);
        byte[] expected = new byte[] { 0x5, 0x39 };
        Assert.Equal(expected, a.AsAtom());
    }
}