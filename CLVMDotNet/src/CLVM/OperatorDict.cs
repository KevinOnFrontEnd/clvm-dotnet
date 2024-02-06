using System.Numerics;

namespace CLVMDotNet.CLVM;

public class OperatorDict
{
    private delegate Tuple<BigInteger, SExp> DictDelegate(SExp sexp);
    private Dictionary<byte[], DictDelegate?> Dictionary = new Dictionary<byte[], DictDelegate>();

    public OperatorDict(OperatorDict dict)
    {
        
    }
    
    public static IEnumerable<int> ArgsLen(string op_name, SExp args)
    {
        foreach (var arg in args.AsIter())
        {
            if (arg.Pair != null)
            {
                throw new EvalError($"{op_name} requires int args", arg);
            }
            yield return arg.Atom!.Length;
        }
    }
    
    public static Tuple<BigInteger, SExp> DefaultUnknownOp(byte[] op, SExp args)
    {
        // any opcode starting with ffff is reserved (i.e. fatal error)
        // opcodes are not allowed to be empty
        if (op.Length == 0 || (op[0] == 0xff && op[1] == 0xff))
        {
            throw new EvalError("reserved operator");
        }

        // all other unknown opcodes are no-ops
        // the cost of the no-ops is determined by the opcode number, except the
        // 6 least significant bits.

        byte costFunction = (byte)((op[op.Length - 1] & 0b11000000) >> 6);
        // the multiplier cannot be 0. it starts at 1

        if (op.Length > 5)
        {
            throw new EvalError("invalid operator");
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
                // like op_add
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
                // like op_multiply
                cost = Costs.MUL_BASE_COST;
                IEnumerator<int> operands = ArgsLen("unknown op", args).GetEnumerator();
                try
                {
                    int vs = operands.MoveNext() ? operands.Current : 0;
                    while (operands.MoveNext())
                    {
                        int rs = operands.Current;
                        cost += Costs.MUL_COST_PER_OP;
                        cost += (rs + vs) * Costs.MUL_LINEAR_COST_PER_BYTE;
                        cost += (rs * vs) / Costs.MUL_SQUARE_COST_PER_BYTE_DIVIDER;
                        // this is an estimate, since we don't want to actually multiply the
                        // values
                        vs += rs;
                    }
                }
                catch (Exception){}
                break;
            case 3:
                // like concat
                cost = Costs.CONCAT_BASE_COST;
                int length = 0;
                foreach (SExp arg in args.AsIter())
                {
                    if (arg.AsPair() != null)
                    {
                        throw new EvalError("unknown op on list");
                    }
                    cost += Costs.CONCAT_COST_PER_ARG;
                    length += arg.AsAtom().Length;
                }
                cost += length * Costs.CONCAT_COST_PER_BYTE;
                break;
            default:
                throw new EvalError("Invalid cost function");
        }

        cost = (int)(cost * costMultiplier);
        if (cost >= (1 << 32))
        {
            throw new EvalError("invalid operator");
        }

        return Tuple.Create(cost, SExp.NULL);
    }
    
    public Tuple<BigInteger, SExp> ApplyOperator(byte[] op, SExp arguments)
    {
        var func = Dictionary[op];
        if (func != null)
        {
            var result = func(arguments);
            return result;
        }
        else
        {
            return DefaultUnknownOp(op, arguments);
        }
    }
}