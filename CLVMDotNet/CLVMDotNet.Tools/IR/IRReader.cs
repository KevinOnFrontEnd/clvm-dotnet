using System.Data;
using System.Text;

namespace CLVMDotNet.Tools.IR;

public class IRReader
{
    private static int ConsumeWhitespace(string s, int offset)
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
    
    private static (string, int) ConsumeUntilWhitespace(string s, int offset)
    {
        int start = offset;
        while (offset < s.Length && !char.IsWhiteSpace(s[offset]) && s[offset] != ')')
        {
            offset++;
        }
        return (s.Substring(start, offset - start), offset);
    }

    public static (string, int) NextConsToken(Stream stream)
    {
        foreach ((string token, int offset) in stream)
        {
            return (token, offset);
        }

        throw new SyntaxErrorException("missing )");
    }
    
    public static IEnumerable<(string, int)> TokenStream(string s)
    {
        int offset = 0;
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
                    throw new SyntaxErrorException($"unterminated string starting at {start}: {s[start..]}");
                }
            }
            (string token, int endOffset) = ConsumeUntilWhitespace(s, offset);
            yield return (token, offset);
            offset = endOffset;
        }
    }
    
    public static (Type, int, byte[]) TokenizeSymbol(string token, int offset)
    {
        return (Type.SYMBOL, offset, Encoding.UTF8.GetBytes(token));
    }
    
    public static CLVMObject TokenizeHex(string token, int offset)
    {
        if (token.Length >= 2 && token.Substring(0, 2).ToUpper() == "0X")
        {
            try
            {
                token = token.Substring(2);
                if (token.Length % 2 == 1)
                {
                    token = "0" + token;
                }
                return IrNew(Type.HEX, HexStringToBytes(token), offset);
            }
            catch (Exception)
            {
                throw new SyntaxErrorException($"invalid hex at {offset}: 0x{token}");
            }
        }
        return null;
    }
    
    public static (Type, int, byte[])? TokenizeQuotes(string token, int offset)
    {
        if (token.Length < 2)
        {
            return null;
        }
        char c = token[0];
        if (c != '\'' && c != '"')
        {
            return null;
        }

        if (token[token.Length - 1] != c)
        {
            throw new SyntaxErrorException($"unterminated string starting at {offset}: {token}");
        }

        Type qType = (c == '\'') ? Type.SINGLE_QUOTE : Type.DOUBLE_QUOTE;

        return (qType, offset, Encoding.UTF8.GetBytes(token.Substring(1, token.Length - 2)));
    }


    private static byte[] HexStringToBytes(string hex)
    {
        int numberChars = hex.Length;
        byte[] bytes = new byte[numberChars / 2];
        for (int i = 0; i < numberChars; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }

    
    public static CLVMObject TokenizeSexp(string token, int offset, Stream stream)
    {
        if (token == "(")
        {
            (string nextToken, int nextOffset) = NextConsToken(stream);
            return TokenizeCons(nextToken, nextOffset, stream);
        }

        Func<string, int, CLVMObject>[] functions = new Func<string, int, CLVMObject>[]
        {
            TokenizeInt,
            TokenizeHex,
            TokenizeQuotes,
            TokenizeSymbol,
        };

        foreach (Func<string, int, CLVMObject> f in functions)
        {
            CLVMObject result = f(token, offset);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
    
    
    public static SExp ReadIr(string s)
    {
        var stream = TokenStream(s);

        foreach ((string token, int offset) in stream)
        {
            return toSexp(TokenizeSexp(token, offset, stream));
        }

        throw new SyntaxErrorException("unexpected end of stream");
    }
}