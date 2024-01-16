using System.Collections;
using System.Numerics;
using System.Text;

namespace CLVMDotNet.Tools.IR;
public class IRReader
{
    public delegate CLVMObject TokenizeSexpDelegate(string token, int offset, IEnumerator<Token> stream);

    public delegate CLVMObject ReadIRDelegate(string s, Func<CLVMObject, byte[]> toSexp);

    public delegate CLVMObject TokenizeConsDelegate(string token, int offset, IEnumerator<Token> stream);

    public delegate Tuple<string, int> NextConsTokenDelegate(IEnumerator<Token> stream);

    public delegate Tuple<Tuple<IRType, int>, byte[]> TokenizeQuotesDelegate(string token, int offset);

    public delegate Tuple<Tuple<IRType, int>, byte[]> TokenizeSymbolDelegate(string token, int offset);

    public delegate bool TryParseIntDelegate(string token, out int result);

    public delegate bool TryParseHexDelegate(string token, out byte[] result);

    public delegate Stream TokenStreamDelegate(string s);


    public class Token
    {
        public string Value { get; set; }
        public int Offset { get; set; }
    }

    public class Stream : IEnumerable<Token>
    {
        private readonly string s;
        private int offset;

        public Stream(string s)
        {
            this.s = s;
            offset = 0;
        }

        public IEnumerator<Token> GetEnumerator()
        {
            while (offset < s.Length)
            {
                offset = ConsumeWhitespace(s, offset);
                if (offset >= s.Length)
                {
                    break;
                }

                char c = s[offset];
                if (c == '(' || c == ')')
                {
                    yield return new Token { Value = c.ToString(), Offset = offset };
                    offset++;
                    continue;
                }

                if (c == '"' || c == '\'')
                {
                    int start = offset;
                    char initialC = s[start];
                    offset++;
                    while (offset < s.Length && s[offset] != initialC)
                    {
                        offset++;
                    }

                    if (offset < s.Length)
                    {
                        yield return new Token { Value = s.Substring(start, offset - start + 1), Offset = start };
                        offset++;
                        continue;
                    }
                    else
                    {
                        throw new SyntaxException(
                            "unterminated string starting at " + start + ": " + s.Substring(start));
                    }
                }

                Tuple<string, int> tokenAndEndOffset = ConsumeUntilWhitespace(s, offset);
                yield return new Token { Value = tokenAndEndOffset.Item1, Offset = offset };
                offset = tokenAndEndOffset.Item2;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static int ConsumeWhitespace(string s, int offset)
    {
        while (true)
        {
            while (offset < s.Length && char.IsWhiteSpace(s[offset]))
            {
                offset++;
            }

            if (offset >= s.Length || s[offset] != ';')
            {
                break;
            }

            while (offset < s.Length && s[offset] != '\n' && s[offset] != '\r')
            {
                offset++;
            }
        }

        return offset;
    }

    public static Tuple<string, int> ConsumeUntilWhitespace(string s, int offset)
    {
        int start = offset;
        while (offset < s.Length && !char.IsWhiteSpace(s[offset]) && s[offset] != ')')
        {
            offset++;
        }

        return new Tuple<string, int>(s.Substring(start, offset - start), offset);
    }

    public static (string, int) NextConsToken(IEnumerator<(string,int)> stream)
    {
        string token = null;
        int offset = -1;

        stream.MoveNext();
        // foreach (var item in stream.)
        // {
        //     token = item.Item1;
        //     offset = item.Item2;
        //     break;
        // }

        token = stream.Current.Item1;
        offset = stream.Current.Item2;

        if (token == null)
        {
            throw new SyntaxException("Missing )");
        }

        return (token, offset);
    }

    public static dynamic? TokenizeCons(string token, int offset, IEnumerator<(string, int)> stream)
    {
        if (token == ")")
        {
            return Utils.IrNew(IRType.NULL, 0, offset);
        }

        int initialOffset = offset;
        var firstSexp = TokenizeSexp(token, offset, stream);

        var nextToken = NextConsToken(stream);
        token = nextToken.Item1;
        offset = nextToken.Item2;

        if (token == ".")
        {
            int dotOffset = offset;
            // grab the last item
            nextToken = NextConsToken(stream);
            token = nextToken.Item1;
            offset = nextToken.Item2;

            var restSexp = TokenizeSexp(token, offset, stream);

            nextToken = NextConsToken(stream);
            token = nextToken.Item1;
            offset = nextToken.Item2;

            if (token != ")")
            {
                throw new SyntaxException("illegal dot expression at " + dotOffset);
            }

            return Utils.IrCons(firstSexp, restSexp, initialOffset);
        }
        else
        {
            var restSexp = TokenizeCons(token, offset, stream);
            return Utils.IrCons(firstSexp, restSexp, initialOffset);
        }
    }


    public static dynamic? TokenizeSexp(string token, int offset, IEnumerator<(string, int)> stream)
    {
        if (token == "(")
        {
            var (t, o) = NextConsToken(stream);
            token = t;
            offset = o;
            return TokenizeCons(token, offset, stream);
        }
    
        var result = TokenizeInt(token, offset);
        if (result != null)
            return result;
        var hex  = TokenizeHex(token, offset);
        if (hex != null)
            return hex;
        var quotes = TokenizeQuotes(token, offset);
        if (quotes != null)
            return quotes;
        var symbol = TokenizeSymbol(token, offset);
        if (symbol != null)
            return symbol;

        return null;
    }

    public static SExp? TokenizeInt(string token, int offset)
    {
        if (int.TryParse(token, out int result))
        {
            return Utils.IrNew(IRType.INT, result, offset);
        }

        return null;
    }

    public static SExp? TokenizeHex(string token, int offset)
    {
        if (token.Length >= 2 && token.Substring(0, 2).ToUpper() == "0X")
        {
            string hexValue = token.Substring(2);
            if (hexValue.Length % 2 == 1)
            {
                hexValue = "0" + hexValue;
            }

            if (TryParseHex(hexValue, out byte[] result))
            {
                return Utils.IrNew(IRType.HEX, result, offset);
            }
            else
            {
                throw new SyntaxException("invalid hex at " + offset + ": 0x" + token);
            }
        }

        return null;
    }

    public static bool TryParseHex(string token, out byte[] result)
    {
        try
        {
            result = Enumerable.Range(0, token.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(token.Substring(x, 2), 16))
                .ToArray();
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    public static Tuple<Tuple<BigInteger, int>, byte[]>? TokenizeQuotes(string token, int offset)
    {
        if (token.Length >= 2)
        {
            char c = token[0];
            if (c == '\'' || c == '"')
            {
                if (token[token.Length - 1] == c)
                {
                    BigInteger qType = (c == '\'') ? IRType.SINGLE_QUOTE : IRType.DOUBLE_QUOTE;
                    byte[] value = Encoding.UTF8.GetBytes(token.Substring(1, token.Length - 2));
                    return new Tuple<Tuple<BigInteger, int>, byte[]>(new Tuple<BigInteger, int>(qType, offset), value);
                }
                else
                {
                    throw new SyntaxException("unterminated string starting at " + offset + ": " + token);
                }
            }
        }

        return null;
    }

    public static Tuple<(BigInteger, int), string>? TokenizeSymbol(string token, int offset)
    {
        return Tuple.Create((IRType.SYMBOL, offset),token);
    }

    public static IEnumerable<(string token, int offset)> TokenStream(string s)
    {
        int offset = 0;
        while (offset < s.Length)
        {
            offset = ConsumeWhitespace(s, offset);
            if (offset >= s.Length)
            {
                yield break;
            }

            char c = s[offset];
            if (c == '(' || c == ')')
            {
                yield return (c.ToString(), offset);
                offset++;
                continue;
            }

            if (c == '"' || c == '\'')
            {
                int start = offset;
                char initialC = s[start];
                offset++;
                while (offset < s.Length && s[offset] != initialC)
                {
                    offset++;
                }

                if (offset < s.Length)
                {
                    yield return (s.Substring(start, offset - start + 1), start);
                    offset++;
                    continue;
                }
                else
                {
                    throw new SyntaxException(
                        $"Unterminated string starting at {start}: {s.Substring(start)}"
                    );
                }
            }

            (string token, int endOffset) = ConsumeUntilWhitespace(s, offset);
            yield return (token, offset);
            offset = endOffset;
        }
    }

    public static SExp ReadIR(string s)
    {
        var stream = TokenStream(s);
        var enumerator = stream.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var item = enumerator.Current;
            var ts = TokenizeSexp(item.token, item.offset, enumerator);
            return SExp.To(ts);
        }
        
        throw new ArgumentException("unexpected end of stream");
    }
}