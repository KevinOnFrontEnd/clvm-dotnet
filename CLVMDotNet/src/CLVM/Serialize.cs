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

using System.Numerics;

namespace CLVMDotNet.CLVM
{

    /// </summary>
    public class Serialize
    {
        const byte MAX_SINGLE_BYTE = 0x7F;


        const byte CONS_BOX_MARKER = 0xFF;

        const byte EMPTY_BYTE = 0x00;

        public delegate void SerializeOperatorDelegate(Stack<SerializeOperatorDelegate> opStack,
            Stack<dynamic> valStack,
            Stream stream, Type to_sexp);

        public static object SexpFromStream(Stream f)
        {
            Stack<SerializeOperatorDelegate> opStack = new Stack<SerializeOperatorDelegate>();
            opStack.Push(OpReadSexp);
            var valStack = new Stack<dynamic>();

            while (opStack.Count > 0)
            {
                var func = opStack.Pop();
                func(opStack, valStack, f, typeof(CLVMObject));
            }

            var val = valStack.Pop();
            return SExp.To(val);
        }

        public static void OpReadSexp(Stack<SerializeOperatorDelegate> opStack, Stack<dynamic> valStack, Stream stream,
            Type to_sexp)
        {
            BinaryReader reader = new BinaryReader(stream);
            byte b = 0;
            var blob = Array.Empty<byte>();

            if (reader.BaseStream.Length == 0)
                throw new Exception("Bad encoding");

            if (reader.BaseStream.Position < reader.BaseStream.Length)
                blob = reader.ReadBytes(1);

            b = blob[0];

            if (b == CONS_BOX_MARKER)
            {
                //Push operations to construct a cons pair
                opStack.Push(OpCons);
                opStack.Push(OpReadSexp);
                opStack.Push(OpReadSexp);
                return;
            }

            //Push an operation to consume the atom and add it to valStack
            var atom = AtomFromStream(reader, b, to_sexp);
            valStack.Push(atom);
        }

        public static void OpCons(Stack<SerializeOperatorDelegate> opStack, Stack<dynamic> valStack, Stream stream,
            Type to_sexp)
        {
            if (valStack.Count < 2)
            {
                throw new InvalidOperationException("Not enough values on val_stack to perform cons operation.");
            }

            var right = valStack.Pop();
            var left = valStack.Pop();

            if (to_sexp == typeof(CLVMObject))
            {
                var obj = new CLVMObject(new Tuple<dynamic, dynamic>(left, right));
                valStack.Push(obj);
            }
        }

        public static dynamic AtomFromStream(BinaryReader reader, byte b, Type to_sexp)
        {
            if (b == 0x80)
            {
                if (to_sexp == typeof(CLVMObject))
                    return new CLVMObject(new byte[0]);

                return SExp.To(new byte[0]);
            }

            if (b <= MAX_SINGLE_BYTE)
            {
                if (to_sexp == typeof(CLVMObject))
                    return new CLVMObject(new byte[] { b });

                return SExp.To(new byte[] { b });
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
                    throw new InvalidOperationException("Bad encoding - AtomFromStream");
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
                throw new InvalidOperationException("Bad encoding");
            }

            return SExp.To(blob);
        }

        public static IEnumerable<byte> SexpToByteIterator(SExp sexp)
        {
            Stack<dynamic> todoStack = new Stack<dynamic>();
            todoStack.Push(sexp);

            while (todoStack.Count > 0)
            {
                var p = todoStack.Pop();
                Tuple<object, object>? pair = p.Pair;

                if (pair != null)
                {
                    todoStack.Push(pair.Item2);
                    todoStack.Push(pair.Item1);

                    yield return CONS_BOX_MARKER;
                }
                else
                {
                    var atom = p.AsAtom();
                    foreach (byte atomByte in AtomToByteIterator(atom))
                    {
                        yield return atomByte;
                    }
                }
            }
        }

        public static Tuple<byte[], int> OpConsumeSexp(Stream stream)
        {
            byte[] blob = new byte[1];
            int bytesRead = stream.Read(blob, 0, 1);

            if (bytesRead == 0)
            {
                throw new Exception("Bad encoding");
            }

            byte b = blob[0];

            if (b == CONS_BOX_MARKER)
            {
                return new Tuple<byte[], int>(blob, 2);
            }
            else
            {
                byte[] atomData = ConsumeAtom(stream, b);
                return new Tuple<byte[], int>(atomData, 0);
            }
        }

        public static byte[] ConsumeAtom(Stream stream, byte b)
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
                ll &= (byte)~bitMask;
                bitMask >>= 1;
            }

            byte[] sizeBlob = new byte[] { ll };

            if (bitCount > 1)
            {
                byte[] llBytes = new byte[bitCount - 1];
                int bytesRead = stream.Read(llBytes, 0, llBytes.Length);

                if (bytesRead != bitCount - 1)
                {
                    throw new InvalidOperationException("Bad encoding - ConsumeAtom");
                }

                sizeBlob = CombineArrays(sizeBlob, llBytes);
            }

            ulong size = BitConverter.ToUInt64(sizeBlob, 0);

            if (size >= 0x400000000)
            {
                throw new InvalidOperationException("Blob too large");
            }

            byte[] blob = new byte[(int)size];
            int bytesReadBlob = stream.Read(blob, 0, blob.Length);

            if (bytesReadBlob != blob.Length)
            {
                throw new InvalidOperationException("Bad encoding");
            }

            byte[] result = new byte[] { b };
            result = CombineArrays(result, sizeBlob, 1);
            result = CombineArrays(result, blob, 0);

            return result;
        }

        public static byte[] CombineArrays(byte[] array1, byte[] array2, int array2StartIndex = 0)
        {
            byte[] result = new byte[array1.Length + array2.Length - array2StartIndex];
            Buffer.BlockCopy(array1, 0, result, 0, array1.Length);
            Buffer.BlockCopy(array2, array2StartIndex, result, array1.Length, array2.Length - array2StartIndex);
            return result;
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
                    throw new InvalidOperationException("Bad encoding");
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
                throw new InvalidOperationException("Bad encoding");
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

        public static byte[] SexpBufferFromStream(Stream stream)
        {
            MemoryStream ret = new MemoryStream();
            int depth = 1;

            while (depth > 0)
            {
                depth--;
                byte[] buf;
                int d;
                (buf, d) = OpConsumeSexp(stream);
                depth += d;
                ret.Write(buf, 0, buf.Length);
            }

            return ret.ToArray();
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
            BigInteger size = asAtom.Length;

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
                yield return (byte)(EMPTY_BYTE | size);
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
}