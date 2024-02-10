using System.Numerics;

namespace CLVMDotNet.CLVM;

public class OperatorDict
{
    public delegate Tuple<BigInteger, SExp> DictDelegate(byte[] op, SExp sexp);
    public DictDelegate? UnknownOpHandler;
    private Dictionary<string, DictDelegate?> OpDictionary = new Dictionary<string, DictDelegate?>();

    public byte[] QuoteAtom { get; set; } = new byte[0];
    public byte[] ApplyAtom { get; set; } = new byte[0];
    
    public OperatorDict(OperatorDict d, Dictionary<string, byte[]>? args, DictDelegate? unknownOp = null)
    {
        // Set quote_atom and apply_atom properties using kwargs or defaults from d
        this.QuoteAtom = args.ContainsKey("quote") ? (byte[])args["quote"] : d.QuoteAtom;
        this.ApplyAtom = args.ContainsKey("apply") ? (byte[])args["apply"] : d.ApplyAtom;
        OpDictionary = d.OpDictionary;
        
        // Set unknown_op_handler property using kwargs or default
        this.UnknownOpHandler = unknownOp ?? DefaultUnknownOp;
    }

    public OperatorDict()
    {
    }
    
    public static OperatorDict OPERATOR_LOOKUP()
    {
        Dictionary<string, DictDelegate?> ops = new Dictionary<string, DictDelegate?>();
        var QUOTE_ATOM = Operators.KEYWORD_TO_ATOM()["q"];
        var APPLY_ATOM = Operators.KEYWORD_TO_ATOM()["a"];


        //core ops
        // ops["0x01"] = (op, sexp) => CoreOps.OpIf(sexp);
        // ops["0x02"] = (op, sexp) => CoreOps.OpIf(sexp);
        ops["0x03"] = (op, sexp) => CoreOps.OpIf(sexp);
        ops["0x04"] = (op, sexp) => CoreOps.OpCons(sexp);
        ops["0x05"] = (op, sexp) => CoreOps.OpFirst(sexp);
        ops["0x06"] = (op, sexp) => CoreOps.OpRest(sexp);
        ops["0x07"] = (op, sexp) => CoreOps.OpListp(sexp);
        ops["0x08"] = (op, sexp) => CoreOps.OpRaise(sexp);
        ops["0x09"] = (op, sexp) => CoreOps.OpEq(sexp);
        
        //more ops
        ops["0x10"] = (op, sexp) => MoreOps.OpAdd(sexp);
        ops["0x11"] = (op, sexp) => MoreOps.OpSubtract(sexp);
        ops["0x12"] = (op, sexp) => MoreOps.OpMultiply(sexp);
        ops["0x13"] = (op, sexp) => MoreOps.OpDiv(sexp);
        ops["0x14"] = (op, sexp) => MoreOps.OpDivmod(sexp);
        ops["0x15"] = (op, sexp) => MoreOps.OpGr(sexp);
        ops["0x16"] = (op, sexp) => MoreOps.OpAsh(sexp);
        ops["0x17"] = (op, sexp) => MoreOps.OpLsh(sexp);
        ops["0x18"] = (op, sexp) => MoreOps.OpLogand(sexp);
        ops["0x19"] = (op, sexp) => MoreOps.OpLogior(sexp);
        ops["0x20"] = (op, sexp) => MoreOps.OpNot(sexp);
        ops["0x1A"] = (op, sexp) => MoreOps.OpLogxor(sexp);
        ops["0x1B"] = (op, sexp) => MoreOps.OpLogNot(sexp);
        ops["0x1D"] = (op, sexp) => MoreOps.OpPointAdd(sexp);
        ops["0x1E"] = (op, sexp) => MoreOps.OpPubkeyForExp(sexp);
        ops["0x0A"] = (op, sexp) => MoreOps.OpGrBytes(sexp);
        ops["0x0B"] = (op, sexp) => MoreOps.OpSha256(sexp);
        ops["0x0C"] = (op, sexp) => MoreOps.OpSubstr(sexp);
        ops["0x0D"] = (op, sexp) => MoreOps.OpStrlen(sexp);
        ops["0x0E"] = (op, sexp) => MoreOps.OpConcat(sexp);
        ops["0x21"] = (op, sexp) => MoreOps.OpAny(sexp);
        ops["0x22"] = (op, sexp) => MoreOps.OpAll(sexp);
        ops["0x23"] = (op, sexp) => MoreOps.OpNot(sexp);
        ops["0x24"] = (op, sexp) => MoreOps.OpSoftfork(sexp);

        var d = new Dictionary<string, byte[]>
        {
            { "quote", QUOTE_ATOM },
            { "apply", APPLY_ATOM }
        };

        OperatorDict p = new OperatorDict();
        p.OpDictionary = ops;
        
        return new OperatorDict(p, d);
    }


    public Tuple<BigInteger, SExp> ApplyOperator(byte[] op, SExp args)
    {
        string hexString = "0x";
        foreach (byte b in op)
        {
            hexString += b.ToString("X2");
        }

        var f = OpDictionary[hexString];
        if (f is null)
            return UnknownOpHandler(op, args);
        else
        {
            return f(Array.Empty<byte>(), args);
        }
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
}