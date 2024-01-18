using clvm = CLVMDotNet.CLVM;

namespace CLVMDotNet.Tests
{
    public static class Common
    {
        #region test helpers that should probably go into SExp object

        public static string PrintLeaves(clvm.SExp tree)
        {
            var a = tree.AsAtom();
            if (a != null)
            {
                if (a.Length == 0)
                    return "() ";

                return $"{a[0]} ";
            }

            var ret = "";
            var pairs = tree.AsPair();
            var list = new List<clvm.SExp>() { pairs.Item1, pairs.Item2 };
            if (pairs != null)
            {
                foreach (clvm.SExp i in list)
                {
                    ret += PrintLeaves(i);
                }
            }

            return ret;
        }

        public static string PrintTree(clvm.SExp tree)
        {
            var a = tree.AsAtom();
            if (a != null)
            {
                if (a.Length == 0)
                {
                    return "() ";
                }

                return $"{a[0]} ";
            }

            var ret = "(";
            var pairs = tree.AsPair();
            var list = new List<clvm.SExp>() { pairs.Item1, pairs.Item2 };
            if (pairs != null)
            {
                foreach (var i in list)
                {
                    ret += PrintTree(i);
                }
            }

            ret += ")";
            return ret;
        }

        public static void ValidateSExp(clvm.SExp sexp)
        {
            Stack<clvm.SExp> validateStack = new Stack<clvm.SExp>();
            validateStack.Push(sexp);

            while (validateStack.Count > 0)
            {
                dynamic v = validateStack.Pop();

                if (!(v is clvm.SExp))
                {
                    throw new InvalidOperationException("v is not an instance of SExp");
                }

                if (v.Pair != null)
                {
                    if (v.Pair.GetType() != typeof(Tuple<object, object>))
                    {
                        throw new InvalidOperationException("v.pair is not a Tuple");
                    }

                    Tuple<dynamic, dynamic> pair = v.Pair;

                    if (!clvm.HelperFunctions.LooksLikeCLVMObject(pair.Item1) ||
                        !clvm.HelperFunctions.LooksLikeCLVMObject(pair.Item2))
                    {
                        throw new InvalidOperationException("One or both elements do not look like CLVM objects");
                    }

                    var sPair = v.AsPair();
                    validateStack.Push(sPair.Item1);
                    validateStack.Push(sPair.Item2);
                }
                else
                {
                    if (!(v.Atom is byte[]))
                    {
                        throw new InvalidOperationException("v.atom is not a byte array");
                    }
                }
            }
        }

        #endregion
    }
}