using CLVMDotNet.CLVM;

namespace CLVMDotNet.Tools.Stages.Stage2;

public static class Helpers
{
    public static byte[] QUOTE_ATOM => CLVM.Operators.KEYWORD_TO_ATOM()["q"];
    public static byte[] APPLY_ATOM => CLVM.Operators.KEYWORD_TO_ATOM()["a"];

    public static Tuple<byte[], SExp> Quote(SExp sexp)
    {
        return Tuple.Create(QUOTE_ATOM, sexp);
    }

    public static SExp Eval(SExp prog, dynamic args)
    {
        var eval = SExp.To(new List<dynamic>() { APPLY_ATOM, prog, args });
        return eval;
    }
    
    public static SExp Run(SExp prog, SExp macroLookup)
    {
        var args = NodePath.TOP.AsShortPath();
        var mac = Quote(macroLookup);
        return Helpers.Eval(SExp.To(new List<dynamic> { "com", prog, mac }), args);
    }
    
    public static SExp Brun(SExp prog, SExp args)
    {
        return Helpers.Eval(SExp.To(Quote(prog)), Quote(args));
    }
}