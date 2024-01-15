using System.Numerics;

namespace CLVMDotNet.Tools.IR;

public class IRType
{
    public static IRType CONS = new IRType(0x434F4E53); // Equivalent to b"CONS"
    public static IRType NULL = new IRType(0x4E554C4C); // Equivalent to b"NULL"
    public static IRType INT = new IRType(0x494E54); // Equivalent to b"INT"
    public static IRType HEX = new IRType(0x484558); // Equivalent to b"HEX"
    public static IRType QUOTES = new IRType(0x5154); // Equivalent to b"QT"
    public static IRType DOUBLE_QUOTE = new IRType(0x44515454); // Equivalent to b"DQT"
    public static IRType SINGLE_QUOTE = new IRType(0x53515454); // Equivalent to b"SQT"
    public static IRType SYMBOL = new IRType(0x53594D42); // Equivalent to b"SYM"
    public static IRType OPERATOR = new IRType(0x4F50); // Equivalent to b"OP"
    public static IRType CODE = new IRType(0x434F4445); // Equivalent to b"CODE"
    public static IRType NODE = new IRType(0x4E4F4445); // Equivalent to b"NODE"
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