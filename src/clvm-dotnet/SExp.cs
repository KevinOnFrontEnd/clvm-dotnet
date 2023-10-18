namespace clvm_dotnet;

using System;
using System.Collections.Generic;

public class SExp
{
    // Constants for true, false, and null
    public SExp True = new SExp(new CLVMObject(new byte[] { 1 }));
    public  SExp False = SExp.__null__ = new SExp(new CLVMObject(new byte[] { }));
    private  SExp __null__;

    // The underlying object implementing the CLVM object protocol
    public byte[] atom { get; private set; }

    // This is a tuple of the underlying CLVMObject-like objects.
    public Tuple<object?, object?>? pair { get; private set; }

    public SExp(CLVMObject obj)
    {
        atom = obj.atom;
        pair = obj.pair;
    }

    // This returns a tuple of two SExp objects or null
    public Tuple<SExp, SExp> as_pair()
    {
        if (pair == null)
        {
            return null;
        }
        return new Tuple<SExp, SExp>(new SExp((CLVMObject)pair.Item1), new SExp((CLVMObject)pair.Item2));
    }

    // Deprecated, same as the atom property
    public byte[] as_atom()
    {
        return atom;
    }

    public bool listp()
    {
        return pair != null;
    }

    public bool nullp()
    {
        return atom != null && atom.Length == 0;
    }

    public int as_int()
    {
        return BitConverter.ToInt32(atom.Reverse().ToArray(), 0);
    }

    public byte[] as_bin()
    {
        MemoryStream stream = new MemoryStream();
        serialize.SExpToStream(this, stream);
        return stream.ToArray();
    }

    public static SExp to(object v)
    {
        if (v is SExp)
        {
            return (SExp)v;
        }

        if (HelperFunctions.LooksLikeCLVMObject(v))
        {
            return new SExp((CLVMObject)v);
        }

        // This will lazily convert elements
        return new SExp(HelperFunctions.ToSexpType(v));
    }

    public SExp cons(SExp right)
    {
        return to(new Tuple<SExp, SExp>(this, right));
    }

    public SExp First()
    {
        if (pair != null)
        {
            return new SExp((CLVMObject)pair.Item1);
        }
        throw new EvalError("first of non-cons", this);
    }

    public SExp Rest()
    {
        if (pair != null)
        {
            return new SExp((CLVMObject)pair.Item2);
        }
        throw new EvalError("rest of non-cons", this);
    }

    public static SExp _null()
    {
        return __null__;
    }

    public IEnumerable<SExp> as_iter()
    {
        SExp v = this;
        while (!v.nullp())
        {
            yield return v.First();
            v = v.Rest();
        }
    }

    public bool Equals(object other)
    {
        try
        {
            other = to(other);
            Stack<Tuple<SExp, SExp>> to_compare_stack = new Stack<Tuple<SExp, SExp>>();
            to_compare_stack.Push(new Tuple<SExp, SExp>(this, (SExp)other));
            while (to_compare_stack.Count > 0)
            {
                var tuple = to_compare_stack.Pop();
                SExp s1 = tuple.Item1;
                SExp s2 = tuple.Item2;
                Tuple<SExp, SExp> p1 = s1.as_pair();
                if (p1 != null)
                {
                    Tuple<SExp, SExp> p2 = s2.as_pair();
                    if (p2 != null)
                    {
                        to_compare_stack.Push(new Tuple<SExp, SExp>(p1.Item1, p2.Item1));
                        to_compare_stack.Push(new Tuple<SExp, SExp>(p1.Item2, p2.Item2));
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (s2.as_pair() != null || !atom.SequenceEqual(s2.atom))
                {
                    return false;
                }
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public int list_len()
    {
        SExp v = this;
        int size = 0;
        while (v.listp())
        {
            size++;
            v = v.Rest();
        }
        return size;
    }

    public object as_python(dynamic sexp)
    {
        return as_python(this);
    }

    public override string ToString()
    {
        return $"{this.GetType().Name}({this})";
    }
}