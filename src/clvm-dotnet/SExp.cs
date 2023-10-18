
using System.Text;
using clvm_dotnet;

public class Sexp
{
    public static CLVMObject True { get; } = new CLVMObject { Atom = new byte[] { 0x01 } };
    public static CLVMObject False { get; } = new CLVMObject();

    public static CLVMObject NULL { get; } = new CLVMObject();

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

    public static CLVMObject ToSexpType(dynamic v)
    {
        var stack = new List<dynamic>();
        var ops = new List<Tuple<int, int>>(); // Tuple of (op, target)

        stack.Add(v);
        ops.Add(new Tuple<int, int>(0, -1)); // Initial operation to convert

        while (ops.Count > 0)
        {
            var (op, target) = ops.Last();
            ops.RemoveAt(ops.Count - 1);

            if (op == 0) // Convert value
            {
                if (Sexp.LooksLikeCLVMObject(stack.Last()))
                {
                    continue;
                }

                var value = stack.Last();
                stack.RemoveAt(stack.Count - 1);

                if (value is Tuple<object, object> tuple)
                {
                    if (tuple.Item2 is not null && tuple.Item1 is not null)
                    {
                        var newSexp = new CLVMObject
                        {
                            Pair = new Tuple<CLVMObject, CLVMObject>(
                                new CLVMObject { Atom = ConvertAtomToBytes(tuple.Item1) },
                                new CLVMObject { Atom = ConvertAtomToBytes(tuple.Item2) }
                            )
                        };

                        target = stack.Count;
                        stack.Add(newSexp);

                        if (!LooksLikeCLVMObject(tuple.Item2))
                        {
                            stack.Add(tuple.Item2);
                            ops.Add(new Tuple<int, int>(2, target)); // Set right
                            ops.Add(new Tuple<int, int>(0, -1));    // Convert
                        }

                        if (!LooksLikeCLVMObject(tuple.Item1))
                        {
                            stack.Add(tuple.Item1);
                            ops.Add(new Tuple<int, int>(1, target)); // Set left
                            ops.Add(new Tuple<int, int>(0, -1));    // Convert
                        }

                        continue;
                    }
                }

                if (value is List<object> list)
                {
                    target = stack.Count;
                    stack.Add(new CLVMObject { Atom = new byte[0] });

                    foreach (var item in list)
                    {
                        stack.Add(item);
                        ops.Add(new Tuple<int, int>(3, target)); // Prepend list

                        if (!LooksLikeCLVMObject(item))
                        {
                            ops.Add(new Tuple<int, int>(0, -1)); // Convert
                        }
                    }

                    continue;
                }

                stack.Add(new CLVMObject() { Atom = ConvertAtomToBytes(value) });
                continue;
            }

            if (op == 1) // Set left
            {
                stack[target].Pair = new Tuple<CLVMObject, CLVMObject>(new CLVMObject { Atom = ConvertAtomToBytes(stack.Last()) }, stack[target].Pair.Item2);
                continue;
            }

            if (op == 2) // Set right
            {
                stack[target].Pair = new Tuple<CLVMObject, CLVMObject>(stack[target].Pair.Item1, new CLVMObject { Atom = ConvertAtomToBytes(stack.Last()) });
                continue;
            }

            if (op == 3) // Prepend list
            {
                stack[target] = new CLVMObject { Pair = new Tuple<CLVMObject, CLVMObject>(new CLVMObject { Atom = ConvertAtomToBytes(stack.Last()) }, stack[target]) };
                continue;
            }
        }

        // There's exactly one item left at this point
        if (stack.Count != 1)
        {
            throw new ArgumentException("Internal error");
        }

        // stack[0] implements the CLVM object protocol and can be wrapped by an SExp
        return stack[0] as CLVMObject;
    }
}

public class SExp
{
    public byte[] Atom { get; set; }
    public Tuple<SExp, SExp> Pair { get; set; }

    public SExp(object obj)
    {
        var clvmObject = obj as CLVMObject;
        if (clvmObject != null)
        {
            Atom = clvmObject.Atom;
            Pair = clvmObject.Pair != null ? new Tuple<SExp, SExp>(new SExp(clvmObject.Pair.Item1), new SExp(clvmObject.Pair.Item2)) : null;
        }
    }

    public Tuple<SExp, SExp> AsPair()
    {
        if (Pair != null)
        {
            return new Tuple<SExp, SExp>(new SExp(Pair.Item1), new SExp(Pair.Item2));
        }
        return null;
    }

    public byte[] AsAtom()
    {
        return Atom;
    }

    public bool Listp()
    {
        return Pair != null;
    }

    public bool Nullp()
    {
        return Atom != null && Atom.Length == 0;
    }

    public int AsInt()
    {
        return BitConverter.ToInt32(Atom, 0);
    }

    public byte[] AsBin()
    {
        using (var stream = new MemoryStream())
        {
            Serialize.SExpFromStream(stream, null);
            return stream.ToArray();
        }
    }

    public static SExp To(object v)
    {
        var vSexp = v as SExp;
        if (vSexp != null)
        {
            return vSexp;
        }

        if (Sexp.LooksLikeCLVMObject(v))
        {
            return new SExp(v);
        }

        return new SExp(HelperFunctions.ToSexpType(v));
    }

    public SExp Cons(SExp right)
    {
        return To(new Tuple<SExp, SExp>(this, right));
    }

    public SExp First()
    {
        if (Pair != null)
        {
            return new SExp(Pair.Item1);
        }
        throw new EvalError("First of non-cons", this);
    }

    public SExp Rest()
    {
        if (Pair != null)
        {
            return new SExp(Pair.Item2);
        }
        throw new EvalError("Rest of non-cons", this);
    }

    public static SExp Null()
    {
        return null;
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

    public override bool Equals(object other)
    {
        try
        {
            var otherSexp = To(other);
            var toCompareStack = new Stack<Tuple<SExp, SExp>>();
            toCompareStack.Push(new Tuple<SExp, SExp>(this, otherSexp));

            while (toCompareStack.Count > 0)
            {
                var (s1, s2) = toCompareStack.Pop();
                var p1 = s1.AsPair();
                if (p1 != null)
                {
                    var p2 = s2.AsPair();
                    if (p2 != null)
                    {
                        toCompareStack.Push(new Tuple<SExp, SExp>(p1.Item1, p2.Item1));
                        toCompareStack.Push(new Tuple<SExp, SExp>(p1.Item2, p2.Item2));
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (s2.AsPair() != null || !Equals(s1.AsAtom(), s2.AsAtom()))
                {
                    return false;
                }
            }
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public int ListLen()
    {
        var v = this;
        var size = 0;
        while (v.Listp())
        {
            size++;
            v = v.Rest();
        }
        return size;
    }

    public object AsPython()
    {
        return null;
    }

    public override string ToString()
    {
        return BitConverter.ToString(AsBin()).Replace("-", "");
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }
}
