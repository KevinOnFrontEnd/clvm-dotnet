using System.Data;
using System.Numerics;
using System.Text;
using CLVMDotNet.CLVM;
using x = CLVMDotNet.CLVM;

namespace CLVMDotNet.Tools.IR
{
    public static class BinUtils
    {
        public const string printable_chars =
            "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!#$%&'()*+,-./:;<=>?@[]^_`{|}~ ";

        public static SExp AssembleFromIR(SExp ir_sexp)
        {
            string keyword = Utils.IrAsSymbol(ir_sexp) as string;
            if (!string.IsNullOrEmpty(keyword))
            {
                if (keyword.StartsWith("#"))
                {
                    keyword = keyword.Substring(1);
                }

                var atom = x.Keywords.KEYWORD_TO_ATOM[keyword];
                if (atom != null)
                {
                    return SExp.To(atom);
                }

                if (true)
                {
                    return Utils.IrVal(ir_sexp);
                }
            }

            if (!Utils.IrListp(ir_sexp))
            {
                return Utils.IrVal(ir_sexp);
            }

            if (Utils.IrNullp(ir_sexp))
            {
                return SExp.To(new List<object>());
            }

            // handle "q"
            var first = Utils.IrFirst(ir_sexp);
            var keyword2 = Utils.IrAsSymbol(first) as string;
            if (keyword2 == "q")
            {
                // TODO: note that any symbol is legal after this point
            }

            var sexp1 = AssembleFromIR(first);
            var sexp2 = AssembleFromIR(Utils.IrRest(ir_sexp));
            return sexp1.Cons(sexp2);
        }

        public static SExp Assemble(string s)
        {
            var symbols = IRReader.ReadIR(s);
            var assembled = AssembleFromIR(symbols);
            return assembled;
        }

        public static BigInteger TypeForAtom(byte[] atom)
        {
            if (atom.Length > 2)
            {
                try
                {
                    string strValue = Encoding.UTF8.GetString(atom);
                    if (strValue.All(c => printable_chars.Contains(c)))
                    {
                        return IRType.QUOTES;
                    }
                }
                catch (Exception)
                {
                }

                return IRType.HEX;
            }

 
            if(x.Casts.IntToBytes(x.Casts.IntFromBytes(atom)).SequenceEqual(atom))
            {
                return IRType.INT;
            }

            return IRType.HEX;
        }

        //Disassemble
        //       disassemble_to_ir
    }
}