using System.Text;
using clvm_dotnet;


/// <summary>
//  SExp provides higher level API on top of any object implementing the CLVM
//  object protocol.
//  The tree of values is not a tree of SExp objects, it's a tree of CLVMObject
//  like objects. SExp simply wraps them to privide a uniform view of any
//  underlying conforming tree structure.
//
//  The CLVM object protocol (concept) exposes two attributes:
//  1. "atom" which is either None or bytes
//  2. "pair" which is either None or a tuple of exactly two elements. Both
// elements implementing the CLVM object protocol.
// Exactly one of "atom" and "pair" must be None.
/// </summary>
public class SExp
{
//     public static CLVMObject True { get; } = new CLVMObject { Atom = new byte[] { 0x01 } };
//     public static CLVMObject False { get; } = new CLVMObject();
//
//     public static CLVMObject NULL { get; } = new CLVMObject();
//

    public byte[]? Atom { get; set; }
    public Tuple<dynamic, dynamic>? Pair { get; set; }

    public SExp(CLVMObject obj)
    {
        Atom = obj.Atom;
        Pair = obj.Pair;
    }
    
    public Tuple<SExp, SExp>? AsPair()
    {
        var pair = this.Pair;
        if (pair == null)
        {
            return null;
        }
        return Tuple.Create(new SExp(pair?.Item1), new SExp(pair?.Item2));
    }

    public byte[]? AsAtom()
    {
        return Atom;
    }
    
    public SExp Cons(SExp right)
    {
        return To(new Tuple<dynamic, dynamic>(this, right));
    }

    public SExp First()
    {
        if (Pair != null)
        {
            return Pair.Item1;
        }
        throw new EvalError("first of non-cons", this);
    }

    public SExp Rest()
    {
        if (Pair != null)
        {
            return Pair.Item2;
        }
        throw new EvalError("rest of non-cons", this);
    }

    public static SExp To(dynamic v)
    {
        if (v is SExp se)
        {
            return se;
        }

        if (LooksLikeCLVMObject(v))
        {
            return new SExp(v);
        }

        return new SExp(HelperFunctions.ToSexpType(v));
    }
    
    public IEnumerable<SExp> AsIter()
    {
        var v = this;
        while (!v.Nullp())
        {
            yield return v.First();
            v = v.Rest();
        }
    }
    
    public bool Nullp()
    {
        byte[]? v = Atom;
        return v is { Length: 0 };
    }

    public override string ToString()
    {
        return BitConverter.ToString(AsBin()).Replace("-", "");
    }
    

    public static bool LooksLikeCLVMObject(object obj)
    {
        var type = obj.GetType();
        var properties = type.GetProperties().Select(p => p.Name).ToList();
        return properties.Contains("Atom") && properties.Contains("Pair");
    }

    public static byte[] ConvertAtomToBytes(object v)
    {
        if (v is byte[] byteArray)
        {
            return byteArray;
        }
        else if (v is string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
        else if (v is int intValue)
        {
            return BitConverter.GetBytes(intValue);
        }
        else if (v is null)
        {
            return new byte[0];
        }
        else if (v is List<object> list)
        {
            var result = new List<byte>();
            foreach (var item in list)
            {
                result.AddRange(ConvertAtomToBytes(item));
            }

            return result.ToArray();
        }

        throw new ArgumentException($"Can't cast {v.GetType()} ({v}) to bytes");
    }
}
