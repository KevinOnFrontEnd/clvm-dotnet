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
    public static SExp CONS_PATTERN = BinUtils.Assemble("(c (: . first) (: . rest)))");
    public static SExp VAR_CHANGE_OPTIMIZER_CONS_EVAL_PATTERN = BinUtils.Assemble("(a (q . (: . sexp)) (: . args))");
    public static SExp CONS_OPTIMIZER_PATTERN_FIRST = BinUtils.Assemble("(f (c (: . first) (: . rest)))");
    public static SExp CONS_OPTIMIZER_PATTERN_REST = BinUtils.Assemble("(r (c (: . first) (: . rest)))");
    public static SExp FIRST_ATOM_PATTERN = BinUtils.Assemble("(f ($ . atom))");
    public static SExp REST_ATOM_PATTERN = BinUtils.Assemble("(r ($ . atom))");
    public static SExp QUOTE_PATTERN_1 = BinUtils.Assemble("(q . 0)");
    public static SExp APPLY_NULL_PATTERN_1 = BinUtils.Assemble("(a 0 . (: . rest))");
    
    
    public static bool NonNil(SExp sexp)
    {
        return sexp.Listp() || (sexp.AsAtom().Length > 0);
    }

    public static bool IsArgsCall(SExp r)
    {
        if (!r.Listp() && r.AsInt() == 1)
            return true;
        return false;
    }
    
    public static bool SeemsConstant(SExp sexp)
    {
        if (!sexp.Listp())
        {
            // note that `0` is a constant
            return !NonNil(sexp);
        }
        var operatorSexp = sexp.First();
        if (!operatorSexp.Listp())
        {
            //TODO: test that a byte array can be compared to a byte
            var asAtom = operatorSexp.AsAtom();
            if (asAtom.Equals(QUOTE_ATOM))
            {
                return true;
            }

            if (asAtom.Equals(RAISE_ATOM))
            {
                return false;
            }
        }
        else if (!SeemsConstant(operatorSexp))
        {
            return false;
        }
        return sexp.Rest().AsIter().All(childSexp => SeemsConstant(childSexp));
    }
    
    
    //DoRead
    //DoWrite
    //RunProgramForSearchPaths
    //SeemsConstant
    //ConstantOptimizer
    //IsArgsCall
    //ConsF
    //ConsR
    //PathFromArgs
    //SubArgs
    //var_change_optimizer_cons_eval
    //children_optimizer
    //cons_optimizer
    //path_optimizer
    //def quote_null_optimizer(r, eval)
    //apply_null_optimizer
    //optimize_sexp
    //make_do_opt
}