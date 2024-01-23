using System.Numerics;

namespace CLVMDotNet.CLVM;

public static class Program
{
    public static Tuple<BigInteger, SExp> RunProgram(SExp program, SExp args, BigInteger? maxCost = null)
    {
        var prog = SExp.To(program);
        Stack<Func<Stack<SExp>, BigInteger>> opStack = new Stack<Func<Stack<SExp>, BigInteger>>();

        var valueStack = new Stack<SExp>();
        valueStack.Push(args);
        opStack.Push(stack => EvalOp(opStack, valueStack));

        BigInteger cost = 0;

        while (opStack.Count != 0)
        {
            var f = opStack.Pop();
            cost += f(valueStack);
            if (maxCost.HasValue && cost > maxCost)
            {
                throw new EvalError("cost exceeded", SExp.To(maxCost));
            }
        }

        return Tuple.Create(new BigInteger(1), prog);
    }
    
    public static (BigInteger, SExp) TraversePath(SExp sexp, SExp env)
    {
        BigInteger cost = Costs.PATH_LOOKUP_BASE_COST;
        cost += Costs.PATH_LOOKUP_COST_PER_LEG;

        if (sexp.Nullp())
        {
            return (cost, SExp.NULL);
        }

        byte[] b = sexp.Atom;

        int endByteCursor = 0;
        while (endByteCursor < b.Length && b[endByteCursor] == 0)
        {
            endByteCursor++;
        }

        cost += endByteCursor * Costs.PATH_LOOKUP_COST_PER_ZERO_BYTE;
        if (endByteCursor == b.Length)
        {
            return (cost, SExp.NULL);
        }

        // Create a bitmask for the most significant *set* bit
        // in the last non-zero byte
        byte endBitmask = MSBMask(b[endByteCursor]);
        int byteCursor = b.Length - 1;
        byte bitmask = 0x01;
        while (byteCursor > endByteCursor || bitmask < endBitmask)
        {
            if (env.Pair == null)
            {
                throw new EvalError("path into atom", env);
            }

            if ((b[byteCursor] & bitmask) != 0)
            {
                env = env.Rest();
            }
            else
            {
                env = env.First();
            }

            cost += Costs.PATH_LOOKUP_COST_PER_LEG;
            bitmask <<= 1;

            if (bitmask == 0x100)
            {
                byteCursor--;
                bitmask = 0x01;
            }
        }
        return (cost, env);
    }
    
    public static BigInteger EvalOp(Stack<Func<Stack<SExp>, BigInteger>> opStack, Stack<SExp> valueStack)
    {
        var pair = valueStack.Pop();
        var sexp = pair.First();
        var args = pair.Rest();

        if (sexp.Pair == null)
        {
            // sexp is an atom
            var (cost, r) = TraversePath(sexp, args);
            valueStack.Push(r);
            return cost;
        }
        
        var op = sexp.First();
        if (op.Pair != null)
        {
            var operatorPair = op.AsPair(); //newOperator,must_be_nil
            var (newOperator, mustBeNil) = (operatorPair.Item1, operatorPair.Item2);
            if (newOperator.Pair != null || mustBeNil.Atom != Array.Empty<byte>())
            {
                throw new EvalError("in ((X)...) syntax X must be lone atom", sexp);
            }

            var newOperandList = sexp.Rest();
            valueStack.Append(newOperator);
            valueStack.Append(newOperandList);
            opStack.Push(stack => ApplyOp(opStack, valueStack));
            return Costs.APPLY_COST;
        }

        var op1 = op.AsAtom();
        var operand_list = sexp.Rest();
        if (op1 == Operator.QuoteAtom)
        {
            valueStack.Append(operand_list);
            return Costs.QUOTE_COST;
        }

        opStack.Append(stack => ConsOp(opStack, valueStack));
        valueStack.Append(op);
        while (operand_list.Nullp())
        {
            var ex = operand_list.First();
            valueStack.Append(ex.Cons(args));
            opStack.Push(stack => ConsOp(opStack, valueStack));
            opStack.Push(stack => EvalOp(opStack, valueStack));
            opStack.Push(stack => SwapOp(opStack, valueStack));
            operand_list = operand_list.Rest();
            valueStack.Append(SExp.NULL);
        }
        return BigInteger.Parse("1");
    }

    public static BigInteger ApplyOp(Stack<Func<Stack<SExp>, BigInteger>> opStack, Stack<SExp> valueStack)
    {
        var operandList = valueStack.Pop();
        var oper = valueStack.Pop();

        if (oper.Pair != null)
        {
            throw new EvalError("internal error", oper);
        }

        var op = oper.AsAtom();

        if (op == Operator.ApplyAtom)
        {
            if (operandList.ListLength() != 2)
            {
                throw new EvalError("apply requires exactly 2 parameters", operandList);
            }

            var newProgram = operandList.First();
            var newArgs = operandList.Rest().First();

            valueStack.Push(newProgram.Cons(newArgs));
            opStack.Push(stack => EvalOp(opStack, valueStack));
            return Costs.APPLY_COST;
        }

        var result = Operator.ApplyOperator(op, operandList);
        valueStack.Push(result.Item2);
        return result.Item1;
    }

    public static BigInteger SwapOp(Stack<Func<Stack<SExp>, BigInteger>> opStack, Stack<SExp> valueStack)
    {
        var v2 = valueStack.Pop();
        var v1 = valueStack.Pop();
        valueStack.Push(v2);
        valueStack.Push(v1);
        return 0;
    }

    public static int ConsOp(Stack<Func<Stack<SExp>, BigInteger>> opStack, Stack<SExp> valueStack)
    {
        var v1 = valueStack.Pop();
        var v2 = valueStack.Pop();
        var result = v1.Cons(v2);
        valueStack.Push(result);
        return 0;
    }

    public static byte MSBMask(byte inputByte)
    {
        inputByte |= (byte)(inputByte >> 1);
        inputByte |= (byte)(inputByte >> 2);
        inputByte |= (byte)(inputByte >> 4);
        return (byte)((inputByte + 1) >> 1);
    }
}