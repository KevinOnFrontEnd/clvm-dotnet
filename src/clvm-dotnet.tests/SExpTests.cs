using System.Text;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace clvm_dotnet.tests;

using Xunit;

public class SExpTests
{
    [Fact]
    public void test_case_1()
    {
        var sexp = SExp.To("foo");
        var t1 = SExp.To(new object[] { 1, sexp });
        ValidateSExp(t1);
    }
    
    /// <summary>
    /// The python clvm has an _eq_ that attempts to cast a sexp type to the type its being compared to.
    /// This remains incomplete until we implement and equals method that tries to do the same.
    ///
    /// https://github.com/Chia-Network/clvm/blob/main/clvm/SExp.py#L209
    /// </summary>
    [Fact(Skip = "Skipping until we can mimick the equality check that the python repo does")]
    public void TestWrapSExp()
    {
        SExp sexp = SExp.To(1);
        CLVMObject o = new CLVMObject(sexp);
        byte[] expected = new byte[] { 1 };
        Assert.Equal(expected, o.Atom);
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
    
    // [Fact]
    // public void TestNoneBytesConversions()
    // {
    //     SExp a = SExp.To(null);
    //     byte[] expected = new byte[0];
    //     Assert.Equal(expected, a.AsAtom());
    // }

    [Fact]
    public void empty_list_conversions()
    {
        SExp a = SExp.To(new object[] { });
        byte[] expected = new byte[0];
        Assert.Equal(expected, a.AsAtom());
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

    public string PrintTree(SExp tree)
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