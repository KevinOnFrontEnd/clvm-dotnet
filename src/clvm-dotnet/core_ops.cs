namespace clvm_dotnet;

public class core_ops
{
public static Tuple<int, SExp> OpIf(SExp args)
    {
        if (args.list_len() != 3)
        {
            throw new EvalError("i takes exactly 3 arguments", args);
        }

        SExp r = args.Rest();
        if (args.First().nullp())
        {
            return new Tuple<int, SExp>(Costs.IF_COST, r.Rest().First());
        }

        return new Tuple<int, SExp>(Costs.IF_COST, r.First());
    }

    public static Tuple<int, SExp> OpCons(SExp args)
    {
        if (args.list_len() != 2)
        {
            throw new EvalError("c takes exactly 2 arguments", args);
        }

        return new Tuple<int, SExp>(Costs.CONS_COST, args.First().cons(args.Rest().First()));
    }

    public static Tuple<int, SExp> OpFirst(SExp args)
    {
        if (args.list_len() != 1)
        {
            throw new EvalError("f takes exactly 1 argument", args);
        }

        return new Tuple<int, SExp>(Costs.FIRST_COST, args.First().First());
    }

    public static Tuple<int, SExp> OpRest(SExp args)
    {
        if (args.list_len() != 1)
        {
            throw new EvalError("r takes exactly 1 argument", args);
        }

        return new Tuple<int, SExp>(Costs.REST_COST, args.First().Rest());
    }

    public static Tuple<int, SExp> OpListp(SExp args)
    {
        if (args.list_len() != 1)
        {
            throw new EvalError("l takes exactly 1 argument", args);
        }

        return new Tuple<int, SExp>(Costs.LISTP_COST, args.First().listp() ? args.True : args.False);
    }

    public static Tuple<int, SExp> OpRaise(SExp args)
    {
        if (args.list_len() == 1 && !args.First().listp())
        {
            throw new EvalError("clvm raise", args.First());
        }
        else
        {
            throw new EvalError("clvm raise", args);
        }
    }

    public static Tuple<int, SExp> OpEq(SExp args)
    {
        if (args.list_len() != 2)
        {
            throw new EvalError("= takes exactly 2 arguments", args);
        }

        SExp a0 = args.First();
        SExp a1 = args.Rest().First();

        if (a0.pair != null || a1.pair != null)
        {
            throw new EvalError("= on list", a0.pair != null ? a0 : a1);
        }

        byte[] b0 = a0.as_atom();
        byte[] b1 = a1.as_atom();

        int cost = Costs.EQ_BASE_COST;
        cost += (b0.Length + b1.Length) * Costs.EQ_COST_PER_BYTE;

        return new Tuple<int, SExp>(cost, b0.Equals(b1) ? args.True : args.False);
    }
}