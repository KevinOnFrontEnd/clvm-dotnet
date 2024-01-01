using Xunit;

namespace clvm_dotnet.tests;

public class SerializeTests
{
    
    public void CheckSerde(dynamic s)
    {
        var v = SExp.To(s);
        var b = v.AsBin();
        var v1 = Serialize.SexpBufferFromStream(new MemoryStream(b));
        
        if (!v.Equals(v1))
        {
            Console.WriteLine($"{v}: {b.Length} {BitConverter.ToString(b)} {v1}");
            System.Diagnostics.Debugger.Break();
            b = v.AsBin();
            v1 = Serialize.SexpBufferFromStream(new MemoryStream(b));
        }
        
        Assert.True(v.Equals(v1));
    }

    [Fact]
    public void EmptyString()
    {
        CheckSerde("");
    }
    
    [Fact]
    public void TestSingleBytes()
    {
        for (int _ = 0; _ < 256; _++)
        {
            byte[] byteArray = new byte[] { (byte)_ };
            CheckSerde(byteArray);
        }
    }
    
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
        byte[] expectedBytes = new byte[] { 0x00 };
        Assert.Equal(expectedBytes, vAsBin);
    }
    
    // [Fact]
    // public void TestPlus()
    // {
    //     Assert.AreEqual(OPERATOR_LOOKUP(KEYWORD_TO_ATOM["+"], SExp.To(new int[] { 3, 4, 5 }))[1], SExp.To(12));
    // }
}