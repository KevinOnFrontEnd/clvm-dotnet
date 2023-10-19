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
public class Sexp
{
//     public static CLVMObject True { get; } = new CLVMObject { Atom = new byte[] { 0x01 } };
//     public static CLVMObject False { get; } = new CLVMObject();
//
//     public static CLVMObject NULL { get; } = new CLVMObject();
//
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
