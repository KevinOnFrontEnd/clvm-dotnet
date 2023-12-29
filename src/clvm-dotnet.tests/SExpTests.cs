namespace clvm_dotnet.tests;
using Xunit;

public class SExpTests
{
    // [Fact]
    // public void test_case_1()
    // {
    //     var sexp = SExp.To("foo");
    //     var t1 = SExp.To([1, sexp]);
    //     ValidateSExp(t1);
    // } 

    [Fact]
    public void wrap_sexp()
    {
        // it's a bit of a layer violation that CLVMObject unwraps SExp, but we
        // rely on that in a fair number of places for now. We should probably
        // work towards phasing that out
        // var o = new CLVMObject(SExp.To(1));
        // Assert.Equal(o.Atom, bytes([1]);
    }

    [Fact]
    public void list_conversions()
    {
        // def test_list_conversions(self):
        // a = SExp.to([1, 2, 3])
        // assert print_tree(a) == "(1 (2 (3 () )))"
    }
    
    [Fact]
    public void string_conversions()
    {
        // SExp a = SExp.To("foobar");
        // Assert.Equal("foobar".EncodeToUtf8(), a.AsAtom());
    }

    [Fact]
    public void int_conversions()
    {
        // SExp a = SExp.To(1337);
        // byte[] expected = new byte[] { 0x5, 0x39 };
        // Assert.Equal(expected, a.AsAtom());
    }
    
    [Fact]
    public void TestNoneConversions()
    {
        // SExp a = SExp.To(null);
        // byte[] expected = new byte[0];
        // Assert.Equal(expected, a.AsAtom());
    }
    
    [Fact]
    public void empty_list_conversions()
    {
        // SExp a = SExp.To(new object[] { });
        // byte[] expected = new byte[0];
        // Assert.Equal(expected, a.AsAtom());
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
    
    public  string PrintTree(SExp tree)
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

    public static void ValidateSExp(SExp sexp)
    {
        Stack<SExp> validateStack = new Stack<SExp>();
        validateStack.Push(sexp);

        while (validateStack.Count > 0)
        {
            SExp v = validateStack.Pop();
            if (!(v is SExp))
            {
                throw new Exception("Validation failed: v is not an instance of SExp.");
            }

            if (v.Pair != null)
            {
                if (!(v.Pair is Tuple<object, object>))
                {
                    throw new Exception("Validation failed: v.Pair is not a Tuple.");
                }

                var (v1, v2) = v.Pair;

                if (!HelperFunctions.LooksLikeCLVMObject((v1)) || !HelperFunctions.LooksLikeCLVMObject(v2))
                {
                    throw new Exception("Validation failed: v1 and v2 do not look like CLVM objects.");
                }

                SExp s1 = v1 as SExp;
                SExp s2 = v2 as SExp;

                if (s1 != null && s2 != null)
                {
                    validateStack.Push(s1);
                    validateStack.Push(s2);
                }
                else
                {
                    throw new Exception("Validation failed: s1 and s2 are not instances of SExp.");
                }
            }
            else
            {
                if (!(v.Atom is byte[]))
                {
                    throw new Exception("Validation failed: v.Atom is not a byte array.");
                }
            }
        }
    }
    #endregion
}