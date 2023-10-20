/// <summary>
// decoding:
// read a byte
// if it's 0x80, it's nil (which might be same as 0)
// if it's 0xff, it's a cons box. Read two items, build cons
// otherwise, number of leading set bits is length in bytes to read size
// For example, if the bit fields of the first byte read are:
// 10xx xxxx -> 1 byte is allocated for size_byte, and the value of the size is 00xx xxxx
// 110x xxxx -> 2 bytes are allocated for size_byte, and the value of the size 000x xxxx xxxx xxxx
// 1110 xxxx -> 3 bytes allocated. The size is 0000 xxxx xxxx xxxx xxxx xxxx
// 1111 0xxx -> 4 bytes allocated.
// 1111 10xx -> 5 bytes allocated.
// If the first byte read is one of the following:
// 1000 0000 -> 0 bytes : nil
// 0000 0000 -> 1 byte : zero (b'\x00')
/// </summary>
public class Serialize
{
    const byte MAX_SINGLE_BYTE = 0x7F;

    const byte CONS_BOX_MARKER = 0xFF;

    public static IEnumerable<byte> SexpToByteIterator(SExp sexp)
    {
        Stack<SExp> todoStack = new Stack<SExp>();
        todoStack.Push(sexp);

        while (todoStack.Count > 0)
        {
            SExp currentSexp = todoStack.Pop();
            var pair = currentSexp.AsPair();

            if (pair != null)
            {
                yield return CONS_BOX_MARKER; // Replace CONS_BOX_MARKER with the appropriate byte value
                todoStack.Push(pair.Item2);
                todoStack.Push(pair.Item1);
            }
            else
            {
                foreach (byte b in AtomToByteIterator(currentSexp.AsAtom()))
                {
                    yield return b;
                }
            }
        }
    }

    public void OpCons(Stack<Action> opStack, Stack<SExp> valStack, Func<dynamic, SExp> toSexp)
    {
        if (valStack.Count < 2)
        {
            throw new InvalidOperationException("Not enough values on val_stack to perform cons operation.");
        }

        SExp right = valStack.Pop();
        SExp left = valStack.Pop();

        valStack.Push(toSexp(new Tuple<dynamic, dynamic>(left, right)));
    }
    
    public void OpReadSexp(Stack<Action> opStack, Stack<SExp> valStack, Stream stream, Func<dynamic, SExp> toSexp)
    {
        BinaryReader reader = new BinaryReader(stream);
        byte b = reader.ReadByte();

        if (b == CONS_BOX_MARKER)
        {
            // Push operations to construct a cons pair
            opStack.Push(() => OpReadSexp(opStack, valStack, stream, toSexp));
            opStack.Push(() => OpReadSexp(opStack, valStack, stream, toSexp));
            opStack.Push(() => OpCons(opStack, valStack, toSexp));
        }
        else
        {
            // Push an operation to consume the atom and add it to valStack
            opStack.Push(() => AtomFromStream(reader, b, toSexp));
        }
    }
    
    public SExp AtomFromStream(BinaryReader reader, byte b, Func<byte[], SExp> toSexp)
    {
        if (b == 0x80)
        {
            return toSexp(new byte[0]);
        }

        if (b <= MAX_SINGLE_BYTE)
        {
            return toSexp(new byte[] { b });
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
            byte[] llBytes = reader.ReadBytes(bitCount - 1);
            if (llBytes.Length != bitCount - 1)
            {
                throw new InvalidOperationException("bad encoding");
            }
            sizeBlob = ConcatBytes(sizeBlob, llBytes);
        }

        ulong size = BitConverter.ToUInt64(sizeBlob, 0);

        if (size >= 0x400000000)
        {
            throw new InvalidOperationException("blob too large");
        }

        byte[] blob = reader.ReadBytes((int)size);
        if (blob.Length != (int)size)
        {
            throw new InvalidOperationException("bad encoding");
        }

        return toSexp(blob);
    }
    
    public byte[] ConsumeAtom(BinaryReader reader, byte b)
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
        byte ll = b;

        while ((ll & bitMask) != 0)
        {
            bitCount++;
            ll &= (byte)(0xFF ^ bitMask);
            bitMask >>= 1;
        }

        byte[] sizeBlob = new byte[] { ll };

        if (bitCount > 1)
        {
            byte[] llBytes = reader.ReadBytes(bitCount - 1);
            if (llBytes.Length != bitCount - 1)
            {
                throw new InvalidOperationException("bad encoding");
            }
            sizeBlob = ConcatBytes(sizeBlob, llBytes);
        }

        ulong size = BitConverter.ToUInt64(sizeBlob, 0);

        if (size >= 0x400000000)
        {
            throw new InvalidOperationException("blob too large");
        }

        byte[] blob = reader.ReadBytes((int)size);
        if (blob.Length != (int)size)
        {
            throw new InvalidOperationException("bad encoding");
        }

        byte[] result = new byte[] { b };
        result = ConcatBytes(result, SliceBytes(sizeBlob, 1));
        result = ConcatBytes(result, blob);
        return result;
    }

    public static byte[] ConcatBytes(byte[] first, byte[] second)
    {
        byte[] result = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, result, 0, first.Length);
        Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
        return result;
    }

    public static byte[] SliceBytes(byte[] source, int startIndex)
    {
        byte[] result = new byte[source.Length - startIndex];
        Buffer.BlockCopy(source, startIndex, result, 0, result.Length);
        return result;
    } 
    
    public byte[] SexpBufferFromStream(Stream stream)
    {
        using (BinaryReader reader = new BinaryReader(stream))
        {
            using (MemoryStream ret = new MemoryStream())
            {
                int depth = 1;
                while (depth > 0)
                {
                    depth--;

                    // Read a byte from the stream
                    byte currentByte = reader.ReadByte();
                    ret.WriteByte(currentByte);

                    // Check if it's the marker for a pair (adjust as needed)
                    if (currentByte == CONS_BOX_MARKER)
                    {
                        depth++;
                    }
                }
                return ret.ToArray();
            }
        }
    }
    
    public static void SexpToStream(SExp sexp, Stream stream)
    {
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            foreach (byte b in SexpToByteIterator(sexp))
            {
                writer.Write(b);
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
}