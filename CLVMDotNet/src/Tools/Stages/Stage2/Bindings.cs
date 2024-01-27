using CLVMDotNet.CLVM;
using CLVMDotNet.Tools.IR;

namespace CLVMDotNet.Tools.Stages.Stage2;

public static class Bindings
{
    public static SExp Brun => BinUtils.Assemble("(a 2 3)");
    
    public static SExp Run => BinUtils.Assemble("(a (opt (com 2)) 3)");
}