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
            var sexp = x.SExp.To(1);
            x.CLVMObject o = new x.CLVMObject(sexp);
            Assert.True(o.Atom.Equals(1));
        }

        [Fact]
        public void TestStringConversions()
        {
            var a = x.SExp.To("foobar");
            byte[] expectedOutput = Encoding.UTF8.GetBytes("foobar");
            byte[] result = a.AsAtom();
            Assert.Equal(expectedOutput, result);
        }
    }
}