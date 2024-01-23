using Xunit;
using x = CLVMDotNet.CLVM;

namespace CLVMDotNet.Tests.CLVM.SExp
{
    [Trait("SExp", "Equal")]
    public class Equals
    {
        [Fact]
        public void Two_identical_sexp_are_equal()
        {
            // Arrange
            var s = x.SExp.To(new List<dynamic> { "+", 1, 2 });
            var t = x.SExp.To(new List<dynamic> { "+", 1, 2 });
            
            // Act
            var isEqual = t.Equals(s);
            
            // Assert
            Assert.True(isEqual);
        }
    }
}