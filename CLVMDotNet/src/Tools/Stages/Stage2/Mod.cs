using CLVMDotNet.CLVM;

namespace CLVMDotNet.Tools.Stages.Stage2;

public static class Mod
{
    public static byte[] QUOTE_ATOM => CLVM.Operators.KEYWORD_TO_ATOM()["q"];
    public static byte[] CONS_ATOM => CLVM.Operators.KEYWORD_TO_ATOM()["c"];
    public static byte[] MAIN_NAME => new byte[] { };
    
    /// <summary>
    /// This function takes a Python list of items and turns it into a binary tree
    /// of the items, suitable for casting to an s-expression.
    /// </summary>
    /// <returns></returns>
    public static dynamic BuildTree(List<dynamic> items)
    {
        var size = items.Count;
        if (size == 0)
            return new List<dynamic>();
        if (size == 1)
            return items[0];
        
        int halfSize = size / 2;
        dynamic left = BuildTree(items.GetRange(0, halfSize));
        dynamic right = BuildTree(items.GetRange(halfSize, size - halfSize));
        return new Tuple<dynamic,dynamic>(left, right);
    }
    
    /// <summary>
    /// his function takes a Python list of items and turns it into a program that
    /// builds a binary tree of the items, suitable for casting to an s-expression.
    /// </summary>
    /// <returns></returns>
    public static List<dynamic> BuildTreeProgram(List<dynamic> items)
    {
        var size = items.Count;
        if (size == 0)
            return new List<dynamic>();
        if (size == 1)
            return items[0];
        
        int halfSize = size / 2;
        dynamic left = BuildTreeProgram(items.GetRange(0, halfSize));
        dynamic right = BuildTreeProgram(items.GetRange(halfSize, size - halfSize));
        return new List<dynamic> { CONS_ATOM, left, right };
    }

    public static List<dynamic> Flatten(SExp sexp)
    {
        if (sexp.Listp())
        {
            List<dynamic> result = new List<dynamic>();
            result.AddRange(Flatten(sexp.First()));
            result.AddRange(Flatten(sexp.Rest()));
            return result;
        }
        return new List<dynamic> { sexp.AsAtom() };
    }
    
    
    //Flattern
    //BuildUsedConstantNames
    //ParseInclude
    //UnquoteArgs
    //DefunInlineToMacro
    //ParseModSexp
    //CompileModStage1
    //SymbolTableToTree
    //BuildMacroLookupTable
    //CompileFunctions
    //CompileMod
    
    
}