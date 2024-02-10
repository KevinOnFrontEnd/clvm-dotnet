using System.Numerics;
using System.Linq;

namespace CLVMDotNet.CLVM
{
    public static class Operators
    {
        public static List<string> KEYWORDS = new List<string>
        {
            // core opcodes 0x01-x08
            ".", "q", "a", "i", "c", "f", "r", "l", "x",

            // opcodes on atoms as strings 0x09-0x0f
            "=", ">s", "sha256", "substr" , "strlen", "concat", ".",

            // opcodes on atoms as ints 0x10-0x17
            "+", "-", "*", "/", "divmod", ">", "ash", "lsh",

            // opcodes on atoms as vectors of bools 0x18-0x1c
            "logand", "logior", "logxor", "lognot", ".",

            // opcodes for bls 1381 0x1d-0x1f
            "point_add", "pubkey_for_exp", ".",

            // bool opcodes 0x20-0x23
            "not", "any", "all", ".",

            // misc 0x24
            "softfork"
        };

        public static Dictionary<byte[], string> KEYWORD_FROM_ATOM => KEYWORDS.Select((keyword, index) => new { Index = index, Keyword = keyword })
            .ToDictionary(item => Casts.IntToBytes(item.Index), item => item.Keyword, new ByteArrayComparer());

        public static Dictionary<string, byte[]> KEYWORD_TO_ATOM()
        {
            var dic = new Dictionary<string, byte[]>();
            foreach (var item in KEYWORD_FROM_ATOM)
            {
                dic[item.Value] = item.Key;
            }

            return dic;
        }
        
        public static  Dictionary<string, string> OpRewrite => new Dictionary<string, string>
        {
            { "+", "add" },
            { "-", "subtract" },
            { "*", "multiply" },
            { "/", "div" },
            { "i", "if" },
            { "c", "cons" },
            { "f", "first" },
            { "r", "rest" },
            { "l", "listp" },
            { "x", "raise" },
            { "=", "eq" },
            { ">", "gr" },
            { ">s", "gr_bytes" }
        };

        public static byte[] QUOTE_ATOM => KEYWORD_TO_ATOM()["q"];
        public static byte[] APPLY_ATOM => KEYWORD_TO_ATOM()["a"];

        // {
        //     public static byte[] QuoteAtom { get; set; } = new byte[0];
        //     public static byte[] ApplyAtom { get; set; } = new byte[0];
        //
        //     /// <summary>
        //     /// Apply Operator to an atom.
        //     ///
        //     /// This is not the tidiest or most efficient way of executing an operator. The
        //     /// python version uses a dictionary to lookup the function. This will likely be changed to that
        //     /// to make it more efficient, rather than executing 30 if statements which ultimately slows down the function.
        //     ///
        //     /// </summary>
        //     /// <param name="atom"></param>
        //     /// <param name="args"></param>
        //     /// <returns></returns>
        //     /// <exception cref="ArgumentException"></exception>
        //     public static Tuple<BigInteger, SExp> ApplyOperator(byte[] atom, SExp args)
        //     {
        //         // core op codes 0x01-x08
        //
        //         if (atom.AsSpan().SequenceEqual(new byte[] { 0x23  }))
        //         {
        //             //. (#)
        //             return CoreOps.OpDefaultUnknown(atom,args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x01 }))
        //         {
        //             //q
        //             return CoreOps.OpDefaultUnknown(atom,args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x02 }))
        //         {
        //             //a
        //             return CoreOps.OpDefaultUnknown(atom, args);
        //         }
        //
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x03 }))
        //         {
        //             //i
        //             return CoreOps.OpIf(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x04 }))
        //         {
        //             //c
        //             return CoreOps.OpCons(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x05 }))
        //         {
        //             //f
        //             return CoreOps.OpFirst(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x06 }))
        //         {
        //             //r
        //             return CoreOps.OpRest(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x07 }))
        //         {
        //             //l
        //             return CoreOps.OpListp(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x08 }))
        //         {
        //             //x
        //             return CoreOps.OpRaise(args);
        //         }
        //
        //
        //         //opcodes on atoms as strings 0x09-0x0f
        //         if (atom.AsSpan().SequenceEqual(new byte[] { 0x09 }))
        //         {
        //             //=
        //             return CoreOps.OpEq(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x0A }))
        //         {
        //             //>s
        //             return MoreOps.OpGrBytes(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x0B }))
        //         {
        //             //sha256
        //             return MoreOps.OpSha256(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x0C }))
        //         {
        //             //substr
        //             return MoreOps.OpSubstr(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x0D }))
        //         {
        //             //strlen
        //             return MoreOps.OpStrlen(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x0E }))
        //         {
        //             //concat
        //             return MoreOps.OpConcat(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x0F }))
        //         {
        //             //.
        //             throw new ArgumentException("Op Not Implemented!");
        //         }
        //
        //
        //         //op codes on atoms as ints
        //         if (atom.AsSpan().SequenceEqual(new byte[] { 0x10 }))
        //         {
        //             //+
        //             return MoreOps.OpAdd(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x11 }))
        //         {
        //             //-
        //             return MoreOps.OpSubtract(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x12 }))
        //         {
        //             //*
        //             return MoreOps.OpMultiply(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x13 }))
        //         {
        //             //divide
        //             return MoreOps.OpDiv(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x14 }))
        //         {
        //             //divmod
        //             return MoreOps.OpDivmod(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x15 }))
        //         {
        //             //>
        //             return MoreOps.OpGr(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x16 }))
        //         {
        //             //ash
        //             return MoreOps.OpAsh(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x17 }))
        //         {
        //             //lsh
        //             return MoreOps.OpLsh(args);
        //         }
        //
        //         // opcodes on atoms as vectors of bools 0x18-0x1c
        //         if (atom.AsSpan().SequenceEqual(new byte[] { 0x18 }))
        //         {
        //             //logand
        //             return MoreOps.OpLogand(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x19 }))
        //         {
        //             //logior
        //             return MoreOps.OpLogior(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x1A }))
        //         {
        //             //logxor
        //             return MoreOps.OpLogxor(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x1B }))
        //         {
        //             //lognot
        //             return MoreOps.OpLogNot(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x1C }))
        //         {
        //             //.
        //             throw new ArgumentException("Op Not Implemented!");
        //         }
        //
        //
        //         //opcodes for bls 1381 0x1d-0x1f
        //         if (atom.AsSpan().SequenceEqual(new byte[] { 0x1D }))
        //         {
        //             //point_add
        //             return MoreOps.OpPointAdd(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x1E }))
        //         {
        //             //pubkey_for_exp
        //             return MoreOps.OpPubkeyForExp(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x1F }))
        //         {
        //             //.
        //             throw new ArgumentException("Op Not Implemented!");
        //         }
        //
        //         // bool opcodes 0x20-0x23
        //         if (atom.AsSpan().SequenceEqual(new byte[] { 0x20 }))
        //         {
        //             //not
        //             return MoreOps.OpNot(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x21 }))
        //         {
        //             //any
        //             return MoreOps.OpAny(args);
        //         }
        //         else if (atom.AsSpan().SequenceEqual(new byte[] { 0x22 }))
        //         {
        //             //all
        //             return MoreOps.OpAll(args);
        //         }
        //         if (atom.AsSpan().SequenceEqual(new byte[] { 0x24 }))
        //         {
        //             //softfork
        //             return MoreOps.OpSoftfork(args);
        //         }
        //
        //         throw new Exception($"{BitConverter.ToString(atom).Replace("-", "")} Operator not found or is unsupported!");
        //     }
    }
    
    public class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            if (x == null || y == null)
                return x == y;

            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                    return false;
            }

            return true;
        }

        public int GetHashCode(byte[] obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            int hash = 17;
            foreach (byte b in obj)
            {
                hash = hash * 31 + b;
            }
            return hash;
        }
    }
}