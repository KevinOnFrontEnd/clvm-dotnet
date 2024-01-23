using Xunit;
using x = CLVMDotNet.CLVM;

namespace CLVMDotNet.Tests.CLVM.CLVMObject;

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
        // Arrange
        Tuple<dynamic?, dynamic?> tuple = new Tuple<dynamic?, dynamic?>(left, right);
        
        // Act
        var clvmobject = new x.CLVMObject(tuple);
        
        // Assert
        Assert.NotNull(clvmobject.Pair);
        Assert.Null(clvmobject.Atom);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(1)]
    [InlineData(0x03)]
    public void Constructor_with_atom_sets_atom(dynamic atom)
    {
        // Arrange
        
        // Act
        var clvmobject = new x.CLVMObject(atom);
        
        // Assert
        Assert.NotNull(clvmobject.Atom);
        Assert.Null(clvmobject.Pair);
    }

    [Fact]
    public void Constructor_with_sexp_sets_atom()
    {
        // Arrange
        var sexp = x.SExp.To(0x03);
        
        // Act
        var clvmobject = new x.CLVMObject(sexp);
        
        // Assert
        Assert.NotNull(clvmobject.Atom);
        Assert.Null(clvmobject.Pair);
    }
    
    [Fact]
    public void Constructor_with_invalid_tuple_shape()
    {
        // Arrange
        
        // Act
        var errorLength3 =
            Assert.Throws<ArgumentException>(() =>
                new x.CLVMObject((1, 2, 3))
            );
        var errorLength4 =
            Assert.Throws<ArgumentException>(() =>
                new x.CLVMObject((1, 2, 3, 4))
            );
        var errorLength5 =
            Assert.Throws<ArgumentException>(() =>
                new x.CLVMObject((1, 2, 3, 4, 5))
            );
        
        // Assert
        Assert.Contains("tuples must be of size 2", errorLength3.Message);
        Assert.Contains("tuples must be of size 2", errorLength4.Message);
        Assert.Contains("tuples must be of size 2", errorLength5.Message);
    }
}