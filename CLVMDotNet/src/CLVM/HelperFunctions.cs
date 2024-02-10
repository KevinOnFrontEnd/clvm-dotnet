using System.Data.SqlTypes;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace CLVMDotNet.CLVM
{
    public static class HelperFunctions
    {
        private static byte[] nullBytes = new byte[0];
        
        public static string PrintLeaves(SExp tree)
        {
            var a = tree.AsAtom();
            if (a != null)
            {
                if (a.Length == 0)
                    return "() ";

                return $"{a[0]} ";
            }

            var ret = "";
            var pairs = tree.AsPair();
            var list = new List<SExp>() { pairs.Item1, pairs.Item2 };
            if (pairs != null)
            {
                foreach (SExp i in list)
                {
                    ret += PrintLeaves(i);
                }
            }

            return ret;
        }

        public static string PrintTree(SExp tree)
        {
            var a = tree.AsAtom();
            if (a != null)
            {
                if (a.Length == 0)
                {
                    return "() ";
                }

                return $"{a[0]} ";
            }

            var ret = "(";
            var pairs = tree.AsPair();
            var list = new List<SExp>() { pairs!.Item1, pairs.Item2 };
            if (pairs != null)
            {
                foreach (var i in list)
                {
                    ret += PrintTree(i);
                }
            }

            ret += ")";
            return ret;
        }

        public static void ValidateSExp(SExp sexp)
        {
            Stack<SExp> validateStack = new Stack<SExp>();
            validateStack.Push(sexp);

            while (validateStack.Count > 0)
            {
                dynamic v = validateStack.Pop();

                if (!(v is SExp))
                {
                    throw new InvalidOperationException("v is not an instance of SExp");
                }

                if (v.Pair != null)
                {
                    if (v.Pair.GetType() != typeof(Tuple<object, object>))
                    {
                        throw new InvalidOperationException("v.pair is not a Tuple");
                    }

                    Tuple<dynamic, dynamic> pair = v.Pair;

                    if (!HelperFunctions.LooksLikeCLVMObject(pair.Item1) ||
                        !HelperFunctions.LooksLikeCLVMObject(pair.Item2))
                    {
                        throw new InvalidOperationException("One or both elements do not look like CLVM objects");
                    }

                    var sPair = v.AsPair();
                    validateStack.Push(sPair.Item1);
                    validateStack.Push(sPair.Item2);
                }
                else
                {
                    if (!(v.Atom is byte[]))
                    {
                        throw new InvalidOperationException("v.atom is not a byte array");
                    }
                }
            }
        }

        private static bool IsTuple(dynamic? obj)
        {
            var isTuple = false;
            Type type = obj?.GetType();


            if (type != null)
            {
                if (!isTuple)
                    isTuple = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Tuple<,>);

                if (!isTuple)
                    isTuple = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTuple<,>);
            }

            return isTuple;
        }

        public static dynamic? GetLeftIfTuple(dynamic t)
        {
            if (IsTuple(t))
            {
                if (t.GetType().GetGenericTypeDefinition() == typeof(ValueTuple<,>))
                {
                    FieldInfo item1Field = t.GetType().GetField("Item1");
                    if (item1Field != null)
                    {
                        return item1Field.GetValue(t);
                    }
                }
                else
                {
                    PropertyInfo item1Property = t.GetType().GetProperty("Item1");
                    if (item1Property != null)
                    {
                        return item1Property.GetValue(t);
                    }
                }
            }

            return null;
        }

        public static dynamic? GetRightIfTuple(dynamic t)
        {
            if (IsTuple(t))
            {
                if (t.GetType().GetGenericTypeDefinition() == typeof(ValueTuple<,>))
                {
                    FieldInfo item1Field = t.GetType().GetField("Item2");
                    if (item1Field != null)
                    {
                        return item1Field.GetValue(t);
                    }
                }
                else
                {
                    PropertyInfo item1Property = t.GetType().GetProperty("Item2");
                    if (item1Property != null)
                    {
                        return item1Property.GetValue(t);
                    }
                }
            }

            return null;
        }


        public static dynamic? ToSexpType(dynamic? v)
        {
            List<dynamic> stack = new List<dynamic>() { v };
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

                    dynamic? value = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);

                    if (IsTuple(value))
                    {
                        var left = GetLeftIfTuple(value);
                        var right = GetRightIfTuple(value);
                        target = stack.Count;
                        stack.Add(new CLVMObject(Tuple.Create(left, right)));

                        if (!LooksLikeCLVMObject(right))
                        {
                            stack.Add(right);
                            ops.Add((2, target)); // set right
                            ops.Add((0, -1)); // convert
                        }

                        if (!LooksLikeCLVMObject(left))
                        {
                            stack.Add(left);
                            ops.Add((1, target)); // set left
                            ops.Add((0, -1)); // convert
                        }

                        continue;
                    }

                    if (value is System.Collections.IList list &&
                        list.GetType().IsGenericType &&
                        list.GetType().GetGenericTypeDefinition() == typeof(List<>))
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
                    var leftValue = new CLVMObject(stack[stack.Count - 1]);
                    stack.RemoveAt(stack.Count - 1);
                    var rightValue = stack[target].Pair.Item2;
                    stack[target].Pair = Tuple.Create<dynamic, dynamic>(leftValue, rightValue);
                    continue;
                }

                if (op == 2) // set right
                {
                    var leftValue = stack[target].Pair.Item1;
                    var right = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
                    var rightValue = new CLVMObject(right);

                    stack[target].Pair = Tuple.Create<dynamic, dynamic>(leftValue, rightValue);
                    continue;
                }

                if (op == 3) // prepend list
                {
                    var item = stack[stack.Count - 1];
                    stack.RemoveAt(stack.Count - 1);
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

            if (v is string[] strarray && strarray.Length == 1)
            {
                return Encoding.UTF8.GetBytes(strarray[0]);
            }

            if (v is int intValue)
            {
                var s = Casts.IntToBytes(intValue);
                return s;
            }

            if (v is BigInteger bigIntValue)
            {
                var s = Casts.IntToBytes(bigIntValue);
                return s;
            }

            if (v is null)
            {
                return Array.Empty<byte>();
            }

            if (v is object[] list && list.Length == 0)
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
}