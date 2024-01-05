using Xunit;
using clvm = clvm_dotnet;

namespace clvm_dotnet.tests;

[Trait("Serialize", "SexpFromStream")]
public class SExpFromStream
{
    [Fact]
    public void DeserializeTruncatedBlobTest()
    {
        // This is a complete length prefix. The blob is supposed to be 63 bytes,
        // but the blob itself is truncated, it's less than 63 bytes

        //dotnet will throw an error when trying use BitConverter here anyway
        byte[] bytesIn = new byte[] { 0xBF, 0x20, 0x20, 0x20 };

        var error1 = Assert.Throws<ArgumentException>(() => { clvm.Serialize.SexpFromStream(new MemoryStream(bytesIn)); });
        Assert.Contains("Destination array is not long enough to copy all the items in the collection", error1.Message);
    }
    
    [Fact]
    public void DeserializeTruncatedSizeTest()
    {
        // fe means the total number of bytes in the length-prefix is 7
        // one for each bit set. 5 bytes is too few
        byte[] bytesIn = new byte[] { 0xFE, 0x20, 0x20, 0x20, 0x20 };

        var error1 = Assert.Throws<InvalidOperationException>(() =>
        {
            clvm.Serialize.SexpFromStream(new MemoryStream(bytesIn));
        });
        Assert.Equal("Bad encoding - AtomFromStream", error1.Message);
    }
    
    private const string TEXT = "the quick brown fox jumps over the lazy dogs";
    
    [Fact]
    public void TestDeserializeEmpty()
    {
        byte[] bytesIn = Array.Empty<byte>();
        Assert.Throws<Exception>(() => { clvm.Serialize.SexpFromStream(new MemoryStream(bytesIn)); });
    }
}