namespace CLVMDotNet;

public class OperatorDict : Dictionary<byte[], Func<CLVMObject, Tuple<int, CLVMObject>>>
{
    public byte[] QuoteAtom { get; set; } = new byte[0];
    public byte[] ApplyAtom { get; set; } = new byte[0];
    public Func<byte[], CLVMObject, Tuple<int, CLVMObject>> UnknownOpHandler { get; set; } = DefaultUnknownOp;

    public OperatorDict()
    {
    }

    public OperatorDict(Dictionary<byte[], Func<CLVMObject, Tuple<int, CLVMObject>>> dict, byte[] quote = null,
        byte[] apply = null, Func<byte[], CLVMObject, Tuple<int, CLVMObject>> unknownOpHandler = null)
        : base(dict)
    {
        if (quote != null) QuoteAtom = quote;
        if (apply != null) ApplyAtom = apply;
        if (unknownOpHandler != null) UnknownOpHandler = unknownOpHandler;
    }

    public Tuple<int, CLVMObject> Execute(byte[] op, CLVMObject arguments)
    {
        if (!ContainsKey(op))
        {
            return UnknownOpHandler(op, arguments);
        }
        else
        {
            return this[op](arguments);
        }
    }
    
    private static readonly Dictionary<string, byte[]> KEYWORD_TO_ATOM = KEYWORD_FROM_ATOM
        .ToDictionary(kv => kv.Value.Trim(), kv => kv.Key);


    // Initialize KEYWORDS as a list of strings
    public static List<string> keywordsList = new List<string>
    {
        // core opcodes 0x01-x08
        ". q a i c f r l x",

        // opcodes on atoms as strings 0x09-0x0f
        "= >s sha256 substr strlen concat .",

        // opcodes on atoms as ints 0x10-0x17
        "+ - * / divmod > ash lsh",

        // opcodes on atoms as vectors of bools 0x18-0x1c
        "logand logior logxor lognot .",

        // opcodes for bls 1381 0x1d-0x1f
        "point_add pubkey_for_exp .",

        // bool opcodes 0x20-0x23
        "not any all .",

        // misc 0x24
        "softfork "
    };

    // Split the concatenated string into individual keywords
    public static string[] KEYWORDS = keywordsList
        .SelectMany(s => s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
        .ToArray();

    public static Dictionary<string, string> OP_REWRITE = new Dictionary<string, string>
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

    private static IEnumerable<int> ArgsLen(string opName, IEnumerable<byte[]> args)
    {
        foreach (var arg in args)
        {
            if (arg.Length > 1)
            {
                throw new EvalError($"{opName} requires int args");
            }

            yield return arg.Length;
        }
    }

    public static Tuple<int, CLVMObject> DefaultUnknownOp(byte[] op, CLVMObject args)
    {
        if (op.Length == 0 || BitConverter.ToUInt16(op, 0) == 0xFFFF)
        {
            throw new EvalError("reserved operator");
        }

        var costFunction = (op[0] & 0b11000000) >> 6;
        
        // Remove the last byte from the op array
        byte[] opWithoutLastByte = op.Take(op.Length - 1).ToArray();

        // Convert the opWithoutLastByte to an integer using big-endian byte order
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(opWithoutLastByte);
        }

        byte[] paddedArray = new byte[4];
        opWithoutLastByte.CopyTo(paddedArray, 0);
        
        int costMultiplier = BitConverter.ToInt32(paddedArray, 0) + 1;
        int cost;
        if (costFunction == 0)
        {
            cost = 1;
        }
        else if (costFunction == 1)
        {
            cost = Costs.ARITH_BASE_COST;
            int argSize = 0;
            foreach (var length in ArgsLen("unknown op", args as IEnumerable<byte[]>))
            {
                argSize += length;
                cost += Costs.ARITH_COST_PER_ARG;
            }

            cost += argSize * Costs.ARITH_COST_PER_BYTE;
        }
        else if (costFunction == 2)
        {
            cost = Costs.MUL_BASE_COST;
            var operands = ArgsLen("unknown op", args as IEnumerable<byte[]>).GetEnumerator();

            var vs = operands.MoveNext() ? operands.Current : 0;
            while (operands.MoveNext())
            {
                var rs = operands.Current;
                cost += Costs.MUL_COST_PER_OP;
                cost += (rs + vs) * Costs.MUL_LINEAR_COST_PER_BYTE;
                cost += (rs * vs) / Costs.MUL_SQUARE_COST_PER_BYTE_DIVIDER;
                vs += rs;
            }
        }
        else if (costFunction == 3)
        {
            cost = Costs.CONCAT_BASE_COST;
            int length = 0;
            foreach (var arg in args as IEnumerable<byte[]>)
            {
                cost += Costs.CONCAT_COST_PER_ARG;
                length += arg.Length;
            }

            cost += length * Costs.CONCAT_COST_PER_BYTE;
        }
        else
        {
            throw new EvalError("Invalid cost function");
        }

        cost *= (int)costMultiplier;
        if (cost >= Math.Pow(2, 32))
        {
            throw new EvalError("Invalid operator");
        }

        return new Tuple<int, CLVMObject>(cost, null);
    }
    
    public static Dictionary<byte[], string> KEYWORD_FROM_ATOM = Enumerable.Range(0, KEYWORDS.Length)
        .ToDictionary(k => HelperFunctions.ConvertAtomToBytes((Int32)k), v => KEYWORDS[v]);
    
    public static byte[] QUOTE_ATOM = KEYWORD_TO_ATOM["q"];
    public static byte[]  APPLY_ATOM = KEYWORD_TO_ATOM["a"];
}