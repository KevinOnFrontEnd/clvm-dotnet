namespace clvm_dotnet;


public static class serialize
{
    public static readonly byte CONS_BOX_MARKER = 0xFF;
    public static readonly byte MAX_SINGLE_BYTE = 0x7F;
    
     public static byte[] SExpToByteArray(SExp sexp)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            foreach (byte b in SExpToByteIterator(sexp))
            {
                memoryStream.WriteByte(b);
            }
            return memoryStream.ToArray();
        }
    }

    public static IEnumerable<byte> SExpToByteIterator(SExp sexp)
    {
        var todoStack = new System.Collections.Generic.Stack<SExp>();
        todoStack.Push(sexp);

        while (todoStack.Count > 0)
        {
            sexp = todoStack.Pop();
            var pair = sexp.as_pair();
            if (pair != null)
            {
                yield return CONS_BOX_MARKER;
                todoStack.Push(pair.Item2);
                todoStack.Push(pair.Item1);
            }
            else
            {
                foreach (byte b in AtomToByteIterator(sexp.as_atom()))
                {
                    yield return b;
                }
            }
        }
    }

    public static IEnumerable<byte> AtomToByteIterator(byte[] asAtom)
    {
        int size = asAtom.Length;
        if (size == 0)
        {
            yield return 0x80;
            yield break;
        }

        if (size == 1 && asAtom[0] <= MAX_SINGLE_BYTE)
        {
            yield return asAtom[0];
            yield break;
        }

        if (size < 0x40)
        {
            yield return (byte)(0x80 | size);
        }
        else if (size < 0x2000)
        {
            yield return (byte)(0xC0 | (size >> 8));
            yield return (byte)(size & 0xFF);
        }
        else if (size < 0x100000)
        {
            yield return (byte)(0xE0 | (size >> 16));
            yield return (byte)((size >> 8) & 0xFF);
            yield return (byte)(size & 0xFF);
        }
        else if (size < 0x8000000)
        {
            yield return (byte)(0xF0 | (size >> 24));
            yield return (byte)((size >> 16) & 0xFF);
            yield return (byte)((size >> 8) & 0xFF);
            yield return (byte)(size & 0xFF);
        }
        else if (size < 0x400000000)
        {
            yield return (byte)(0xF8 | (size >> 32));
            yield return (byte)((size >> 24) & 0xFF);
            yield return (byte)((size >> 16) & 0xFF);
            yield return (byte)((size >> 8) & 0xFF);
            yield return (byte)(size & 0xFF);
        }
        else
        {
            throw new ArgumentException("sexp too long " + BitConverter.ToString(asAtom));
        }

        foreach (byte b in asAtom)
        {
            yield return b;
        }
    }

    public static void SExpToStream(SExp sexp, Stream stream)
    {
        foreach (byte b in SExpToByteIterator(sexp))
        {
            stream.WriteByte(b);
        }
    }

    private static void ReadSExpStack(
        Stream stream,
        Func<byte[], SExp> toSExp,
        out System.Collections.Generic.Stack<Action<System.Collections.Generic.Stack<Action<System.IO.Stream>>>> opStack,
        out System.Collections.Generic.Stack<SExp> valStack)
    {
        opStack = new System.Collections.Generic.Stack<Action<System.Collections.Generic.Stack<Action<System.IO.Stream>>>>();
        valStack = new System.Collections.Generic.Stack<SExp>();

        while (opStack.Count > 0)
        {
            var opFunc = opStack.Pop();
            opFunc(opStack, stream, toSExp, valStack);
        }
    }

    private static void ReadSExp(
        System.Collections.Generic.Stack<Action<System.Collections.Generic.Stack<Action<System.IO.Stream>>>> opStack,
        Stream stream,
        Func<byte[], SExp> toSExp,
        System.Collections.Generic.Stack<SExp> valStack)
    {
        byte[] blob = new byte[1];
        int bytesRead = stream.Read(blob, 0, 1);

        if (bytesRead == 0)
        {
            throw new ArgumentException("bad encoding");
        }

        byte b = blob[0];

        if (b == CONS_BOX_MARKER)
        {
            opStack.Push(ReadCons);
            opStack.Push(ReadSExp);
            opStack.Push(ReadSExp);
        }
        else
        {
            valStack.Push(AtomFromStream(stream, b, toSExp));
        }
    }

    private static void ReadCons(
        System.Collections.Generic.Stack<Action<System.Collections.Generic.Stack<Action<System.IO.Stream>>>> opStack,
        Stream stream,
        Func<byte[], SExp> toSExp,
        System.Collections.Generic.Stack<SExp> valStack)
    {
        SExp right = valStack.Pop();
        SExp left = valStack.Pop();
        valStack.Push(toSExp(new byte[] { CONS_BOX_MARKER }));
        valStack.Push(left);
        valStack.Push(right);
    }

    public static SExp SExpFromStream(Stream stream, Func<byte[], SExp> toSExp)
    {
        var opStack = new System.Collections.Generic.Stack<Action<System.Collections.Generic.Stack<Action<System.IO.Stream>>>>();
        var valStack = new System.Collections.Generic.Stack<SExp>();

        opStack.Push(ReadSExp);

        ReadSExpStack(stream, toSExp, out opStack, out valStack);

        return toSExp(valStack.Pop());
    }

    private static (byte[], int) ConsumeSExp(Stream stream)
    {
        byte[] blob = new byte[1];
        int bytesRead = stream.Read(blob, 0, 1);

        if (bytesRead == 0)
        {
            throw new ArgumentException("bad encoding");
        }

        byte b = blob[0];

        if (b == CONS_BOX_MARKER)
        {
            return (new byte[] { b }, 2);
        }

        return (ConsumeAtom(stream, b), 0);
    }

    private static byte[] ConsumeAtom(Stream stream, byte b)
    {
        if (b == 0x80)
        {
            return new byte[] { b };
        }

        if (b <= MAX_SINGLE_BYTE)
        {
            return new byte[] { b };
        }

        int bitCount = 0;
        byte bitMask = 0x80;

        while ((b & bitMask) != 0)
        {
            bitCount++;
            b &= (byte)(0xFF ^ bitMask);
            bitMask >>= 1;
        }

        byte[] sizeBlob = new byte[] { b };

        if (bitCount > 1)
        {
            byte[] ll = new byte[bitCount - 1];
            int bytesRead = stream.Read(ll, 0, bitCount - 1);

            if (bytesRead != bitCount - 1)
            {
                throw new ArgumentException("bad encoding");
            }

            sizeBlob = sizeBlob.Concat(ll).ToArray();
        }

        int size = BitConverter.ToInt32(sizeBlob.Reverse().ToArray(), 0);

        if (size >= 0x400000000)
        {
            throw new ArgumentException("blob too large");
        }

        byte[] blob = new byte[size];
        int blobBytesRead = stream.Read(blob, 0, size);

        if (blobBytesRead != size)
        {
            throw new ArgumentException("bad encoding");
        }

        return sizeBlob.Skip(1).Concat(blob).ToArray();
    }

    public static byte[] SExpBufferFromStream(Stream stream)
    {
        MemoryStream ret = new MemoryStream();
        int depth = 1;

        while (depth > 0)
        {
            depth--;
            var (buf, d) = ConsumeSExp(stream);
            depth += d;
            ret.Write(buf, 0, buf.Length);
        }

        return ret.ToArray();
    }

    private static byte[] AtomFromStream(Stream stream, byte b, Func<byte[], SExp> toSExp)
    {
        if (b == 0x80)
        {
            return new byte[] { b };
        }

        if (b <= MAX_SINGLE_BYTE)
        {
            return new byte[] { b };
        }

        int bitCount = 0;
        byte bitMask = 0x80;

        while ((b & bitMask) != 0)
        {
            bitCount++;
            b &= (byte)(0xFF ^ bitMask);
            bitMask >>= 1;
        }

        byte[] sizeBlob = new byte[] { b };

        if (bitCount > 1)
        {
            byte[] ll = new byte[bitCount - 1];
            int bytesRead = stream.Read(ll, 0, bitCount - 1);

            if (bytesRead != bitCount - 1)
            {
                throw new ArgumentException("bad encoding");
            }

            sizeBlob = sizeBlob.Concat(ll).ToArray();
        }

        int size = BitConverter.ToInt32(sizeBlob.Reverse().ToArray(), 0);

        if (size >= 0x400000000)
        {
            throw new ArgumentException("blob too large");
        }

        byte[] blob = new byte[size];
        int blobBytesRead = stream.Read(blob, 0, size);

        if (blobBytesRead != size)
        {
            throw new ArgumentException("bad encoding");
        }
        return sizeBlob.Skip(1).Concat(blob).ToArray();
    }
}