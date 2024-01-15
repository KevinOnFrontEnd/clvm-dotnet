using System.Numerics;

namespace CLVMDotNet.Tools.IR;

public class Utils
{
    public static SExp IrNew(IRType type, dynamic val, int? offset = null)
    {
        if (offset.HasValue)
        {
            return SExp.To(new Tuple<BigInteger, BigInteger>(type._val, offset.Value));
        }

        return SExp.To(new Tuple<BigInteger, dynamic>(type._val, val));
    }
    
    public static SExp IrNew(SExp first, SExp rest)
    {
        return SExp.To((first, rest));
    }
    
    public static SExp IrCons(SExp first, SExp rest, int? offset = null)
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
            IRType t = new IRType(theType);
            if (t._val == IRType.CONS._val)
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