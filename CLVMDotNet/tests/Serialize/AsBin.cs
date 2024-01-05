using Xunit;
using CLVM = CLVMDotNet;

namespace CLVMDotNet.Tests.Serialize
{
    [Trait("SExp", "AsBin")]
    public class AsBin
    {
        [Fact]
        public void TestZero()
        {
            byte b = 0x00;
            var v = CLVM.SExp.To(b);
            byte[] vAsBin = v.AsBin();
            byte[] expectedBytes = new byte[] { 0x00 };
            Assert.Equal(expectedBytes, vAsBin);
        }

        [Fact]
        public void TestEmpty()
        {
            byte[] inputBytes = new byte[0];
            var v = CLVM.SExp.To(inputBytes);
            byte[] vAsBin = v.AsBin();
            byte[] expectedBytes = new byte[] { 0x80 };
            Assert.Equal(expectedBytes, vAsBin);
        }
    }
}