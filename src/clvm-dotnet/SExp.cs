using System.Numerics;
using System.Text;
using clvm_dotnet;


/// <summary>
//  SExp provides higher level API on top of any object implementing the CLVM
//  object protocol.
//  The tree of values is not a tree of SExp objects, it's a tree of CLVMObject
//  like objects. SExp simply wraps them to provide a uniform view of any
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
     public static CLVMObject True { get; } = new CLVMObject { Atom = new byte[] { 0x01 } };
     public static CLVMObject False { get; } = new CLVMObject();
     public static CLVMObject NULL { get; } = null;

     public byte[]? Atom { get; set; }
    public Tuple<dynamic, dynamic>? Pair { get; set; }

    public SExp(CLVMObject obj)
    {
        Atom = obj.Atom;
        Pair = obj.Pair;
    }

    public SExp()
    {
        
    }

    public Tuple<SExp, SExp>? AsPair()
    {
        if (Pair == null)
        {
            return null;
        }
        var left = Pair.Item1 is SExp ? Pair.Item1 : new SExp (Pair.Item1);
        var right = Pair.Item2 is SExp ? Pair.Item2 : new SExp(Pair.Item2);
        return Tuple.Create<SExp,SExp>(left,right);
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

    public bool Listp()
    {
        return Pair != null;
    }
    
    public int ListLength()
    {
        SExp v = this;
        int size = 0;

        while (v.Listp())
        {
            size++;
            v = v.Rest();
        }

        return size;
    }
    
    public static SExp To(dynamic v)
    {
        if (v is SExp se)
        {
            Console.WriteLine("looks like sexp type to()");
            return se;
        }

        if (HelperFunctions.LooksLikeCLVMObject(v))
        {
            Console.WriteLine("clvm object");
            return new SExp(v);
        }

        Console.WriteLine("looks like sexp type to()");
        return new SExp(HelperFunctions.ToSexpType(v));
    }
    
    public BigInteger AsInt()
    {
        if (Atom != null)
        {
            return Casts.IntFromBytes(Atom!);
        }
        else
        {
            // Handle the case where atom is null (or empty)
            throw new InvalidOperationException("The atom property is null or empty.");
        }
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

    public byte[] AsBin()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            Serialize.SexpToStream(this, stream);
            return stream.ToArray();
        }
    }

    public override string ToString()
    {
        return BitConverter.ToString(AsBin()).Replace("-", "");
    }

    public SExp ToSexpType(dynamic v)
    {
        Stack<dynamic> stack = new Stack<dynamic>();
        var stackList = stack.ToArray();
        Stack<Tuple<int, int>> ops = new Stack<Tuple<int, int>>();
        ops.Push(new Tuple<int, int>(0, -1)); // convert
        int opIteration = 0;
        while (ops.Count > 0)
        {
            opIteration += 1;
            Console.WriteLine($"op iteration: {opIteration}");
            Tuple<int, int> opTarget = ops.Pop();
            int op = opTarget.Item1;
            int target = opTarget.Item2;

            // Convert value
            if (op == 0)
            {
                Console.Write("op0");
                if (HelperFunctions.LooksLikeCLVMObject(stack.Peek()))
                {
                    continue;
                }

                dynamic value = stack.Pop();

                if (value is Tuple<dynamic, dynamic> tupleValue)
                {
                    object left = tupleValue.Item1;
                    object right = tupleValue.Item2;

                    if (left == null || right == null)
                    {
                        throw new InvalidOperationException("Tuple elements cannot be null.");
                    }

                    if (left is dynamic && right is dynamic)
                    {
                        Tuple<dynamic, dynamic> tuple = Tuple.Create((dynamic)left, (dynamic)right);
                        target = stack.Count;
                        stack.Push(tuple);

                        if (!HelperFunctions.LooksLikeCLVMObject((dynamic)right))
                        {
                            stack.Push((dynamic)right);
                            ops.Push(Tuple.Create(2, target)); // set right
                            ops.Push(Tuple.Create(0, -1)); // convert
                        }

                        if (!HelperFunctions.LooksLikeCLVMObject((dynamic)left))
                        {
                            stack.Push((dynamic)left);
                            ops.Push(Tuple.Create(1, target)); // set left
                            ops.Push(Tuple.Create(0, -1)); // convert
                        }

                        continue;
                    }
                }
                else if (value is List<dynamic> listValue)
                {
                    target = stack.Count;
                    stack.Push(new CLVMObject(NULL));

                    foreach (var item in listValue)
                    {
                        stack.Push(item);
                        ops.Push(Tuple.Create(3, target)); // prepend list

                        // We only need to convert if it's not already the right type
                        if (!HelperFunctions.LooksLikeCLVMObject(item))
                        {
                            ops.Push(Tuple.Create(0, -1)); // convert
                        }
                    }

                    continue;
                }

                stack.Push(new CLVMObject(ConvertAtomToBytes(value)));
                continue;
            }

            if (op == 1) // set left
            {
                
                if (stackList[target] is CLVMObject clvmObject)
                {
                    clvmObject.Pair = new (stack.Pop(), clvmObject.Pair.Item2);
                }

                continue;
            }

            if (op == 2) // set right
            {
                if (stackList[target] is CLVMObject clvmObject)
                {

                    clvmObject.Pair = new(clvmObject.Pair.Item1, new CLVMObject(stack.Pop()));
                }

                continue;
            }

            if (op == 3) // prepend list
            {
                if (stackList[target] is CLVMObject clvmObject)
                {
                    clvmObject.Pair = new (stack.Pop(), clvmObject.Pair);
                }

                continue;
            }
        }

        // There's exactly one item left at this point
        if (stack.Count != 1)
        {
            throw new InvalidOperationException("Internal error.");
        }

        // stack[0] implements the CLVM object protocol and can be wrapped by an SExp
        return new SExp(stack.Pop());
    }

    public static byte[] ConvertAtomToBytes(object v)
    {
        if (v is byte[] byteArray)
        {
            return byteArray;
        }
        else if (v is string str)
        {
            Console.WriteLine("is string");
            return Encoding.UTF8.GetBytes(str);
        }
        else if (v is int intValue)
        {
            Console.WriteLine("is int");
            return BitConverter.GetBytes(intValue);
        }
        else if (v is null)
        {
            Console.WriteLine("is null");
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