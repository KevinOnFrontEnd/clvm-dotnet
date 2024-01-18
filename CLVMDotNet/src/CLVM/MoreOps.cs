using System.Numerics;
using System.Security.Cryptography;

namespace CLVMDotNet.CLVM
{
    public static class MoreOps
    {
        private const int MALLOC_COST_PER_BYTE = 1;
        private const int SHA256_BASE_COST = 1; // Define other constants as needed.

        public static Tuple<BigInteger, SExp> MallocCost(BigInteger cost, SExp atom)
        {
            BigInteger newCost = cost + atom.AsAtom().Length * MALLOC_COST_PER_BYTE;
            return Tuple.Create(newCost, atom);
        }

        public static Tuple<BigInteger, SExp> OpSha256(SExp args)
        {
            int cost = SHA256_BASE_COST;
            int argLen = 0;
            using (SHA256 sha256 = SHA256.Create())
            {
                foreach (SExp arg in args.AsIter())
                {
                    byte[] atom = arg.AsAtom();
                    if (atom == null)
                    {
                        throw new EvalError("sha256 on list", arg);
                    }

                    argLen += atom.Length;
                    cost += Costs.SHA256_COST_PER_ARG;
                    sha256.TransformBlock(atom, 0, atom.Length, null, 0);
                }

                sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                byte[] result = sha256.Hash;
                cost += argLen * Costs.SHA256_COST_PER_BYTE;

                return MallocCost(cost, SExp.To(result));
            }
        }

        public static IEnumerable<(BigInteger, int)> ArgsAsInts(string opName, SExp args)
        {
            foreach (SExp arg in args.AsIter())
            {
                if (arg.Pair != null)
                {
                    throw new EvalError($"{opName} requires int args", arg);
                }

                BigInteger intValue = arg.AsInt();
                int atomLength = arg.AsAtom().Length;

                yield return (intValue, atomLength);
            }
        }

        public static IEnumerable<BigInteger> ArgsAsInt32(string opName, SExp args)
        {
            foreach (SExp arg in args.AsIter())
            {
                if (arg != null)
                {
                    throw new EvalError($"{opName} requires int32 args", arg);
                }

                if (arg.AsAtom().Length > 4)
                {
                    throw new EvalError($"{opName} requires int32 args (with no leading zeros)", arg);
                }

                yield return arg.AsInt();
            }
        }

        public static List<(BigInteger, BigInteger)> ArgsAsIntList(string opName, dynamic args, int count)
        {
            var intList = ArgsAsInts(opName, args);
            if (intList.Count != count)
            {
                string plural = count != 1 ? "s" : "";
                throw new EvalError($"{opName} takes exactly {count} argument{plural}", args);
            }

            return intList;
        }

        public static IEnumerable<SExp> ArgsAsBools(string opName, SExp args)
        {
            foreach (var arg in args.AsIter())
            {
                byte[] v = arg.AsAtom();
                if (v.Length == 0)
                {
                    yield return SExp.False;
                }
                else
                {
                    yield return SExp.True;
                }
            }
        }

        public static List<SExp> ArgsAsBoolList(string opName, SExp args, int count)
        {
            List<SExp> boolList = ArgsAsBools(opName, args).ToList();
            if (boolList.Count != count)
            {
                string plural = count != 1 ? "s" : "";
                throw new EvalError($"{opName} takes exactly {count} argument{plural}", args);
            }

            return boolList;
        }

        public static Tuple<BigInteger, SExp> OpAdd(SExp args)
        {
            BigInteger total = 0;
            BigInteger cost = Costs.ARITH_BASE_COST;
            BigInteger argSize = 0;
            foreach ((BigInteger r, BigInteger l) in ArgsAsInts("+", args))
            {
                total += r;
                argSize += l;
                cost += Costs.ARITH_COST_PER_ARG;
            }

            cost += argSize * Costs.ARITH_COST_PER_BYTE;
            return MallocCost(cost, SExp.To(total));
        }

        public static (BigInteger, SExp) OpDivmod(SExp args)
        {
            BigInteger cost = Costs.DIV_BASE_COST;
            var (i0, l0) = ArgsAsIntList("divmod", args, 2)[0];
            var (i1, l1) = ArgsAsIntList("divmod", args, 2)[1];
            if (i1 == 0)
            {
                throw new EvalError("divmod with 0", SExp.To(i0));
            }

            cost += (l0 + l1) * Costs.DIV_COST_PER_BYTE;
            BigInteger q = BigInteger.DivRem(i0, i1, out BigInteger r);
            SExp q1 = SExp.To(q);
            SExp r1 = SExp.To(r);
            cost += (q1.Atom.Length + r1.Atom.Length) * Costs.MALLOC_COST_PER_BYTE;
            return (cost, SExp.To(new List<SExp> { q1, r1 }));
        }

        public static Tuple<BigInteger, SExp> OpDiv(SExp args)
        {
            BigInteger cost = Costs.DIV_BASE_COST;
            var (i0, l0) = ArgsAsIntList("/", args, 2)[0];
            var (i1, l1) = ArgsAsIntList("/", args, 2)[1];
            if (i1 == 0)
            {
                throw new EvalError("div with 0", SExp.To(i0));
            }

            if (i0 < 0 || i1 < 0)
            {
                throw new EvalError("div operator with negative operands is deprecated", args);
            }

            cost += (l0 + l1) * Costs.DIV_COST_PER_BYTE;
            BigInteger q = BigInteger.Divide(i0, i1);

            return MallocCost(cost, SExp.To(q));
        }

        // public (BigInteger, SExp) OpGr(SExp args)
        // {
        //     var (i0, l0) = ArgsAsIntList(">", args, 2);
        //     BigInteger cost = Costs.GR_BASE_COST;
        //     cost += (l0 + ArgsAsIntList(">", args, 2)[1].Item2) * Costs.GR_COST_PER_BYTE;
        //     return (cost, i0 > ArgsAsIntList(">", args, 2)[1].Item1 ? SExp.True : SExp.False);
        // }
//     
//     public (BigInteger, SExp) OpGrBytes(dynamic args)
//     {
//         var argList = args.AsList();
//         if (argList.Count != 2)
//         {
//             throw new EvalError(">s takes exactly 2 arguments", args);
//         }
//         var a0 = argList[0];
//         var a1 = argList[1];
//         if (a0.IsPair || a1.IsPair)
//         {
//             throw new EvalError(">s on list", a0.IsPair ? a0 : a1);
//         }
//         var b0 = a0.AsAtom();
//         var b1 = a1.AsAtom();
//         BigInteger cost = Costs.GRS_BASE_COST;
//         cost += (b0.Length + b1.Length) * Costs.GRS_COST_PER_BYTE;
//         return (cost, b0.CompareTo(b1) > 0 ? args.True : args.False);
//     }
//
//     public (BigInteger, SExp) OpPubkeyForExp(SExp args)
//     {
//         var (i0, l0) = ArgsAsIntList("pubkey_for_exp", args, 1)[0];
//         i0 %= BigInteger.Parse("0x73EDA753299D7D483339D80809A1D80553BDA402FFFE5BFEFFFFFFFF00000001");
//         Exponent exponent = PrivateKey.FromBytes(i0.ToByteArray());
//         try
//         {
//             G1.Generator.ToBytes();
//             SExp r = args.To(exponent.GetG1().ToByteArray());
//             BigInteger cost = Costs.PUBKEY_BASE_COST;
//             cost += l0 * Costs.PUBKEY_COST_PER_BYTE;
//             return MallocCost(cost, r);
//         }
//         catch (Exception ex)
//         {
//             throw new EvalError($"problem in op_pubkey_for_exp: {ex}", args);
//         }
//     }
//
//     public (BigInteger, SExp) OpPointAdd(dynamic items)
//     {
//         BigInteger cost = Costs.POINT_ADD_BASE_COST;
//         G1 p = new G1();
//
//         foreach (var item in items.AsEnumerable())
//         {
//             if (item.IsPair)
//             {
//                 throw new EvalError("point_add on list", item);
//             }
//             try
//             {
//                 p += G1.FromBytes(item.AsAtom());
//                 cost += Costs.POINT_ADD_COST_PER_ARG;
//             }
//             catch (Exception ex)
//             {
//                 throw new EvalError($"point_add expects blob, got {item}: {ex}", items);
//             }
//         }
//         return MallocCost(cost, items.To(p.ToBytes()));
//     }
//
        public static Tuple<BigInteger, SExp> OpStrlen(SExp args)
        {
            if (args.ListLength() != 1)
            {
                throw new EvalError("strlen takes exactly 1 argument", args);
            }

            var a0 = args.First();
            if (a0.Pair != null)
            {
                throw new EvalError("strlen on list", a0);
            }

            int size = a0.AsAtom().Length;
            BigInteger cost = Costs.STRLEN_BASE_COST + size * Costs.STRLEN_COST_PER_BYTE;
            return MallocCost(cost, SExp.To(size));
        }

        public static Tuple<BigInteger, SExp> OpSubstr(SExp args)
        {
            int argCount = args.ListLength();
            if (argCount != 2 && argCount != 3)
            {
                throw new EvalError("substr takes exactly 2 or 3 arguments", args);
            }

            var a0 = args.First();
            if (a0.Pair != null)
            {
                throw new EvalError("substr on list", a0);
            }

            var s0 = a0.AsAtom();

            int i1, i2;
            if (argCount == 2)
            {
                i1 = ArgsAsInt32("substr", args.Rest()).Single();
                i2 = s0.Length;
            }
            else
            {
                var intArgs = ArgsAsInt32("substr", args.Rest()).ToArray();
                i1 = intArgs[0];
                i2 = intArgs[1];
            }

            if (i2 > s0.Length || i2 < i1 || i2 < 0 || i1 < 0)
            {
                throw new EvalError("invalid indices for substr", args);
            }

            var s = s0.SubString(i1, i2 - i1);
            BigInteger cost = 1;
            return MallocCost(cost, SExp.To(s));
        }

        public static Tuple<BigInteger, SExp> OpConcat(SExp args)
        {
            BigInteger cost = Costs.CONCAT_BASE_COST;
            var s = new MemoryStream();
            foreach (var arg in args.AsIter())
            {
                if (arg.Pair != null)
                {
                    throw new EvalError("concat on list", arg);
                }

                var atom = arg.AsAtom();
                s.Write(atom, 0, atom.Length);
                cost += Costs.CONCAT_COST_PER_ARG;
            }

            var r = s.ToArray();
            cost += r.Length * Costs.CONCAT_COST_PER_BYTE;
            return MallocCost(cost, SExp.To(r));
        }

        // public static  Tuple<BigInteger, SExp> OpAsh(SExp args)
        // {
        //     var (i0, l0) = ArgsAsIntList("ash", args, 2);
        //     var (i1, l1) = ArgsAsIntList("ash", args.Rest(), 1).First();
        //
        //     if (l1 > 4)
        //     {
        //         throw new EvalError("ash requires int32 args (with no leading zeros)", args.Rest().First());
        //     }
        //
        //     if (Math.Abs(i1) > 65535)
        //     {
        //         throw new EvalError("shift too large", args.To(i1));
        //     }
        //
        //     BigInteger r;
        //
        //     if (i1 >= 0)
        //     {
        //         r = i0 << (int)i1;
        //     }
        //     else
        //     {
        //         r = i0 >> (int)-i1;
        //     }
        //
        //     BigInteger cost = Costs.ASHIFT_BASE_COST;
        //     cost += (l0 + Casts.LimbsForInt(r)) * Costs.ASHIFT_COST_PER_BYTE;
        //
        //     return MallocCost(cost, SExp.To(r));
        // }
//
//     public (BigInteger, SExp) OpLsh(SExp args)
//     {
//         var (i0, l0) = ArgsAsIntList("lsh", args, 2);
//         var (i1, l1) = ArgsAsIntList("lsh", args.Rest(), 1).First();
//
//         if (l1 > 4)
//         {
//             throw new EvalError("lsh requires int32 args (with no leading zeros)", args.Rest().First());
//         }
//
//         if (Math.Abs(i1) > 65535)
//         {
//             throw new EvalError("shift too large", args.To(i1));
//         }
//
//         // We actually want i0 to be an unsigned int
//         var a0 = args.First().AsAtom();
//         var i0Bytes = a0.Reverse().ToArray(); // Reverse bytes for little-endian representation
//         var i0 = new BigInteger(i0Bytes);
//
//         BigInteger r;
//
//         if (i1 >= 0)
//         {
//             r = i0 << (int)i1;
//         }
//         else
//         {
//             r = i0 >> (int)-i1;
//         }
//
//         BigInteger cost = Costs.LSHIFT_BASE_COST;
//         cost += (l0 + Casts.LimbsForInt(r)) * Costs.LSHIFT_COST_PER_BYTE;
//
//         return (cost, args.To(r));
//     }

        public static Tuple<BigInteger, SExp> BinopReduction(string opName, BigInteger initialValue, SExp args,
            Func<BigInteger, BigInteger, BigInteger> opF)
        {
            BigInteger total = initialValue;
            BigInteger argSize = 0;
            BigInteger cost = Costs.LOG_BASE_COST;

            foreach (var (r, l) in ArgsAsInts(opName, args))
            {
                total = opF(total, r);
                argSize += l;
                cost += Costs.LOG_COST_PER_ARG;
            }

            cost += argSize * Costs.LOG_COST_PER_BYTE;
            return MallocCost(cost, SExp.To(total));
        }

        public static Tuple<BigInteger, SExp> OpLogand(SExp args)
        {
            BigInteger Binop(BigInteger a, BigInteger b) => a & b;

            return BinopReduction("logand", -1, args, Binop);
        }

        public static Tuple<BigInteger, SExp> OpLogior(SExp args)
        {
            BigInteger Binop(BigInteger a, BigInteger b) => a | b;

            return BinopReduction("logior", 0, args, Binop);
        }

        public static Tuple<BigInteger, SExp> OpLogxor(SExp args)
        {
            BigInteger Binop(BigInteger a, BigInteger b) => a ^ b;

            return BinopReduction("logxor", 0, args, Binop);
        }

        // public static Tuple<BigInteger, SExp> OpLognot(SExp args)
        // {
        //     var (i0, l0) = ArgsAsIntList("lognot", args, 1);
        //     BigInteger result = ~i0;
        //     int cost = Costs.LOGNOT_BASE_COST + l0 * Costs.LOGNOT_COST_PER_BYTE;
        //     return MallocCost(cost, SExp.To(result));
        // }

        // public static Tuple<BigInteger, SExp> OpNot(dynamic args)
        // {
        //     List<bool> boolList = ArgsAsBoolList("not", args, 1);
        //     bool i0 = boolList[0];
        //     bool result = !i0;
        //     int cost = Costs.BOOL_BASE_COST;
        //     return MallocCost(cost, args.To(result ? args.True : args.False));
        // }
//     
//     public (int, SExp) OpAny(dynamic args)
//     {
//         List<bool> boolList = ArgsAsBoolList("any", args, 1);
//         int cost = Costs.BOOL_BASE_COST + boolList.Count * Costs.BOOL_COST_PER_ARG;
//         bool result = boolList.Any(v => v);
//         return (cost, args.To(result ? args.True : args.False));
//     }
//
        // public (int, SExp) OpAll(dynamic args)
        // {
        //     List<bool> boolList = ArgsAsBoolList("all", args, 1);
        //     int cost = Costs.BOOL_BASE_COST + boolList.Count * Costs.BOOL_COST_PER_ARG;
        //     bool result = boolList.All(v => v);
        //     return (cost, args.To(result ? args.True : args.False));
        // }


        public static Tuple<BigInteger, SExp> OpSoftfork(SExp args)
        {
            if (args.ListLength() < 1)
            {
                throw new EvalError("softfork takes at least 1 argument", args);
            }

            SExp a = args.First();

            if (a.Pair != null)
            {
                throw new EvalError("softfork requires int args", a);
            }

            var cost = a.AsInt();

            if (cost < 1)
            {
                throw new EvalError("cost must be > 0", args);
            }

            return MallocCost(cost, SExp.False);
        }
    }
}