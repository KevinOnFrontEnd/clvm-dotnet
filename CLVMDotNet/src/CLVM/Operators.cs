using System.Numerics;

namespace CLVMDotNet.CLVM
{
    public static class Operator
    {
        public static byte[] QuoteAtom { get; set; } = new byte[0];
        public static byte[] ApplyAtom { get; set; } = new byte[0];

        /// <summary>
        /// Apply Operator to an atom.
        ///
        /// This is not the tidiest or most efficient way of executing an operator. The
        /// python version uses a dictionary to lookup the function. This will likely be changed to that
        /// to make it more efficient, rather than executing 30 if statements which ultimately slows down the function.
        ///
        /// TODO: MAKE SURE THIS IS CHANGED TO A Dictionary to lookup the Operator function.
        /// </summary>
        /// <param name="atom"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Tuple<BigInteger, SExp> ApplyOperator(byte[] atom, SExp args)
        {
            // core op codes 0x01-x08
            if (atom.AsSpan().SequenceEqual(new byte[] { 0x01 }))
            {
                //.
                //return MoreOps.OpMultiply(args);
                throw new ArgumentException("Op Not Implemented!");
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x02 }))
            {
                //q
                //return MoreOps.OpMultiply(args);
                throw new ArgumentException("Op Not Implemented!");
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x03 }))
            {
                //a
                //return MoreOps.OpMultiply(args);
                throw new ArgumentException("Op Not Implemented!");
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x04 }))
            {
                //i
                //return MoreOps.OpMultiply(args);
                throw new ArgumentException("Op Not Implemented!");
            }           
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x05 }))
            {
                //c
                //return MoreOps.OpMultiply(args);
                throw new ArgumentException("Op Not Implemented!");
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x06 }))
            {
                //f
                //return MoreOps.OpMultiply(args);
                throw new ArgumentException("Op Not Implemented!");
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x07 }))
            {
                //r
                //return MoreOps.OpMultiply(args);
                throw new ArgumentException("Op Not Implemented!");
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x08 }))
            {
                //l
                //return MoreOps.OpMultiply(args);
                throw new ArgumentException("Op Not Implemented!");
            }
            //x - note sure what this byte should be
            // else if (atom.AsSpan().SequenceEqual(new byte[] { 0x17 }))
            // {
            //     //x
            //     //return MoreOps.OpMultiply(args);
            //     throw new ArgumentException("Op Not Implemented!");
            // }
            
            
            //opcodes on atoms as strings 0x09-0x0f
            if (atom.AsSpan().SequenceEqual(new byte[] { 0x09 }))
            {
                //=
                return CoreOps.OpEq(args);
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x0A }))
            {
                //>s
                //return MoreOps.OpSubtract(args);
                throw new ArgumentException("Op Not Implemented!");
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x0B }))
            {
                //sha256
                return MoreOps.OpSha256(args);
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x0C }))
            {
                //substr
                return MoreOps.OpSubstr(args);
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x0D }))
            {
                //strlen
                return MoreOps.OpStrlen(args);
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x0E }))
            {
                //concat
                return MoreOps.OpConcat(args);
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x0F }))
            {
                //.
                //return MoreOps.OpSubtract(args);
                throw new ArgumentException("Op Not Implemented!");
            }
            
            
            //op codes on atoms as ints
            if (atom.AsSpan().SequenceEqual(new byte[] { 0x10 }))
            {
                //+
                return MoreOps.OpAdd(args);
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x11 }))
            {
                //-
                return MoreOps.OpSubtract(args);
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x12 }))
            {
                //*
                return MoreOps.OpDiv(args);
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x13 }))
            {
                // divide
                return MoreOps.OpMultiply(args);
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x14 }))
            {
                //divmod
                return MoreOps.OpDivmod(args);
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x15 }))
            {
                //>
                return MoreOps.OpGr(args);
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x16 }))
            {
                //ash
                //return MoreOps.OpMultiply(args);
                throw new ArgumentException("Op Not Implemented!");
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x17 }))
            {
                //lsh
                //return MoreOps.OpMultiply(args);
                throw new ArgumentException("Op Not Implemented!");
            }
            
            // opcodes on atoms as vectors of bools 0x18-0x1c
            if (atom.AsSpan().SequenceEqual(new byte[] { 0x18 }))
            {
                //logand
                throw new ArgumentException("Op Not Implemented!");
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x19 }))
            {
                //logior
                throw new ArgumentException("Op Not Implemented!");
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x1A }))
            {
                //logxor
                throw new ArgumentException("Op Not Implemented!");
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x1B }))
            {
                //lognot
                throw new ArgumentException("Op Not Implemented!");
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x1C }))
            {
                //.
                throw new ArgumentException("Op Not Implemented!");
            }

            
            //opcodes for bls 1381 0x1d-0x1f
            if (atom.AsSpan().SequenceEqual(new byte[] { 0x1D }))
            {
                //point_add
                throw new ArgumentException("Op Not Implemented!");
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x1E }))
            {
                //pubkey_for_exp
                throw new ArgumentException("Op Not Implemented!");
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x1F }))
            {
                //.
                throw new ArgumentException("Op Not Implemented!");
            }

            // bool opcodes 0x20-0x23
            if (atom.AsSpan().SequenceEqual(new byte[] { 0x20 }))
            {
                //not
                throw new ArgumentException("Op Not Implemented!");
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x21 }))
            {
                //any
                throw new ArgumentException("Op Not Implemented!");
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x22 }))
            {
                //all
                throw new ArgumentException("Op Not Implemented!");
            }
            else if (atom.AsSpan().SequenceEqual(new byte[] { 0x23 }))
            {
                //.
                throw new ArgumentException("Op Not Implemented!");
            }
            
            if (atom.AsSpan().SequenceEqual(new byte[] { 0x24 }))
            {
                //softfork
                throw new ArgumentException("Op Not Implemented!");
            }

            return null;
        }
    }

    //
    // public class OperatorDict : Dictionary<byte[], Func<CLVMObject, Tuple<int, CLVMObject>>>
    // {
    //
    //     public Func<byte[], CLVMObject, Tuple<int, CLVMObject>> UnknownOpHandler { get; set; } = DefaultUnknownOp;
    //
    //     public OperatorDict()
    //     {
    //     }
    //
    //     public OperatorDict(Dictionary<byte[], Func<CLVMObject, Tuple<int, CLVMObject>>> dict, byte[] quote = null,
    //         byte[] apply = null, Func<byte[], CLVMObject, Tuple<int, CLVMObject>> unknownOpHandler = null)
    //         : base(dict)
    //     {
    //         if (quote != null) QuoteAtom = quote;
    //         if (apply != null) ApplyAtom = apply;
    //         if (unknownOpHandler != null) UnknownOpHandler = unknownOpHandler;
    //     }
    //
    //     public Tuple<int, CLVMObject> Execute(byte[] op, CLVMObject arguments)
    //     {
    //         if (!ContainsKey(op))
    //         {
    //             return UnknownOpHandler(op, arguments);
    //         }
    //         else
    //         {
    //             return this[op](arguments);
    //         }
    //     }
    //
    //     
    //     public static Dictionary<string, string> OP_REWRITE = new Dictionary<string, string>
    //     {
    //         { "+", "add" },
    //         { "-", "subtract" },
    //         { "*", "multiply" },
    //         { "/", "div" },
    //         { "i", "if" },
    //         { "c", "cons" },
    //         { "f", "first" },
    //         { "r", "rest" },
    //         { "l", "listp" },
    //         { "x", "raise" },
    //         { "=", "eq" },
    //         { ">", "gr" },
    //         { ">s", "gr_bytes" }
    //     };
    //
    //     private static IEnumerable<int> ArgsLen(string opName, IEnumerable<byte[]> args)
    //     {
    //         foreach (var arg in args)
    //         {
    //             if (arg.Length > 1)
    //             {
    //                 throw new EvalError($"{opName} requires int args");
    //             }
    //
    //             yield return arg.Length;
    //         }
    //     }
    //
    //     public static Tuple<int, CLVMObject> DefaultUnknownOp(byte[] op, CLVMObject args)
    //     {
    //         if (op.Length == 0 || BitConverter.ToUInt16(op, 0) == 0xFFFF)
    //         {
    //             throw new EvalError("reserved operator");
    //         }
    //
    //         var costFunction = (op[0] & 0b11000000) >> 6;
    //
    //         // Remove the last byte from the op array
    //         byte[] opWithoutLastByte = op.Take(op.Length - 1).ToArray();
    //
    //         // Convert the opWithoutLastByte to an integer using big-endian byte order
    //         if (BitConverter.IsLittleEndian)
    //         {
    //             Array.Reverse(opWithoutLastByte);
    //         }
    //
    //         byte[] paddedArray = new byte[4];
    //         opWithoutLastByte.CopyTo(paddedArray, 0);
    //
    //         int costMultiplier = BitConverter.ToInt32(paddedArray, 0) + 1;
    //         int cost;
    //         if (costFunction == 0)
    //         {
    //             cost = 1;
    //         }
    //         else if (costFunction == 1)
    //         {
    //             cost = Costs.ARITH_BASE_COST;
    //             int argSize = 0;
    //             foreach (var length in ArgsLen("unknown op", args as IEnumerable<byte[]>))
    //             {
    //                 argSize += length;
    //                 cost += Costs.ARITH_COST_PER_ARG;
    //             }
    //
    //             cost += argSize * Costs.ARITH_COST_PER_BYTE;
    //         }
    //         else if (costFunction == 2)
    //         {
    //             cost = Costs.MUL_BASE_COST;
    //             var operands = ArgsLen("unknown op", args as IEnumerable<byte[]>).GetEnumerator();
    //
    //             var vs = operands.MoveNext() ? operands.Current : 0;
    //             while (operands.MoveNext())
    //             {
    //                 var rs = operands.Current;
    //                 cost += Costs.MUL_COST_PER_OP;
    //                 cost += (rs + vs) * Costs.MUL_LINEAR_COST_PER_BYTE;
    //                 cost += (rs * vs) / Costs.MUL_SQUARE_COST_PER_BYTE_DIVIDER;
    //                 vs += rs;
    //             }
    //         }
    //         else if (costFunction == 3)
    //         {
    //             cost = Costs.CONCAT_BASE_COST;
    //             int length = 0;
    //             foreach (var arg in args as IEnumerable<byte[]>)
    //             {
    //                 cost += Costs.CONCAT_COST_PER_ARG;
    //                 length += arg.Length;
    //             }
    //
    //             cost += length * Costs.CONCAT_COST_PER_BYTE;
    //         }
    //         else
    //         {
    //             throw new EvalError("Invalid cost function");
    //         }
    //
    //         cost *= (int)costMultiplier;
    //         if (cost >= Math.Pow(2, 32))
    //         {
    //             throw new EvalError("Invalid operator");
    //         }
    //
    //         return new Tuple<int, CLVMObject>(cost, null);
    //     }
    //     
    //     public static byte[] QUOTE_ATOM = Keywords.KEYWORD_TO_ATOM["q"];
    //     public static byte[] APPLY_ATOM = Keywords.KEYWORD_TO_ATOM["a"];
    // }
}