using Xunit;
using x = CLVMDotNet;

namespace CLVMDotNet.Tests.SExp;

[Trait("SExp", "Equal")]
public class Equals
{
    [Fact]
    public void Two_identical_sexp_are_equal()
    {
        var s = x.SExp.To(new dynamic[] { "+", 1, 2 });
        var t = x.SExp.To(new dynamic[] { "+", 1, 2 });
        var isEqual = t.Equals(s);
        Assert.True(isEqual);
    }
}