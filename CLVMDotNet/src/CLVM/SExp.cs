using System.Numerics;
using System.Security.Cryptography;
using System.Text;

//  SExp provides higher level API on top of any object implementing the CLVM
//  object protocol.
//  The tree of values is not a tree of SExp objects, it's a tree of CLVMObject
//  like objects. SExp simply wraps them to provide a uniform view of any
//  underlying conforming tree structure.
//
//  The CLVM object protocol (concept) exposes two attributes:
//  1. "atom" which is either None or bytes
//  2. "pair" which is either None or a tuple of exactly two elements. Both
// elements implementing the CLVM object protocol.
// Exactly one of "atom" and "pair" must be None.
namespace CLVMDotNet.CLVM
{

    public class SExp
    {
        public static SExp True { get; } = new SExp(new CLVMObject { Atom = new byte[] { 0x01 } });
        public static SExp False { get; } = new SExp(new CLVMObject());
        public static CLVMObject NULL { get; } = null;
        public byte[]? Atom { get; set; }
        public Tuple<dynamic, dynamic>? Pair { get; set; }

        public SExp(dynamic? obj)
        {
            Atom = obj?.Atom;
            if (obj?.Pair is (_, _))
                Pair = Tuple.Create<dynamic, dynamic>(obj?.Pair.Item1, obj?.Pair.Item2);
            else
                Pair = null;
        }

        public SExp()
        {
        }

        public Tuple<SExp, SExp>? AsPair()
        {
            if (Pair == null)
            {
                return null;
            }

            var left = Pair.Item1 is SExp ? Pair.Item1 : new SExp(Pair.Item1);
            var right = Pair.Item2 is SExp ? Pair.Item2 : new SExp(Pair.Item2);
            return Tuple.Create<SExp, SExp>(left, right);
        }

        public byte[]? AsAtom()
        {
            return Atom;
        }

        public SExp Cons(SExp right)
        {
            return To(new Tuple<dynamic, dynamic>(this, right));
        }

        public dynamic First()
        {
            if (Pair is (_,_))
            {
                return new SExp(Pair.Item1);
            }

            throw new EvalError("first of non-cons", this);
        }

        public dynamic Rest()
        {
            if (Pair is (_,_))
            {
                return new SExp(Pair.Item2);
            }

            throw new EvalError("rest of non-cons", this);
        }

        public bool Listp()
        {
            return Pair != null;
        }

        public int ListLength()
        {
            SExp v = this;
            int size = 0;

            while (v.Listp())
            {
                size++;
                v = v.Rest();
            }

            return size;
        }

        public static SExp To(dynamic? v)
        {
            if (v is SExp se)
            {
                return se;
            }

            if (HelperFunctions.LooksLikeCLVMObject(v))
            {
                return new SExp(v);
            }

            var sexp = new SExp(HelperFunctions.ToSexpType(v));
            return sexp;
        }

        public BigInteger AsInt()
        {
            if (Atom != null)
            {
                return Casts.IntFromBytes(Atom!);
            }
            else
            {
                // Handle the case where atom is null (or empty)
                throw new InvalidOperationException("The atom property is null or empty.");
            }
        }

        public IEnumerable<SExp> AsIter()
        {
            var v = this;
            while (!v.Nullp())
            {
                yield return v.First();
                v = v.Rest();
            }
        }

        public bool Nullp()
        {
            byte[]? v = Atom;
            return v is { Length: 0 };
        }

        public byte[] AsBin()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serialize.SexpToStream(this, stream);
                return stream.ToArray();
            }
        }

        public override string ToString()
        {
            return BitConverter.ToString(AsBin()).Replace("-", "");
        }

        public bool Equals(dynamic other)
        {
            try
            {
                var otherObj = SExp.To(other);
                Stack<(SExp, SExp)> toCompareStack = new Stack<(SExp, SExp)>();
                toCompareStack.Push((this, otherObj));

                while (toCompareStack.Count > 0)
                {
                    var (s1, s2) = toCompareStack.Pop();
                    var p1 = s1.AsPair();

                    if (p1 != null)
                    {
                        var p2 = s2.AsPair();

                        if (p2 != null)
                        {
                            toCompareStack.Push((p1.Item1, p2.Item1));
                            toCompareStack.Push((p1.Item2, p2.Item2));
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (s2.AsPair() != null || !s1.AsAtom().SequenceEqual(s2.AsAtom()))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public static byte[] ConvertAtomToBytes(object v)
        {
            if (v is byte[] byteArray)
            {
                return byteArray;
            }
            else if (v is string str)
            {
                return Encoding.UTF8.GetBytes(str);
            }
            else if (v is int intValue)
            {
                return BitConverter.GetBytes(intValue);
            }
            else if (v is null)
            {
                return new byte[0];
            }
            else if (v is List<object> list)
            {
                var result = new List<byte>();
                foreach (var item in list)
                {
                    result.AddRange(ConvertAtomToBytes(item));
                }

                return result.ToArray();
            }

            throw new ArgumentException($"Can't cast {v.GetType()} ({v}) to bytes");
        }
    }
}