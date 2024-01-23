using Xunit;
using x = CLVMDotNet.CLVM;

namespace CLVMDotNet.Tests.Serialize
{
    [Trait("Serialize", "SexpBufferFromStream")]
    public class SexpBufferFromStream
    {
        private const string TEXT = "the quick brown fox jumps over the lazy dogs";

        [Fact]
        public void DeserializeTruncatedBlobTest()
        {
            
            // Arrange
            // This is a complete length prefix. The blob is supposed to be 63 bytes,
            // but the blob itself is truncated, it's less than 63 bytes
            //dotnet will throw an error when trying use BitConverter here anyway
            byte[] bytesIn = new byte[] { 0xBF, 0x20, 0x20, 0x20 };

            // Act
            var error = Assert.Throws<ArgumentException>(() =>
            {
                x.Serialize.SexpBufferFromStream(new MemoryStream(bytesIn));
            });
            
            // Assert
            Assert.Contains("The array starting from the specified index is not long enough to read a value of the specified type",
                error.Message);
        }

        [Fact]
        public void TestDeserializeLargeBlob()
        {
            // Arrange
            // This length prefix is 7 bytes long, the last 6 bytes specifies the
            // length of the blob, which is 0xffffffffffff, or (2^48 - 1)
            // We don't support blobs this large, and we should fail immediately when
            // exceeding the max blob size, rather than trying to read this many
            // bytes from the stream
            //
            //DOT NET WILL ERROR ON BitConverter.ToUInt64(sizeBlob anyway
            byte[] bytesIn = new byte[] { 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

            // Act
            var error2 =
                Assert.Throws<ArgumentException>(() =>
                    x.Serialize.SexpBufferFromStream(new InfiniteStream(bytesIn)));
            
            // Assert
            Assert.Contains("The array starting from the specified index is not long enough to read a value of the specified type.", error2.Message);
        }

        [Fact]
        public void DeserializeTruncatedSizeTest()
        {
            // Arrange
            // fe means the total number of bytes in the length-prefix is 7
            // one for each bit set. 5 bytes is too few
            byte[] bytesIn = new byte[] { 0xFE, 0x20, 0x20, 0x20, 0x20 };

            // Act
            var error2 = Assert.Throws<InvalidOperationException>(() =>
            {
                x.Serialize.SexpBufferFromStream(new MemoryStream(bytesIn));
            });
            
            // Assert
            Assert.Equal("Bad encoding - ConsumeAtom", error2.Message);
        }

        [Fact]
        public void TestDeserializeEmpty()
        {
            // Arrange
            byte[] bytesIn = Array.Empty<byte>();
            
            // Act
            var error = Assert.Throws<Exception>(() => { x.Serialize.SexpBufferFromStream(new MemoryStream(bytesIn)); });
            
            // Assert
            Assert.Equal("Bad encoding", error.Message);
        }
    }
}