using Xunit;
using CLVM = CLVMDotNet;
namespace CLVMDotNet.Tests.CLVMObject;

[Trait("CLVMObject", "InstantiationTests")]
public class InstantiationTests
{
    [Theory]
    [InlineData("","")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData(0x03, 0x03)]
    public void Constructor_with_tuple_sets_pair(dynamic? left, dynamic? right)
    {
        Tuple<dynamic?, dynamic?> tuple = new Tuple<dynamic?, dynamic?>(left, right);
        var clvmobject = new CLVM.CLVMObject(tuple);
        Assert.NotNull(clvmobject.Pair);
        Assert.Null(clvmobject.Atom);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(1)]
    [InlineData(0x03)]
    public void Constructor_with_atom_sets_atom(dynamic atom)
    {
        var clvmobject = new CLVM.CLVMObject(atom);
        Assert.NotNull(clvmobject.Atom);
        Assert.Null(clvmobject.Pair);
    }

    [Fact]
    public void Constructor_with_sexp_sets_atom()
    {
        var sexp = CLVM.SExp.To(0x03);
        var clvmobject = new CLVM.CLVMObject(sexp);
        Assert.NotNull(clvmobject.Atom);
        Assert.Null(clvmobject.Pair);
    }
    
    [Fact]
    public void Constructor_with_invalid_tuple_shape()
    {
        var errorLength3 =
            Assert.Throws<ArgumentException>(() =>
                new CLVM.CLVMObject((1, 2, 3))
            );
        Assert.Contains("tuples must be of size 2", errorLength3.Message);
        
        var errorLength4 =
            Assert.Throws<ArgumentException>(() =>
                new CLVM.CLVMObject((1, 2, 3, 4))
            );
        Assert.Contains("tuples must be of size 2", errorLength4.Message);
        
        var errorLength5 =
            Assert.Throws<ArgumentException>(() =>
                new CLVM.CLVMObject((1, 2, 3, 4, 5))
            );
        Assert.Contains("tuples must be of size 2", errorLength5.Message);
    }
}