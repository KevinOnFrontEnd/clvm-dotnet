using System.Runtime.InteropServices;
using Xunit;
using BinaryReader = System.IO.BinaryReader;

namespace clvm_dotnet.tests;

public class InfiniteStream : Stream
{
    private byte[] buf;
    private int position;

    public InfiniteStream(byte[] b)
    {
        buf = b;
        position = 0;
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int bytesRead = 0;

        while (count > 0 && position < buf.Length)
        {
            buffer[offset] = buf[position];
            offset++;
            position++;
            count--;
            bytesRead++;
        }

        return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => buf.Length;

    public override long Position
    {
        get => position;
        set => throw new NotSupportedException();
    }
}

public class SerializeTests
{
    private const string TEXT = "the quick brown fox jumps over the lazy dogs";


    [Fact]
    public void TestDeserializeEmpty()
    {
        byte[] bytesIn = Array.Empty<byte>();
        Assert.Throws<Exception>(() => { Serialize.SexpFromStream(new MemoryStream(bytesIn)); });

        Assert.Throws<Exception>(() => { Serialize.SexpBufferFromStream(new MemoryStream(bytesIn)); });
    }

    [Fact]
    public void DeserializeTruncatedSizeTest()
    {
        // fe means the total number of bytes in the length-prefix is 7
        // one for each bit set. 5 bytes is too few
        byte[] bytesIn = new byte[] { 0xFE, 0x20, 0x20, 0x20, 0x20 };

        var error1 = Assert.Throws<InvalidOperationException>(() =>
        {
            Serialize.SexpFromStream(new MemoryStream(bytesIn));
        });
        Assert.Equal("Bad encoding - AtomFromStream", error1.Message);

        var error2 = Assert.Throws<InvalidOperationException>(() =>
        {
            Serialize.SexpBufferFromStream(new MemoryStream(bytesIn));
        });
        Assert.Equal("Bad encoding - ConsumeAtom", error2.Message);
    }

    [Fact]
    public void DeserializeTruncatedBlobTest()
    {
        // This is a complete length prefix. The blob is supposed to be 63 bytes,
        // but the blob itself is truncated, it's less than 63 bytes

        //dotnet will throw an error when trying use BitConverter here anyway
        byte[] bytesIn = new byte[] { 0xBF, 0x20, 0x20, 0x20 };

        var error1 = Assert.Throws<ArgumentException>(() => { Serialize.SexpFromStream(new MemoryStream(bytesIn)); });
        Assert.Contains("Destination array is not long enough to copy all the items in the collection", error1.Message);


        var error2 = Assert.Throws<ArgumentException>(() =>
        {
            Serialize.SexpBufferFromStream(new MemoryStream(bytesIn));
        });
        Assert.Contains("Destination array is not long enough to copy all the items in the collection", error1.Message);
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

        var error1 = Assert.Throws<ArgumentException>(() => Serialize.SexpFromStream(new InfiniteStream(bytesIn)));
        Assert.Contains("Destination array is not long enough", error1.Message);

        var error2 =
            Assert.Throws<ArgumentException>(() => Serialize.SexpBufferFromStream(new InfiniteStream(bytesIn)));
        Assert.Contains("Destination array is not long enough", error2.Message);
    }

    public void CheckSerde(dynamic s)
    {
        SExp v = SExp.To(s);
        var b = v.AsBin();
        var v1 = Serialize.SexpFromStream(new MemoryStream(b));
        var isEqual = v.Equals(v1);
        if (!isEqual)
        {
            Console.WriteLine($"{v}: {b.Length} {BitConverter.ToString(b)} {v1}");
            System.Diagnostics.Debugger.Break();
            b = v.AsBin();
            v1 = Serialize.SexpBufferFromStream(new MemoryStream(b));
        }
        Assert.True(isEqual);
    }
    
    // [Fact]
    // public void EmptyString()
    // {
    //     CheckSerde(Array.Empty<byte>());
    // }
    
    // [Fact]
    // public void TestSingleBytes()
    // {
    //     for (int _ = 0; _ < 256; _++)
    //     {
    //         byte[] byteArray = new byte[] { (byte)_ };
    //         CheckSerde(byteArray);
    //     }
    // }
    

    #region AtomFromStream

    [Fact]
    public void AtomFromStreamEmptyString()
    {
        byte b = 0x80;
        var sexp = Serialize.AtomFromStream(null, b,typeof(CLVMObject));
        Assert.Equal(Array.Empty<byte>(), sexp.Atom);
    }

    [Fact]
    public void AtomFromStreamMaxSingleByte()
    {
        byte b = 0x7F;
        byte[] byteArray = new byte[] { 0x7F };
        var sexp = Serialize.AtomFromStream(null, b, typeof(CLVMObject));
        Assert.Equal(byteArray, sexp.Atom);
    }

    #endregion

    
    [Fact]
    public void TestZero()
    {
        byte b = 0x00;
        SExp v = SExp.To(b);
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

    [Fact]
     public void TestShortLists()
     {
         for (int _ = 8; _ < 36; _ += 8)
         {
             for (int size = 1; size < 5; size++)
             {
                 CheckSerde(Enumerable.Repeat(_, size).ToArray());
             }
         }
     }   

    // [Fact(Skip = "Broken for the moment")]
    // public void TestConsBox()
    // {
    //     CheckSerde(Tuple.Create<object, object>(null, null));
    //     CheckSerde(Tuple.Create<object, object>(null, new object[] { 1, 2, 30, 40, 600, Tuple.Create<object, object>(null, 18) }));
    //     CheckSerde(Tuple.Create<object, object>(100, Tuple.Create<object, object>(TEXT, Tuple.Create<object, object>(30, Tuple.Create<object, object>(50, Tuple.Create<object, object>(90, Tuple.Create<object, object>(TEXT, TEXT + TEXT)))))));
    // }

    // [Fact]
    // public void TestPlus()
    // {
    //     Assert.AreEqual(OPERATOR_LOOKUP(KEYWORD_TO_ATOM["+"], SExp.To(new int[] { 3, 4, 5 }))[1], SExp.To(12));
    // }
}