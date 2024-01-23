using System.Numerics;

namespace CLVMDotNet.CLVM
{
    public class CoreOps
    {
        
        public static IEnumerable<int> ArgsLen(string opName, SExp args)
        {
            foreach (var arg in args.AsIter())
            {
                if (arg.Pair != null)
                {
                    throw new EvalError(string.Format("{0} requires int args", opName), arg);
                }
                yield return arg.AsAtom().Length;
            }
        }
        
        public static Tuple<BigInteger, SExp> OpDefaultUnknown(byte[] op, SExp args)
        {
            // Any opcode starting with 0xFFFF is reserved (i.e., fatal error).
            // Opcodes are not allowed to be empty.
            if (op.Length == 0 || (op[0] == 0xFF && op[1] == 0xFF))
            {
                throw new EvalError("reserved operator", SExp.To(op));
            }

            // All other unknown opcodes are no-ops.
            // The cost of the no-ops is determined by the opcode number, except the
            // 6 least significant bits.

            int costFunction = (op[op.Length - 1] & 0b11000000) >> 6;
            // The multiplier cannot be 0. It starts at 1.

            if (op.Length > 5)
            {
                throw new EvalError("invalid operator", SExp.To(op));
            }

            BigInteger costMultiplier = new BigInteger(op.Take(op.Length - 1).ToArray()) + 1;

            // 0 = constant
            // 1 = like op_add/op_sub
            // 2 = like op_multiply
            // 3 = like op_concat
            BigInteger cost;
            switch (costFunction)
            {
                case 0:
                    cost = 1;
                    break;
                case 1:
                    // Like op_add
                    cost = Costs.ARITH_BASE_COST;
                    int argSize = 0;
                    foreach (int l in ArgsLen("unknown op", args))
                    {
                        argSize += l;
                        cost += Costs.ARITH_COST_PER_ARG;
                    }

                    cost += argSize * Costs.ARITH_COST_PER_BYTE;
                    break;
                case 2:
                    // Like op_multiply
                    cost = Costs.MUL_BASE_COST;
                    var operands = ArgsLen("unknown op", args).GetEnumerator();
                    try
                    {
                        int vs = operands.MoveNext() ? operands.Current : 0;
                        while (operands.MoveNext())
                        {
                            int rs = operands.Current;
                            cost += Costs.MUL_COST_PER_OP;
                            cost += (rs + vs) * Costs.MUL_LINEAR_COST_PER_BYTE;
                            cost += (rs * vs) / Costs.MUL_SQUARE_COST_PER_BYTE_DIVIDER;
                            // This is an estimate, since we don't want to actually multiply the values.
                            vs += rs;
                        }
                    }
                    catch (Exception)
                    {
                        // Handle StopIteration
                    }

                    break;
                case 3:
                    // Like concat
                    cost = Costs.CONCAT_BASE_COST;
                    int length = 0;
                    foreach (var arg in args.AsIter())
                    {
                        if (arg.Pair != null)
                        {
                            throw new EvalError("unknown op on list", arg);
                        }

                        cost += Costs.CONCAT_COST_PER_ARG;
                        length += arg.Atom.Length;
                    }

                    cost += length * Costs.CONCAT_COST_PER_BYTE;
                    break;
                default:
                    throw new EvalError("invalid operator", SExp.To(op));
            }

            cost *= (int)costMultiplier;
            if (cost >= (BigInteger)1 << 32)
            {
                throw new EvalError("invalid operator", SExp.To(op));
            }

            return Tuple.Create(cost, SExp.NULL);
        }

        public static Tuple<BigInteger, SExp> OpIf(SExp args)
        {
            if (args.ListLength() != 3)
            {
                throw new EvalError("i takes exactly 3 arguments", args);
            }

            SExp r = args.Rest();
            if (args.First().Nullp())
            {
                return new Tuple<BigInteger, SExp>(Costs.IF_COST, r.Rest().First());
            }

            return new Tuple<BigInteger, SExp>(Costs.IF_COST, r.First());
        }

        public static Tuple<BigInteger, SExp> OpCons(SExp args)
        {
            if (args.ListLength() != 2)
            {
                throw new EvalError("c takes exactly 2 arguments", args);
            }

            return new Tuple<BigInteger, SExp>(Costs.CONS_COST, args.First().Cons(args.Rest().First()));
        }

        public static Tuple<BigInteger, SExp> OpFirst(SExp args)
        {
            if (args.ListLength() != 1)
            {
                throw new EvalError("f takes exactly 1 argument", args);
            }

            return new Tuple<BigInteger, SExp>(Costs.FIRST_COST, args.First().First());
        }

        public static Tuple<BigInteger, SExp> OpRest(SExp args)
        {
            if (args.ListLength() != 1)
            {
                throw new EvalError("r takes exactly 1 argument", args);
            }

            return new Tuple<BigInteger, SExp>(Costs.REST_COST, args.First().Rest());
        }

        public static Tuple<BigInteger, SExp> OpListp(SExp args)
        {
            if (args.ListLength() != 1)
            {
                throw new EvalError("l takes exactly 1 argument", args);
            }

            return new Tuple<BigInteger, SExp>(Costs.LISTP_COST, args.First().Listp() ? SExp.True : SExp.False);
        }

        public static Tuple<BigInteger, SExp> OpRaise(SExp args)
        {
            if (args.ListLength() == 1 && !args.First().Listp())
            {
                throw new EvalError("clvm raise", args.First());
            }
            else
            {
                throw new EvalError("clvm raise", args);
            }
        }

        public static Tuple<BigInteger, SExp> OpEq(SExp args)
        {
            if (args.ListLength() != 2)
            {
                throw new EvalError("= takes exactly 2 arguments", args);
            }

            SExp a0 = args.First();
            SExp a1 = args.Rest().First();

            if (a0.Pair != null || a1.Pair != null)
            {
                throw new EvalError("= on list", a0.Pair != null ? a0 : a1);
            }

            byte[] b0 = a0.AsAtom();
            byte[] b1 = a1.AsAtom();

            BigInteger cost = Costs.EQ_BASE_COST;
            cost += (b0!.Length + b1!.Length) * Costs.EQ_COST_PER_BYTE;

            return Tuple.Create(cost, b0.SequenceEqual(b1) ? SExp.True : SExp.False);
        }
    }
}