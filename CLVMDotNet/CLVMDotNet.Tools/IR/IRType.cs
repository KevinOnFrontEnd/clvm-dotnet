using System.Numerics;

namespace CLVMDotNet.Tools.IR;

public class IRType
{
    public static IRType CONS = new IRType(Utils.ConvertToBase256("CONS")); // Equivalent to b"CONS"
    public static IRType NULL = new IRType(Utils.ConvertToBase256("NULL")); // Equivalent to b"NULL"
    public static IRType INT = new IRType(Utils.ConvertToBase256("INT")); // Equivalent to b"INT"
    public static IRType HEX = new IRType(Utils.ConvertToBase256("HEX")); // Equivalent to b"HEX"
    public static IRType QUOTES = new IRType(Utils.ConvertToBase256("QT")); // Equivalent to b"QT"
    public static IRType DOUBLE_QUOTE = new IRType(Utils.ConvertToBase256("DQT")); // Equivalent to b"DQT"
    public static IRType SINGLE_QUOTE = new IRType(Utils.ConvertToBase256("SQT")); // Equivalent to b"SQT"
    public static IRType SYMBOL = new IRType(Utils.ConvertToBase256("SYM")); // Equivalent to b"SYM"
    public static IRType OPERATOR = new IRType(Utils.ConvertToBase256("OP")); // Equivalent to b"OP"
    public static IRType CODE = new IRType(Utils.ConvertToBase256("CODE")); // Equivalent to b"CODE"
    public static IRType NODE = new IRType(Utils.ConvertToBase256("NODE")); // Equivalent to b"NODE"
    public BigInteger _val { get; private set; }
    
    public IRType(BigInteger val)
    {
        _val = val;
    }
    
    public bool Listp()
    {
        return false;
    }

    public byte[] AsAtom()
    {
        return Casts.IntToBytes(_val);
    }

    public int BitLength()
    {
        return (int)Math.Ceiling(BigInteger.Log(_val, 256) / 8);
    }
}