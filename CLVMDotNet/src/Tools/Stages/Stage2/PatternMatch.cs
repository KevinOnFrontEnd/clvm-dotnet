using System.Text;
using CLVMDotNet.CLVM;

namespace CLVMDotNet.Tools.Stages.Stage2;

public class PatternMatch
{
    public static byte[] ATOM_MATCH => Encoding.UTF8.GetBytes("$");
    public static byte[] SEXP_MATCH => Encoding.UTF8.GetBytes(":");
    
    public static Dictionary<string, SExp>? UnifyBindings(Dictionary<string, SExp> bindings, byte[] newKey,
        SExp newValue)
    {
        string newKeyString = System.Text.Encoding.UTF8.GetString(newKey);
        if (bindings.ContainsKey(newKeyString))
        {
            var obj = bindings[newKeyString];
            if (!obj.Equals(newValue))
            {
                return null;
            }

            return bindings;
        }

        Dictionary<string, SExp> newBindings = new Dictionary<string, SExp>(bindings);
        newBindings[newKeyString] = newValue;
        return newBindings;
    }

    public static Dictionary<string, SExp>? Match(SExp pattern, SExp sexp, Dictionary<string, SExp> knownBindings = null)
    {
        if (knownBindings == null)
        {
            knownBindings = new Dictionary<string, SExp>();
        }

        if (!pattern.Listp())
        {
            if (!sexp.Listp())
            {
                return pattern.AsAtom() == sexp.AsAtom() ? knownBindings : null;
            }

            return null;
        }

        var left = pattern.First();
        var right = pattern.Rest();
        object atom = sexp.AsAtom();

        if (left.AsAtom() == ATOM_MATCH)
        {
            if (!sexp.Listp())
            {
                return null;
            }

            if (right.AsAtom() == ATOM_MATCH)
            {
                return atom is string ? UnifyBindings(knownBindings, right.AsAtom(), sexp) : null;
            }

            return UnifyBindings(knownBindings, right.AsAtom(), sexp);
        }

        if (left.AsAtom() == SEXP_MATCH)
        {
            if (right.AsAtom() == SEXP_MATCH)
            {
                return atom is string ? UnifyBindings(knownBindings, right.AsAtom(), sexp) : null;
            }

            return UnifyBindings(knownBindings, right.AsAtom(), sexp);
        }

        if (!sexp.Listp())
        {
            return null;
        }

        Dictionary<string, SExp> newBindings = Match(left, sexp.First(), knownBindings);
        if (newBindings == null)
        {
            return newBindings;
        }
        return Match(right, sexp.Rest(), newBindings);
    }
}