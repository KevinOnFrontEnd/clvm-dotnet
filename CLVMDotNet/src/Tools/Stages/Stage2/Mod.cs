using CLVMDotNet.CLVM;

namespace CLVMDotNet.Tools.Stages.Stage2;

public static class Mod
{
    public static byte QUOTE_ATOM => Keywords.KEYWORD_TO_ATOM["q"];
    public static byte CONS_ATOM => Keywords.KEYWORD_TO_ATOM["c"];
    public static byte[] MAIN_NAME => new byte[] { };
    
    //BuildTree (Tuple<dynamic,dynamic>)
    //BuildTreeProgram 
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