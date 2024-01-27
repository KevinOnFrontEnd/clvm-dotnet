using CLVMDotNet.CLVM;

namespace CLVMDotNet.Tools.Stages.Stage2;

public static class Compile
{
    public static byte QUOTE_ATOM => Keywords.KEYWORD_TO_ATOM["q"];
    public static byte APPLY_ATOM => Keywords.KEYWORD_TO_ATOM["a"];
    public static byte CONS_ATOM => Keywords.KEYWORD_TO_ATOM["c"];
}