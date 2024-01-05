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