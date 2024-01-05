using Xunit;

namespace clvm_dotnet.tests;
using clvm = clvm_dotnet;

[Trait("Serialize", "SexpBufferFromStream")]
public class SexpBufferFromStream
{
    private const string TEXT = "the quick brown fox jumps over the lazy dogs";
    
    [Fact]
    public void DeserializeTruncatedBlobTest()
    {
        // This is a complete length prefix. The blob is supposed to be 63 bytes,
        // but the blob itself is truncated, it's less than 63 bytes

        //dotnet will throw an error when trying use BitConverter here anyway
        byte[] bytesIn = new byte[] { 0xBF, 0x20, 0x20, 0x20 };

        var error = Assert.Throws<ArgumentException>(() =>
        {
            clvm.Serialize.SexpBufferFromStream(new MemoryStream(bytesIn));
        });
        Assert.Contains("Destination array is not long enough to copy all the items in the collection", error.Message);
    }
    
    [Fact]
    public void TestDeserializeLargeBlob()
    {
        // This length prefix is 7 bytes long, the last 6 bytes specifies the
        // length of the blob, which is 0xffffffffffff, or (2^48 - 1)
        // We don't support blobs this large, and we should fail immediately when
        // exceeding the max blob size, rather than trying to read this many
        // bytes from the stream
        //
        //DOT NET WILL ERROR ON BitConverter.ToUInt64(sizeBlob anyway
        byte[] bytesIn = new byte[] { 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        
        var error2 =
            Assert.Throws<ArgumentException>(() => clvm.Serialize.SexpBufferFromStream(new InfiniteStream(bytesIn)));
        Assert.Contains("Destination array is not long enough", error2.Message);
    }
    
    [Fact]
    public void DeserializeTruncatedSizeTest()
    {
        // fe means the total number of bytes in the length-prefix is 7
        // one for each bit set. 5 bytes is too few
        byte[] bytesIn = new byte[] { 0xFE, 0x20, 0x20, 0x20, 0x20 };
        
        var error2 = Assert.Throws<InvalidOperationException>(() =>
        {
            clvm.Serialize.SexpBufferFromStream(new MemoryStream(bytesIn));
        });
        Assert.Equal("Bad encoding - ConsumeAtom", error2.Message);
    }
    
    [Fact]
    public void TestDeserializeEmpty()
    {
        byte[] bytesIn = Array.Empty<byte>();
        Assert.Throws<Exception>(() => { clvm.Serialize.SexpBufferFromStream(new MemoryStream(bytesIn)); });
    }
}