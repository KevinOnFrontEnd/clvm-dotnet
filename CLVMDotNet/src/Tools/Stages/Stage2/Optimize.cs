using CLVMDotNet.CLVM;
using CLVMDotNet.Tools.IR;

namespace CLVMDotNet.Tools.Stages.Stage2;

public static class Optimize
{
    public static byte QUOTE_ATOM => Keywords.KEYWORD_TO_ATOM["q"];
    public static byte APPLY_ATOM => Keywords.KEYWORD_TO_ATOM["a"];
    public static byte FIRST_ATOM => Keywords.KEYWORD_TO_ATOM["f"];
    public static byte REST_ATOM => Keywords.KEYWORD_TO_ATOM["r"];
    public static byte CONS_ATOM => Keywords.KEYWORD_TO_ATOM["c"];
    public static byte RAISE_ATOM => Keywords.KEYWORD_TO_ATOM["x"];
    public static int DEBUG_OPTIMIZATIONS = 0;
    public static SExp CONS_Q_A_OPTIMIZER_PATTERN => BinUtils.Assemble("(a (q . (: . sexp)) (: . args))");
    
    public static bool NonNil(SExp sexp)
    {
        return sexp.Listp() || (sexp.AsAtom().Length > 0);
    }
}