using System.Numerics;
using CLVMDotNet.CLVM;

namespace CLVMDotNet.Tools.Stages;

public class Stage0
{
    public static Tuple<BigInteger, SExp> RunProgram(SExp program, dynamic args, BigInteger? max_cost)
    {
        //method eval op
        //method traversePath
        //method swap op
        //method apply op

        BigInteger cost = 0;
        
        //while opstack
        //pop opstack
        //cost += stackFunction
        if (max_cost.HasValue && cost > max_cost)
            throw new EvalError("cost exceeded", SExp.To(max_cost));

        return Tuple.Create(cost, SExp.To(1));
    }
}