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
        var s = clvm.SExp.To(new dynamic[] { "+", 1, 2 });
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


    [Fact]
    // SExp provides a view on top of a tree of arbitrary types, as long as
    // those types implement the CLVMObject protocol. This is an example of
    // a tree that's generate
    //
    // There is a subtle differences between the python version and the 
    // c# version of representing trees. 
    public void arbitrary_underlying_tree()
    {
        var gentree1 = new GeneratedTree(5, 0);
        var tree1 = clvm.SExp.To(gentree1);
        Assert.Equal(Common.PrintTree(tree1), "(((((0 1 )(2 3 ))((4 5 )(6 7 )))(((8 9 )(10 11 ))((12 13 )(14 15 ))))((((16 17 )(18 19 ))((20 21 )(22 23 )))(((24 25 )(26 27 ))((28 29 )(30 31 )))))");
        
        
        var gentree2 = new GeneratedTree(3, 0);
        var tree2 = clvm.SExp.To(gentree2);
        Assert.Equal(Common.PrintTree(tree2), "(((0 1 )(2 3 ))((4 5 )(6 7 )))");

        var gentree3 = new GeneratedTree(3, 10);
        var tree3 = clvm.SExp.To(gentree3);
        Assert.Equal(Common.PrintTree(tree3), "(((10 11 )(12 13 ))((14 15 )(16 17 )))");
    }
}