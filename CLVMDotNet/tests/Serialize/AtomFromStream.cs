using Xunit;
using CLVM = CLVMDotNet;

namespace CLVMDotNet.Tests.Serialize
{
    [Trait("SExp", "AtomFromStream")]
    public class AtomFromStream
    {
        [Fact]
        public void AtomFromStreamEmptyString()
        {
            byte b = 0x80;
            var sexp = CLVM.Serialize.AtomFromStream(null, b, typeof(CLVMObject));
            Assert.Equal(Array.Empty<byte>(), sexp.Atom);
        }

        [Fact]
        public void AtomFromStreamMaxSingleByte()
        {
            byte b = 0x7F;
            byte[] byteArray = new byte[] { 0x7F };
            var sexp = CLVM.Serialize.AtomFromStream(null, b, typeof(CLVMObject));
            Assert.Equal(byteArray, sexp.Atom);
        }
    }
}