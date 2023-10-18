using System;
using System.IO;
using System.Collections.Generic;
using clvm_dotnet;

public class Serialize
{
    const byte MAX_SINGLE_BYTE = 0x7F;
    const byte CONS_BOX_MARKER = 0xFF;

    public static IEnumerable<byte> SExpToByteIterator(CLVMObject sexp)
    {
        var todoStack = new Stack<CLVMObject>();
        todoStack.Push(sexp);

        while (todoStack.Count > 0)
        {
            sexp = todoStack.Pop();
            var pair = sexp.Pair;
            if (pair != null)
            {
                yield return CONS_BOX_MARKER;
                todoStack.Push(pair.Item2);
                todoStack.Push(pair.Item1);
            }
            else
            {
                foreach (var b in AtomToByteIterator(sexp.AsAtom()))
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

        byte[] sizeBlob;
        if (size < 0x40)
        {
            sizeBlob = new byte[] { (byte)(0x80 | size) };
        }
        else if (size < 0x2000)
        {
            sizeBlob = new byte[] { (byte)(0xC0 | (size >> 8)), (byte)(size & 0xFF) };
        }
        else if (size < 0x100000)
        {
            sizeBlob = new byte[] { (byte)(0xE0 | (size >> 16)), (byte)(size >> 8), (byte)(size & 0xFF) };
        }
        else if (size < 0x8000000)
        {
            sizeBlob = new byte[]
            {
                (byte)(0xF0 | (size >> 24)),
                (byte)(size >> 16),
                (byte)(size >> 8),
                (byte)(size & 0xFF)
            };
        }
        else if (size < 0x400000000)
        {
            sizeBlob = new byte[]
            {
                (byte)(0xF8 | (size >> 32)),
                (byte)(size >> 24),
                (byte)(size >> 16),
                (byte)(size >> 8),
                (byte)(size & 0xFF)
            };
        }
        else
        {
            throw new ArgumentException("Sexp too long");
        }

        foreach (var b in sizeBlob)
        {
            yield return b;
        }
        foreach (var b in asAtom)
        {
            yield return b;
        }
    }

    public static void SExpToStream(CLVMObject sexp, Stream stream)
    {
        foreach (var b in SExpToByteIterator(sexp))
        {
            stream.WriteByte(b);
        }
    }

    private static void OpReadSExp(Stack<Action<Stack<Action<Stack<Action<Stream, Func<byte[], CLVMObject>>>>>>> opStack, Stack<CLVMObject> valStack, Stream stream, Func<byte[], CLVMObject> toSexp)
    {
        var buffer = new byte[1];
        if (stream.Read(buffer, 0, 1) == 0)
        {
            throw new ArgumentException("Bad encoding");
        }
        byte b = buffer[0];
        if (b == CONS_BOX_MARKER)
        {
            opStack.Push(OpCons);
            opStack.Push(OpReadSExp);
            opStack.Push(OpReadSExp);
            return;
        }
        valStack.Push(AtomFromStream(stream, b, toSexp));
    }

    private static void OpCons(Stack<Action<Stream, Func<byte[], CLVMObject>>> opStack, Stack<CLVMObject> valStack, Stream stream, Func<byte[], CLVMObject> toSexp)
    {
        var right = valStack.Pop().Atom!;
        var left = valStack.Pop().Atom!;
        valStack.Push(toSexp(new byte[] { left, right }));
    }

    public static CLVMObject SExpFromStream(Stream stream, Func<byte[], CLVMObject> toSexp)
    {
        var opStack = new Stack<Action<Stream, Func<byte[], CLVMObject>>>();
        var valStack = new Stack<CLVMObject>();

        while (opStack.Count > 0)
        {
            var func = opStack.Pop();
            func(opStack, valStack, stream, toSexp);
        }
        return toSexp(valStack.Pop());
    }

    private static (byte[], int) OpConsumeSExp(Stream stream)
    {
        var buffer = new byte[1];
        if (stream.Read(buffer, 0, 1) == 0)
        {
            throw new ArgumentException("Bad encoding");
        }
        byte b = buffer[0];
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
            var tempBuffer = new byte[bitCount - 1];
            if (stream.Read(tempBuffer, 0, bitCount - 1) != bitCount - 1)
            {
                throw new ArgumentException("Bad encoding");
            }
            sizeBlob = sizeBlob.Concat(tempBuffer).ToArray();
        }

        int size = BitConverter.ToInt32(sizeBlob, 0);
        if (size >= 0x400000000)
        {
            throw new ArgumentException("Blob too large");
        }

        var blob = new byte[size];
        if (stream.Read(blob, 0, size) != size)
        {
            throw new ArgumentException("Bad encoding");
        }

        return new byte[] { b }.Concat(sizeBlob.Skip(1)).Concat(blob).ToArray();
    }

    public static byte[] SExpBufferFromStream(Stream stream)
    {
        var ret = new MemoryStream();

        int depth = 1;
        while (depth > 0)
        {
            depth--;
            var (buf, d) = OpConsumeSExp(stream);
            depth += d;
            ret.Write(buf, 0, buf.Length);
        }
        return ret.ToArray();
    }

    private static CLVMObject AtomFromStream(Stream stream, byte b, Func<byte[], CLVMObject> toSexp)
    {
        if (b == 0x80)
        {
            return toSexp(new byte[] { b });
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
            var tempBuffer = new byte[bitCount - 1];
            if (stream.Read(tempBuffer, 0, bitCount - 1) != bitCount - 1)
            {
                throw new ArgumentException("Bad encoding");
            }
            sizeBlob = sizeBlob.Concat(tempBuffer).ToArray();
        }

        int size = BitConverter.ToInt32(sizeBlob, 0);
        if (size >= 0x400000000)
        {
            throw new ArgumentException("Blob too large");
        }

        var blob = new byte[size];
        if (stream.Read(blob, 0, size) != size)
        {
            throw new ArgumentException("Bad encoding");
        }

        return toSexp(blob);
    }
}
