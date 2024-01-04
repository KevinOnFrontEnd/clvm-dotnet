using System.Reflection;
using System.Text;

namespace clvm_dotnet;

public static class HelperFunctions
{
    private static byte[] nullBytes = new byte[0];

    public static dynamic? ToSexpType(dynamic? v)
    {
        List<dynamic> stack = new List<dynamic>(){v};
        List<(int op, int target)> ops = new List<(int op, int target)> { (0, -1) }; // convert
        int opIteration = 0;
        
        while (ops.Count > 0)
        {
            opIteration += 1;
            
            var (op, target) = ops[ops.Count - 1];
            ops.RemoveAt(ops.Count - 1);
            
            // Convert value
            if (op == 0)
            {
                if (LooksLikeCLVMObject(stack[stack.Count - 1]))
                {
                    continue;
                }

                dynamic value = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);

                if (value is Tuple<object, object> tuple)
                {
                    if (tuple.Item2 != null)
                    {
                        stack.Add(tuple.Item2);
                        ops.Add((2, target)); // set right
                        ops.Add((0, -1)); // convert
                    }

                    if (tuple.Item1 != null)
                    {
                        stack.Add(tuple.Item1);
                        ops.Add((1, target)); // set left
                        ops.Add((0, -1)); // convert
                    }
                    continue;
                }
                
                if (value != null && value.GetType().IsArray)
                {
                    target = stack.Count;
                    stack.Add(new CLVMObject(nullBytes));
                    int iteration = 0;
                    foreach (object item in value)
                    {
                        iteration += 1;
                        stack.Add(item);
                        ops.Add((3, target)); // prepend list
                        // We only need to convert if it's not already the right type
                        if (!LooksLikeCLVMObject(item))
                        {
                            ops.Add((0, -1)); // convert
                        }
                    }
                    continue;
                }
                var atomBytes = ConvertAtomToBytes(value);
                stack.Add(new CLVMObject(atomBytes));
                continue;
            }

            if (op == 1) // set left
            {
                var leftValue = new CLVMObject(stack[stack.Count-1]);
                stack.RemoveAt(stack.Count-1);
                var currentPair = ((Tuple<CLVMObject, CLVMObject>)stack[target]);
                stack[target] = new Tuple<CLVMObject, CLVMObject>(leftValue, currentPair.Item2);
                continue;
            }

            if (op == 2) // set right
            {
                var rightValue = new CLVMObject(stack[stack.Count-1]);
                stack.RemoveAt(stack.Count-1);
                var currentPair = ((Tuple<CLVMObject, CLVMObject>)stack[target]);
                stack[target] = new Tuple<CLVMObject, CLVMObject>(currentPair.Item1, rightValue);
                continue;
            }

            if (op == 3) // prepend list
            {
                var item = stack[stack.Count-1];
                stack.RemoveAt(stack.Count-1);
                var newValue = new Tuple<dynamic, dynamic>(
                    item,
                    stack[target]
                );
                stack[target] = new CLVMObject(newValue);
                continue;
            }
        }

        // There's exactly one item left at this point
        if (stack.Count != 1)
        {
            throw new ArgumentException("Internal error");
        }

        // stack[0] implements the CLVM object protocol and can be wrapped by an SExp
        return stack[0];
    }

    public static byte[] ConvertAtomToBytes(dynamic? v)
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
            var s = Casts.IntToBytes(intValue);
            return s;
        }

        if (v is null)
        {
            return Array.Empty<byte>();
        }

        if (v is object [] list &&  list.Length == 0)
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

    public static bool LooksLikeCLVMObject(object? o)
    {
        if (o != null)
        {
            Type type = o.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            bool hasAtom = false;
            bool hasPair = false;

            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "Atom")
                {
                    hasAtom = true;
                }
                if (property.Name == "Pair")
                {
                    hasPair = true;
                }

                if (hasAtom && hasPair)
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    public static byte MSBMask(byte inputByte)
    {
        inputByte |= (byte)(inputByte >> 1);
        inputByte |= (byte)(inputByte >> 2);
        inputByte |= (byte)(inputByte >> 4);
        return (byte)((inputByte + 1) >> 1);
    }
}