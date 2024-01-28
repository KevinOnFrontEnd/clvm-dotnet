using CLVMDotNet.CLVM;

namespace CLVMDotNet.Tools.Stages.Stage2;

public static class Helpers
{
    public static byte QUOTE_ATOM => Keywords.KEYWORD_TO_ATOM["q"];
    public static byte APPLY_ATOM => Keywords.KEYWORD_TO_ATOM["a"];

    public static Tuple<byte, SExp> Quote(SExp sexp)
    {
        return Tuple.Create(QUOTE_ATOM, sexp);
    }
    
    //Run
    //Brun
}