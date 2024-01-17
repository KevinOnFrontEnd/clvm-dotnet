using System.Data;

namespace CLVMDotNet.Tools.IR;

public static class IRWRiter
{
    public static IEnumerable<string> IterSexpFormat(SExp irSexp)
    {
        yield return "(";
        bool isFirst = true;
        while (!Utils.IrNullp(irSexp))
        {
            if (!Utils.IrListp(irSexp))
            {
                yield return " . ";
                foreach (var item in IterIrFormat(irSexp))
                {
                    yield return item;
                }

                yield break;
            }

            if (!isFirst)
            {
                yield return " ";
            }

            foreach (var item in IterIrFormat(Utils.IrFirst(irSexp)))
            {
                yield return item;
            }

            irSexp = Utils.IrRest(irSexp);
            isFirst = false;
        }

        yield return ")";
    }

    public static IEnumerable<string> IterIrFormat(SExp irSexp)
    {
        if (Utils.IrListp(irSexp))
        {
            foreach (var item in IterSexpFormat(irSexp))
            {
                yield return item;
            }

            yield break;
        }

        var type = Utils.IrType(irSexp);

        if (type == IRType.CODE)
        {
            using (var bio = new MemoryStream())
            {
                Serialize.SexpToStream(Utils.IrVal(irSexp), bio);
                var code = BitConverter.ToString(bio.ToArray()).Replace("-", "");
                yield return $"CODE[{code}]";
            }

            yield break;
        }

        if (type == IRType.NULL)
        {
            yield return "()";
            yield break;
        }

        var atom = Utils.IrAsAtom(irSexp);

        if (type == IRType.INT)
        {
            yield return $"{Casts.IntFromBytes(atom)}";
        }
        else if (type == IRType.NODE)
        {
            yield return $"NODE[{Casts.IntFromBytes(atom)}]";
        }
        else if (type == IRType.HEX)
        {
            yield return $"0x{BitConverter.ToString(atom).Replace("-", "")}";
        }
        else if (type == IRType.QUOTES)
        {
            yield return $"\"{System.Text.Encoding.UTF8.GetString(atom)}\"";
        }
        else if (type == IRType.DOUBLE_QUOTE)
        {
            yield return $"\"{System.Text.Encoding.UTF8.GetString(atom)}\"";
        }
        else if (type == IRType.SINGLE_QUOTE)
        {
            yield return $"'{System.Text.Encoding.UTF8.GetString(atom)}'";
        }
        else if (new[] { IRType.SYMBOL, IRType.OPERATOR }.Contains(type))
        {
            string? val = null;
            string? error = null;
            try
            {
                 val = System.Text.Encoding.UTF8.GetString(atom);
            }
            catch (Exception)
            {
                error = $"(indecipherable symbol: {BitConverter.ToString(atom)})";
            }

            if (!string.IsNullOrEmpty(error))
                yield return error;
            else
            {
                yield return val!;
            }
        }
        else
        {
            throw new SyntaxErrorException($"bad ir format: {irSexp}");
        }
    }

    public static void WriteIrToStream(SExp irSexp, Stream stream)
    {
        using (var writer = new StreamWriter(stream))
        {
            foreach (var item in IterIrFormat(irSexp))
            {
                writer.Write(item);
            }
        }
    }

    public static string WriteIr(SExp irSexp)
    {
        using (var stream = new MemoryStream())
        {
            WriteIrToStream(irSexp, stream);
            return System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}