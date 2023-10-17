using System.Reflection;

namespace clvm_dotnet;

public static class Op_Utils
{
    public static Dictionary<string, Func<object, object>> OperatorsForDict(
        Dictionary<string, dynamic> keywordToAtom, 
        Dictionary<string, Func<object, object>> opDict,
        Dictionary<string, string> opNameLookup = null)
    {
        Dictionary<string, Func<dynamic, dynamic>> result = new Dictionary<string, Func<dynamic, dynamic>>();

        foreach (string op in keywordToAtom.Keys)
        {
            string opName = opNameLookup != null && opNameLookup.ContainsKey(op)
                ? opNameLookup[op]
                : "op_" + op;

            if (opDict.TryGetValue(opName, out Func<object, object> opFunc))
            {
                result[keywordToAtom[op]] = opFunc;
            }
        }

        return result;
    }

    public static Dictionary<string, Func<object, object>> OperatorsForModule(
        Dictionary<string, object> keywordToAtom, 
        object mod, 
        Dictionary<string, string> opNameLookup = null)
    {
        Dictionary<string, Func<object, object>> modDict = new Dictionary<string, Func<object, object>>();

        // Get all public static methods from the module
        foreach (var methodInfo in mod.GetType().GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            modDict[methodInfo.Name] = (Func<object, object>)Delegate.CreateDelegate(
                typeof(Func<object, object>), 
                methodInfo);
        }

        return OperatorsForDict(keywordToAtom, modDict, opNameLookup);
    }
}