using Xunit;

namespace clvm_dotnet.tests;

public class SerializeTests
{
    [Fact]
    public void TestZero()
    {
        byte[] inputBytes = new byte[] { 0x00 };
        SExp v = SExp.To(inputBytes);
        byte[] vAsBin = v.AsBin();
        byte[] expectedBytes = new byte[] { 0x00 };
        Assert.Equal(expectedBytes, vAsBin);
    }
    
    [Fact]
    public void TestEmpty()
    {
        byte[] inputBytes = new byte[0];
        SExp v = SExp.To(inputBytes);
        byte[] vAsBin = v.AsBin();
        byte[] expectedBytes = new byte[] { 0x80 };
        Assert.Equal(expectedBytes, vAsBin);
    }
}