using System.Numerics;
using System.Text;
using CLVMDotNet.CLVM;
using CLVMDotNet.Tools.IR;

namespace CLVMDotNet.Tools.Stages.Stage2;

public static class Operators
{
    public static Tuple<BigInteger, SExp> DoRead(SExp args)
    {
        var filename = args.First().AsAtom();
        string filepath = Encoding.UTF8.GetString(filename);
        string s = File.ReadAllText(filepath);
        var irSexp = SExp.To(IR.IRReader.ReadIR(s));
        var sexp = BinUtils.AssembleFromIR(irSexp);
        return new Tuple<BigInteger, SExp>(1, sexp);
    }
    
    public static Tuple<BigInteger, SExp> DoWrite(SExp args)
    {
        var filename = args.First().AsAtom();
        string filepath = Encoding.UTF8.GetString(filename);
        string s = File.ReadAllText(filepath);
        var irSexp = SExp.To(IR.IRReader.ReadIR(s));
        var sexp = BinUtils.AssembleFromIR(irSexp);
        return new Tuple<BigInteger, SExp>(1, sexp);
    }
}