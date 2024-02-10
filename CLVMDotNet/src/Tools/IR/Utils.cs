using System.Numerics;
using System.Text;
using CLVMDotNet.CLVM;

namespace CLVMDotNet.Tools.IR
{

    public class Utils

    {
        public static BigInteger ConvertToBase256(string s)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(s);
            int val = 0;

            for (int i = 0; i < byteArray.Length; i++)
            {
                val = (val << 8) | byteArray[i];
            }

            return val;
        }

        public static string? IrAsSymbol(SExp irSexp)
        {
            if (irSexp.Listp() && IrType(irSexp) == IRType.SYMBOL)
            {
                var sexp = IrAsSexp(irSexp);
                return Encoding.UTF8.GetString(sexp.AsAtom());
            }

            return null;
        }

        public static Tuple<BigInteger, byte[]> IrSymbol(string symbol)
        {
            return new Tuple<BigInteger, byte[]>(IRType.SYMBOL, Encoding.UTF8.GetBytes(symbol));
        }

        public static SExp IrAsSexp(SExp irSexp)
        {
            if (IrNullp(irSexp))
            {
                return new SExp(new List<object>()); // Assuming SExp can be constructed from a List<object>.
            }

            if (IrType(irSexp) == IRType.CONS)
            {
                SExp first = IrAsSexp(IrFirst(irSexp));
                SExp rest = IrAsSexp(IrRest(irSexp));
                return first.Cons(rest);
            }

            return irSexp.Rest();
        }

        public static SExp IrNew(dynamic type, dynamic val, int? offset = null)
        {
            if (offset.HasValue)
            {
                var t = SExp.To(new Tuple<dynamic, dynamic>(type, offset));
                var sexp = SExp.To(new Tuple<dynamic, dynamic>(t, val));
                return sexp;
            }

            return SExp.To(new Tuple<dynamic, dynamic>(type, val));
        }

        public static SExp IrNew(SExp first, SExp rest)
        {
            return SExp.To((first, rest));
        }

        public static SExp IrCons(dynamic first, dynamic rest, int? offset = null)
        {
            return IrNew(IRType.CONS, IrNew(first, rest), offset);
        }

        public static SExp IrNull()
        {
            return IrNew(IRType.NULL, 0);
        }

        public static bool IrListp(SExp irSexp)
        {
            var irType = IrType(irSexp);
            return IRType.CONS_TYPES.Contains(irType);
        }

        public static bool IrNullp(SExp irSexp)
        {
            return IrType(irSexp) == IRType.NULL;
        }

        public static BigInteger IrType(SExp irSexp)
        {
            SExp theType = irSexp.First();
            if (theType.Listp())
            {
                dynamic first = theType.First();
                return Casts.IntFromBytes(first.AsAtom());
            }

            return Casts.IntFromBytes(theType.AsAtom());
        }

        public static SExp IrVal(SExp irSexp)
        {
            return irSexp.Rest();
        }

        public static SExp IrFirst(SExp irSexp)
        {
            return irSexp.Rest().First();
        }

        public static BigInteger IrOffset(SExp irSexp)
        {
            SExp theOffset = irSexp.First();
            if (theOffset.Listp())
            {
                theOffset.Atom = theOffset.Rest().AsAtom();
            }
            else
            {
                theOffset = new SExp(new byte[] { 0xff });
            }

            return Casts.IntFromBytes(theOffset.AsAtom());
        }

        public static BigInteger IrAsInt(SExp irSexp)
        {
            var atom = IrAsAtom(irSexp);
            return Casts.IntFromBytes(atom);
        }

        public static byte[] IrAsAtom(SExp irSexp)
        {
            var rest = irSexp.Rest().AsAtom();
            return rest;
        }

        public static SExp IrRest(SExp irSexp)
        {
            return irSexp.Rest().Rest();
        }

        public static bool IsIr(SExp sexp)
        {
            if (sexp.Atom != null)
            {
                return false;
            }

            Tuple<dynamic, dynamic> pair = sexp.Pair;
            byte[] f = pair.Item1.Atom;
            if (f == null || f.Length > 1)
            {
                return false;
            }

            var theType = Casts.IntFromBytes(f);
            try
            {
                if (theType == IRType.CONS)
                {
                    if (pair.Item2.Atom != null && pair.Item2.Atom.Length == 0)
                    {
                        return true;
                    }

                    if (pair.Item2.Pair != null)
                    {
                        foreach (SExp item in pair.Item2.Pair)
                        {
                            if (!IsIr(item))
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                }

                return pair.Item2.Atom != null;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
    }
}