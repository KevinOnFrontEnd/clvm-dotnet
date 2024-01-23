using System.Text;
using x = CLVMDotNet.CLVM;
using Xunit;

namespace CLVMDotNet.Tests.CLVM.SExp
{
    public class SExpTests
    {
        [Fact]
        public void TestWrapSExp()
        {
            // Arrange
            
            // Act
            var sexp = x.SExp.To(1);
            x.CLVMObject o = new x.CLVMObject(sexp);
            
            // Assert
            Assert.True(o.Atom.Equals(1));
        }

        [Fact]
        public void TestStringConversions()
        {
            // Arrange
            
            // Act
            var a = x.SExp.To("foobar");
            byte[] expectedOutput = Encoding.UTF8.GetBytes("foobar");
            byte[] result = a.AsAtom();
            
            // Assert
            Assert.Equal(expectedOutput, result);
        }
    }
}