namespace clvm_dotnet.tests;
using Xunit;

public class SExpTests
{
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
}