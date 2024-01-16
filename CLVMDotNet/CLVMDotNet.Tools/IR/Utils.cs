using System.Numerics;
using System.Text;

namespace CLVMDotNet.Tools.IR;

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
    
    public static SExp IrNew(dynamic type, dynamic val, int? offset = null)
    {
        if (offset.HasValue)
        {
            var t = SExp.To(new Tuple<dynamic, dynamic>(type, offset));
            return SExp.To(new Tuple<dynamic, dynamic>(t, val));
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
    
    public static BigInteger IrType(SExp irSexp)
    {
        SExp theType = irSexp.First();
        if (theType.Listp())
        {
            theType = theType.First();
        }

        return Casts.IntFromBytes(theType.AsAtom());
    }
    
    public static BigInteger IrOffset(SExp irSexp)
    {
        SExp theOffset = irSexp.First();
        if (theOffset.Listp())
        {
            theOffset.Atom = theOffset.Rest().AsAtom();
        }
        // else
        // {
        //     theOffset = new SExp(new byte[] { 0xff }); // Assuming b"\xff" in Python corresponds to new byte[] { 0xff } in C#
        // }

        return Casts.IntFromBytes(theOffset.AsAtom());
    }
    
    // public static int IrAsInt(SExp irSexp)
    // {
    //     return Casts.IntFromBytes(IrAsAtom(irSexp));
    // }
    
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