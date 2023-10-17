using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace clvm_dotnet;

public static class HelperFunctions
{
    public static readonly byte[] NULL = Array.Empty<byte>();
    
    public static dynamic ToSexpType(dynamic v)
    {
        var stack = new Stack<dynamic>();
        var ops = new Stack<(int, int?)>();
        ops.Push((0, null));  // convert

        while (ops.Count > 0)
        {
            var (op, target) = ops.Pop();

            // Convert value
            if (op == 0)
            {
                if (LooksLikeCLVMObject(stack.Peek()))
                {
                    continue;
                }

                var value = stack.Pop();

                if (value is Tuple<object, object> tuple)
                {
                    if (tuple.Item1 != null && tuple.Item2 != null)
                    {
                        target = stack.Count;
                        stack.Push(new CLVMObject(new Tuple<CLVMObject, CLVMObject>(
                            new CLVMObject(tuple.Item1), new CLVMObject(tuple.Item2))));

                        if (!LooksLikeCLVMObject(tuple.Item2))
                        {
                            stack.Push(tuple.Item2);
                            ops.Push((2, target));  // set right
                            ops.Push((0, null));    // convert
                        }

                        if (!LooksLikeCLVMObject(tuple.Item1))
                        {
                            stack.Push(tuple.Item1);
                            ops.Push((1, target));  // set left
                            ops.Push((0, null));    // convert
                        }

                        continue;
                    }
                }

                if (value is List<object> list)
                {
                    target = stack.Count;
                    stack.Push(new CLVMObject(NULL));

                    foreach (var item in list)
                    {
                        stack.Push(item);
                        ops.Push((3, target));  // prepend list

                        // We only need to convert if it's not already the right type
                        if (!LooksLikeCLVMObject(item))
                        {
                            ops.Push((0, null));  // convert
                        }
                    }

                    continue;
                }

                stack.Push(new CLVMObject(ConvertAtomToBytes(value)));
                continue;
            }

            if (op == 1)  // set left
            {
                var left = new CLVMObject(stack.Pop());
                stack.Push(new CLVMObject(new Tuple<CLVMObject, CLVMObject>(
                    left, ((Tuple<CLVMObject, CLVMObject>)stack.Peek().Value).Item2)));
                continue;
            }

            if (op == 2)  // set right
            {
                var right = new CLVMObject(stack.Pop());
                stack.Push(new CLVMObject(new Tuple<CLVMObject, CLVMObject>(
                    ((Tuple<CLVMObject, CLVMObject>)stack.Peek().Value).Item1, right)));
                continue;
            }

            if (op == 3)  // prepend list
            {
                stack.Push(new CLVMObject(new Tuple<CLVMObject, CLVMObject>(
                    new CLVMObject(stack.Pop()), ((Tuple<CLVMObject, CLVMObject>)stack.Peek().Value).Item2)));
                continue;
            }
        }

        // There's exactly one item left at this point
        if (stack.Count != 1)
        {
            throw new ArgumentException("Internal error");
        }

        // stack[0] implements the CLVM object protocol and can be wrapped by a SExp
        return stack.Pop();
    }
    
    
    public static byte[] ConvertAtomToBytes(object v)
    {
        if (v is byte[] bytes)
        {
            return bytes;
        }
        if (v is string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
        if (v is int intValue)
        {
            return BitConverter.GetBytes(intValue);
        }
        if (v is null)
        {
            return Array.Empty<byte>();
        }
        if (v is IList list && list.Count == 0)
        {
            return Array.Empty<byte>();
        }
        if (v is IConvertible convertible)
        {
            byte byteValue = convertible.ToByte(System.Globalization.CultureInfo.InvariantCulture);
            return new byte[] { byteValue };
        }
        throw new ArgumentException($"Can't cast {v.GetType()} ({v}) to bytes");
    }
    
    public static bool LooksLikeCLVMObject(object o)
    {
        Type type = o.GetType();
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        bool hasAtom = false;
        bool hasPair = false;

        foreach (PropertyInfo property in properties)
        {
            if (property.Name == "atom")
            {
                hasAtom = true;
            }
            else if (property.Name == "pair")
            {
                hasPair = true;
            }

            if (hasAtom && hasPair)
            {
                return true;
            }
        }
        return false;
    }
}