namespace clvm_dotnet;


public class EvalError : Exception
{
    public SExp SExpression { get; private set; }

    public EvalError(string message, SExp sexp) : base(message)
    {
        SExpression = sexp;
    }
}