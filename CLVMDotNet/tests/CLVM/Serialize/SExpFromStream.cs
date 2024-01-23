using Xunit;
using x = CLVMDotNet.CLVM;

namespace CLVMDotNet.Tests.Serialize
{
    [Trait("Serialize", "SexpFromStream")]
    public class SExpFromStream
    {
        [Fact]
        public void DeserializeTruncatedBlobTest()
        {
            // Arrange
            // This is a complete length prefix. The blob is supposed to be 63 bytes,
            // but the blob itself is truncated, it's less than 63 bytes

            //dotnet will throw an error when trying use BitConverter here anyway
            byte[] bytesIn = new byte[] { 0xBF, 0x20, 0x20, 0x20 };

            // Act
            var error1 = Assert.Throws<ArgumentException>(() =>
            {
                x.Serialize.SexpFromStream(new MemoryStream(bytesIn));
            });
            
            // Assert
            Assert.Contains("The array starting from the specified index is not long enough to read a value of the specified type",
                error1.Message);
        }

        [Fact]
        public void DeserializeTruncatedSizeTest()
        {
            // Arrange
            // fe means the total number of bytes in the length-prefix is 7
            // one for each bit set. 5 bytes is too few
            byte[] bytesIn = new byte[] { 0xFE, 0x20, 0x20, 0x20, 0x20 };

            // Act
            var error1 = Assert.Throws<InvalidOperationException>(() =>
            {
                x.Serialize.SexpFromStream(new MemoryStream(bytesIn));
            });
            
            // Assert
            Assert.Equal("Bad encoding - AtomFromStream", error1.Message);
        }

        private const string TEXT = "the quick brown fox jumps over the lazy dogs";

        [Fact]
        public void TestDeserializeEmpty()
        {
            // Arrange
            byte[] bytesIn = Array.Empty<byte>();
            
            // Act
            var error1 = Assert.Throws<Exception>(() => { x.Serialize.SexpFromStream(new MemoryStream(bytesIn)); });
            
            // Assert
            Assert.Equal("Bad encoding", error1.Message);
        }
    }
}