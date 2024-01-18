using System.Numerics;
using CLVMDotNet.CLVM;

namespace CLVMDotNet.Tools.IR
{
    public class IRType
    {
        public static BigInteger CONS = Utils.ConvertToBase256("CONS"); // Equivalent to b"CONS"
        public static BigInteger NULL = Utils.ConvertToBase256("NULL"); // Equivalent to b"NULL"
        public static BigInteger INT = Utils.ConvertToBase256("INT"); // Equivalent to b"INT"
        public static BigInteger HEX = Utils.ConvertToBase256("HEX"); // Equivalent to b"HEX"
        public static BigInteger QUOTES = Utils.ConvertToBase256("QT"); // Equivalent to b"QT"
        public static BigInteger DOUBLE_QUOTE = Utils.ConvertToBase256("DQT"); // Equivalent to b"DQT"
        public static BigInteger SINGLE_QUOTE = Utils.ConvertToBase256("SQT"); // Equivalent to b"SQT"
        public static BigInteger SYMBOL = Utils.ConvertToBase256("SYM"); // Equivalent to b"SYM"
        public static BigInteger OPERATOR = Utils.ConvertToBase256("OP"); // Equivalent to b"OP"
        public static BigInteger CODE = Utils.ConvertToBase256("CODE"); // Equivalent to b"CODE"
        public static BigInteger NODE = Utils.ConvertToBase256("NODE"); // Equivalent to b"NODE"
        public static BigInteger[] CONS_TYPES => new[] { CONS };

        public static bool Listp()
        {
            return false;
        }

        public byte[] AsAtom(BigInteger val)
        {
            return Casts.IntToBytes(val);
        }

        public static int BitLength(BigInteger val)
        {
            return (int)Math.Ceiling(BigInteger.Log(val, 256) / 8);
        }
    }
}