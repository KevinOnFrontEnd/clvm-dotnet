using Xunit;
using x = CLVMDotNet.CLVM;

namespace CLVMDotNet.Tests.Serialize
{
    [Trait("SExp", "AtomFromStream")]
    public class AtomFromStream
    {
        [Fact]
        public void AtomFromStreamEmptyString()
        {
            // Arrange
            byte b = 0x80;
            
            // Act
            var sexp = x.Serialize.AtomFromStream(null, b, typeof(x.CLVMObject));
            
            // Assert
            Assert.Equal(Array.Empty<byte>(), sexp.Atom);
        }

        [Fact]
        public void AtomFromStreamMaxSingleByte()
        {
            // Arrange
            byte b = 0x7F;
            byte[] byteArray = new byte[] { 0x7F };
            
            // Act
            var sexp = x.Serialize.AtomFromStream(null, b, typeof(x.CLVMObject));
            
            // Assert
            Assert.Equal(byteArray, sexp.Atom);
        }
    }
}