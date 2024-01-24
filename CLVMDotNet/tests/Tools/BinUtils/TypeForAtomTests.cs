using System.Numerics;
using System.Text;
using CLVMDotNet.Tools.IR;
using Xunit;
using x = CLVMDotNet.Tools.IR;

namespace CLVMDotNet.Tests.Tools.BinUtils
{

    [Trait("BinUtils", "TypeForAtom")]
    public class TypeForAtomTests
    {
        [Theory]
        [InlineData("this is a test")]
        [InlineData("178884")]
        [InlineData("aaaa")]
        [InlineData("100")]
        [InlineData("126")]
        public void TypeForAtomReturnsQuote(string value)
        {
            // Arrange
            byte[] bytes  = Encoding.UTF8.GetBytes(value);
            
            // Act
            var t = x.BinUtils.TypeForAtom(bytes);
            
            // Assert
            Assert.Equal(IRType.QUOTES, t);
        }
        
        [Theory]
        [InlineData("1")]
        [InlineData("4")]
        [InlineData("99")]
        public void TypeForAtomReturnsInt(string value)
        {
            // Arrange
            byte[] bytes  = Encoding.UTF8.GetBytes(value);
            
            // Act
            var t = x.BinUtils.TypeForAtom(bytes);
            
            // Assert
            Assert.Equal(IRType.INT, t);
        }
        
        //TODO: ADD Test to check that IRType.HEX is returned
    }
}