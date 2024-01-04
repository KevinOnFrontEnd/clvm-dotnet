using System.Text;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace clvm_dotnet.tests;

using Xunit;

public class SExpTests
{
    [Fact]
    public void test_case_1()
    {
        var sexp = SExp.To(Encoding.UTF8.GetBytes("foo"));
        var t1 = SExp.To(new dynamic[] { 1, sexp });
        ValidateSExp(t1);
    }

    [Fact]
    public void TestWrapSExp()
    {
        var sexp = SExp.To(1);
        CLVMObject o = new CLVMObject(sexp);
        Assert.True(o.Atom.Equals(1));
    }
    
    [Fact]
    public void TestListConversions()
    {
        SExp a = SExp.To(new object[] { 1, 2, 3 });
        string expectedOutput = "(1 (2 (3 () )))";
        string result = PrintTree(a);       
        Assert.Equal(expectedOutput, result);
    }

    [Fact]
    public void TestStringConversions()
    {
        SExp a = SExp.To("foobar");
        byte[] expectedOutput = Encoding.UTF8.GetBytes("foobar");
        byte[] result = a.AsAtom();
        Assert.Equal(expectedOutput, result);
    }

    [Fact]
    public void int_conversions()
    {
        SExp a = SExp.To(1337);
        byte[] expected = new byte[] { 0x5, 0x39 };
        Assert.Equal(expected, a.AsAtom());
    }
    
    [Fact]
    public void TestNullConversions()
    {
        SExp a = SExp.To(null);
        byte[] expected = Array.Empty<byte>();
        Assert.Equal(expected, a.AsAtom());
    }

    [Fact]
    public void empty_list_conversions()
    {
        SExp a = SExp.To(new object[] { });
        byte[] expected = new byte[0];
        Assert.Equal(expected, a.AsAtom());
    }

    [Theory]
    [InlineData(new byte[]{ 0xFF, 0x80, 0x80  }, new int[] {0})] 
    [InlineData(new byte[]{ 0xFF, 0x01, 0x80  }, new int[] {1})] 
    [InlineData(new byte[] { 0xFF, 0x08, 0xFF, 0x08, 0x80}, new int[] {8,8})]
    //[InlineData(new byte[] { 0xFF, 0x82, 0x01, 0x00, 0xFF, 0x82, 0x01, 0x00, 0x80 }, new int[]  {256,256})]  
    // [InlineData(new byte[] { 8, 9 }, new byte[]   {512,512,512})]  
    // [InlineData(new byte[] { 8, 9 }, new byte[]  {1024,1024,1024,1024})]  
    // [InlineData(new byte[] { 8, 9 }, new List<int>  {2048,248,2048,2048,2048})]  
    public void sexp_AsBinIsCorrectOrder(byte[] expected, dynamic  sexp_list)
    {
        SExp v = SExp.To(sexp_list);
        var bytes = v.AsBin();
        Assert.Equal(expected, bytes );
    }

    
    #region test helpers that should probably go into SExp object
    private string PrintLeaves(SExp tree)
    {
        var a = tree.AsAtom();
        if (a != null)
        {
            if (a.Length == 0)
                return "() ";

            return $"{a[0]} ";
        }

        var ret = "";
        var pairs = tree.AsPair();
        var list = new List<SExp>() { pairs.Item1, pairs.Item2 };
        if (pairs != null)
        {
            foreach (SExp i in list)
            {
                ret += PrintLeaves(i);
            }
        }

        return ret;
    }

    private string PrintTree(SExp tree)
    {
        var a = tree.AsAtom();
        if (a != null)
        {
            if (a.Length == 0)
            {
                return "() ";
            }

            return $"{a[0]} ";
        }

        var ret = "(";
        var pairs = tree.AsPair();
        var list = new List<SExp>() { pairs.Item1, pairs.Item2 };
        if (pairs != null)
        {
            foreach (var i in list)
            {
                ret += PrintTree(i);
            }
        }

        ret += ")";
        return ret;
    }

    private static void ValidateSExp(SExp sexp)
    {
        Stack<SExp> validateStack = new Stack<SExp>();
        validateStack.Push(sexp);

        while (validateStack.Count > 0)
        {
            dynamic v = validateStack.Pop();

            if (!(v is SExp))
            {
                throw new InvalidOperationException("v is not an instance of SExp");
            }

            if (v.Pair != null)
            {
                if (v.Pair.GetType() != typeof(Tuple<object,object>))
                {
                    throw new InvalidOperationException("v.pair is not a Tuple");
                }

                Tuple<dynamic, dynamic> pair = v.Pair;

                if (!HelperFunctions.LooksLikeCLVMObject(pair.Item1) || !HelperFunctions.LooksLikeCLVMObject(pair.Item2))
                {
                    throw new InvalidOperationException("One or both elements do not look like CLVM objects");
                }

                var sPair = v.AsPair();
                validateStack.Push(sPair.Item1);
                validateStack.Push(sPair.Item2);
            }
            else
            {
                if (!(v.Atom is byte[]))
                {
                    throw new InvalidOperationException("v.atom is not a byte array");
                }
            }
        }
    }
    #endregion
}