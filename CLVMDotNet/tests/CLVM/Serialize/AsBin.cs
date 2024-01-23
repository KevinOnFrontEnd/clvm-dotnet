using Xunit;
using x = CLVMDotNet.CLVM;

namespace CLVMDotNet.Tests.Serialize
{
    [Trait("SExp", "AsBin")]
    public class AsBin
    {
        [Fact]
        public void TestZero()
        {
            // Arrange
            byte b = 0x00;
            var v = x.SExp.To(b);
            
            // Act
            byte[] vAsBin = v.AsBin();
            byte[] expectedBytes = new byte[] { 0x00 };
            
            // Assert
            Assert.Equal(expectedBytes, vAsBin);
        }

        [Fact]
        public void TestEmpty()
        {
            // Arrange
            byte[] inputBytes = new byte[0];
            var v = x.SExp.To(inputBytes);
            
            // Act
            byte[] vAsBin = v.AsBin();
            byte[] expectedBytes = new byte[] { 0x80 };
            
            // Assert
            Assert.Equal(expectedBytes, vAsBin);
        }
    }
}